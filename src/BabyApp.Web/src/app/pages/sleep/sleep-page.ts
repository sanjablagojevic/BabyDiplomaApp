import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { timer } from 'rxjs';
import { BabyApiService, NapPlanDto, SleepSessionDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-sleep-page',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './sleep-page.html',
  styleUrl: './sleep-page.scss',
})
export class SleepPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly plan = signal<NapPlanDto | null>(null);
  readonly planErr = signal('');
  readonly sessions = signal<SleepSessionDto[]>([]);
  readonly audio = signal<{ title: string; kind: string; url: string | null }[]>([]);
  /** Trenutno vrijeme za odbrojavanje / traku napretka */
  readonly nowMs = signal(Date.now());

  wakeForm = this.fb.nonNullable.group({
    forDate: [new Date().toISOString().slice(0, 10), Validators.required],
    morningWakeTime: ['07:00', Validators.required],
  });

  sessionForm = this.fb.nonNullable.group({
    start: ['', Validators.required],
    end: [''],
    isNap: [true],
    notes: [''],
  });

  readonly wakeInsight = computed(() => {
    this.nowMs();
    const p = this.plan();
    const sessions = this.sessions();
    if (!p) return null;

    const latest = this.latestSession(sessions);
    const asleep = p.isBabyAsleepNow || (latest !== null && !latest.endUtc);

    if (asleep) {
      return { kind: 'asleep' as const };
    }

    const lastWake = this.resolveLastWakeUtc(p, sessions);
    if (!lastWake) {
      return {
        kind: 'need_wake' as const,
        min: p.wakeWindowMinMinutes,
        max: p.wakeWindowMaxMinutes,
        band: p.wakeWindowBandLabel,
      };
    }

    const t0 = lastWake.getTime();
    const minMs = p.wakeWindowMinMinutes * 60_000;
    const maxMs = p.wakeWindowMaxMinutes * 60_000;
    const typicalMs = p.suggestedWakeWindowMinutes * 60_000;
    const now = Date.now();

    const suggestedAt = new Date(t0 + typicalMs);
    const windowStart = new Date(t0 + minMs);
    const windowEnd = new Date(t0 + maxMs);

    const elapsed = now - t0;
    const progressTypical = Math.min(1, Math.max(0, elapsed / typicalMs));
    const progressMax = Math.min(1, Math.max(0, elapsed / maxMs));

    const untilSuggested = Math.max(0, Math.round((suggestedAt.getTime() - now) / 60_000));
    const untilWindowEnd = Math.max(0, Math.round((windowEnd.getTime() - now) / 60_000));
    const overdueSuggested = now > suggestedAt.getTime();

    return {
      kind: 'awake' as const,
      lastWake,
      suggestedAt,
      windowStart,
      windowEnd,
      progressTypical,
      progressMax,
      progressTypicalPct: Math.round(Math.min(100, progressTypical * 100)),
      progressMaxPct: Math.round(Math.min(100, progressMax * 100)),
      untilSuggestedMinutes: untilSuggested,
      untilWindowEndMinutes: untilWindowEnd,
      overdueSuggested,
      min: p.wakeWindowMinMinutes,
      max: p.wakeWindowMaxMinutes,
      typical: p.suggestedWakeWindowMinutes,
      band: p.wakeWindowBandLabel,
    };
  });

  ngOnInit(): void {
    timer(0, 1000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.nowMs.set(Date.now()));

    const id = this.ctx.babyId();
    if (!id) return;
    this.api.sleepAudio().subscribe({ next: (a) => this.audio.set(a) });
    this.refreshSessions();
    this.loadPlan();
  }

  saveWake(): void {
    const id = this.ctx.babyId();
    if (!id || this.wakeForm.invalid) return;
    const { forDate, morningWakeTime } = this.wakeForm.getRawValue();
    const t = morningWakeTime.length === 5 ? `${morningWakeTime}:00` : morningWakeTime;
    this.api.logWake(id, forDate, t).subscribe({
      next: () => this.loadPlan(),
      error: () => this.planErr.set('Spremanje buđenja nije uspjelo.'),
    });
  }

  loadPlan(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.planErr.set('');
    const d = this.wakeForm.get('forDate')?.value;
    this.api.napPlan(id, d).subscribe({
      next: (p) => {
        this.plan.set(p);
        this.planErr.set('');
      },
      error: (e: { error?: { message?: string; title?: string } }) => {
        this.plan.set(null);
        this.planErr.set(
          e?.error?.message ?? e?.error?.title ?? 'Unesi jutarnje buđenje za taj dan.',
        );
      },
    });
  }

  refreshSessions(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const from = new Date();
    from.setDate(from.getDate() - 14);
    this.api.sleepSessions(id, from.toISOString()).subscribe({ next: (s) => this.sessions.set(s) });
  }

  private refreshSessionsAndPlan(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const from = new Date();
    from.setDate(from.getDate() - 14);
    this.api.sleepSessions(id, from.toISOString()).subscribe({
      next: (s) => {
        this.sessions.set(s);
        this.loadPlan();
      },
    });
  }

  addSession(): void {
    const id = this.ctx.babyId();
    if (!id || this.sessionForm.invalid) return;
    const v = this.sessionForm.getRawValue();
    const start = new Date(v.start);
    const end = v.end ? new Date(v.end) : null;
    this.api
      .addSleepSession(id, {
        startUtc: start.toISOString(),
        endUtc: end ? end.toISOString() : null,
        isNap: v.isNap,
        notes: v.notes || null,
      })
      .subscribe({ next: () => this.refreshSessionsAndPlan() });
  }

  removeSession(s: SleepSessionDto): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.deleteSleepSession(id, s.id).subscribe({ next: () => this.refreshSessionsAndPlan() });
  }

  formatDurationMinutes(total: number): string {
    if (total < 60) return `${total} min`;
    const h = Math.floor(total / 60);
    const m = total % 60;
    return m === 0 ? `${h} h` : `${h} h ${m} min`;
  }

  private latestSession(sessions: SleepSessionDto[]): SleepSessionDto | null {
    if (sessions.length === 0) return null;
    return [...sessions].sort(
      (a, b) => new Date(b.startUtc).getTime() - new Date(a.startUtc).getTime(),
    )[0];
  }

  private resolveLastWakeUtc(p: NapPlanDto, sessions: SleepSessionDto[]): Date | null {
    const sorted = [...sessions].sort(
      (a, b) => new Date(b.startUtc).getTime() - new Date(a.startUtc).getTime(),
    );
    if (sorted.length > 0) {
      const latest = sorted[0];
      if (latest.endUtc) return new Date(latest.endUtc);
      const prev = sorted.slice(1).find((s) => !!s.endUtc);
      if (prev?.endUtc) return new Date(prev.endUtc);
    }
    if (p.lastSleepEndUtc) return new Date(p.lastSleepEndUtc);
    return this.morningWakeLocal(p);
  }

  /** Jutarnje buđenje za odabrani dan (lokalno vrijeme preglednika). */
  private morningWakeLocal(p: NapPlanDto): Date | null {
    const t = p.morningWake?.length >= 5 ? p.morningWake.slice(0, 5) : p.morningWake;
    const d = p.date;
    if (!d || !t) return null;
    const dt = new Date(`${d}T${t}:00`);
    return Number.isNaN(dt.getTime()) ? null : dt;
  }
}
