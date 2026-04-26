import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
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

  readonly plan = signal<NapPlanDto | null>(null);
  readonly planErr = signal('');
  readonly sessions = signal<SleepSessionDto[]>([]);
  readonly audio = signal<{ title: string; kind: string; url: string | null }[]>([]);

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

  ngOnInit(): void {
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
      .subscribe({ next: () => this.refreshSessions() });
  }

  removeSession(s: SleepSessionDto): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.deleteSleepSession(id, s.id).subscribe({ next: () => this.refreshSessions() });
  }
}
