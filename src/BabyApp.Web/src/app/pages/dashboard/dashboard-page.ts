import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  BabyApiService,
  DashboardDto,
  DiaperLogDto,
  FeedingDto,
  ReminderDto,
  SleepSessionDto,
} from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [DecimalPipe, DatePipe, RouterLink],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);

  readonly data = signal<DashboardDto | null>(null);
  readonly sleepRows = signal<SleepSessionDto[]>([]);
  readonly feedingRows = signal<FeedingDto[]>([]);
  readonly diaperRows = signal<DiaperLogDto[]>([]);
  readonly reminderRows = signal<ReminderDto[]>([]);
  readonly insights = signal<string[]>([]);
  readonly nextNap = signal<string | null>(null);
  readonly err = signal('');

  ngOnInit(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const from = new Date();
    from.setDate(from.getDate() - 7);
    const today = new Date().toISOString().slice(0, 10);

    forkJoin({
      d: this.api.dashboard(id),
      sleep: this.api.sleepSessions(id, from.toISOString()),
      feeding: this.api.feedingLogs(id, from.toISOString()),
      diapers: this.api.diaperLogs(id, from.toISOString()),
      reminders: this.api.reminders(id),
      insights: this.api.insights(id),
      napPlan: this.api.napPlan(id, today),
    }).subscribe({
      next: ({ d, sleep, feeding, diapers, reminders, insights, napPlan }) => {
        this.data.set(d);
        this.sleepRows.set(sleep);
        this.feedingRows.set(feeding);
        this.diaperRows.set(diapers);
        this.reminderRows.set(reminders);
        this.insights.set(insights);
        this.nextNap.set(napPlan.naps.length ? napPlan.naps[0].suggestedStartLocal : null);
      },
      error: () => {
        // Nap plan može pasti ako nije upisano jutarnje buđenje; u tom slučaju učitaj ostalo.
        forkJoin({
          d: this.api.dashboard(id),
          sleep: this.api.sleepSessions(id, from.toISOString()),
          feeding: this.api.feedingLogs(id, from.toISOString()),
          diapers: this.api.diaperLogs(id, from.toISOString()),
          reminders: this.api.reminders(id),
          insights: this.api.insights(id),
        }).subscribe({
          next: ({ d, sleep, feeding, diapers, reminders, insights }) => {
            this.data.set(d);
            this.sleepRows.set(sleep);
            this.feedingRows.set(feeding);
            this.diaperRows.set(diapers);
            this.reminderRows.set(reminders);
            this.insights.set(insights);
            this.nextNap.set(null);
          },
          error: () => this.err.set('Ne mogu učitati pregled.'),
        });
      },
    });
  }

  todaySleepMinutes(): number {
    return this.sleepRows()
      .filter((s) => this.isToday(s.startUtc) && s.endUtc)
      .reduce((sum, s) => {
        const diff = new Date(s.endUtc!).getTime() - new Date(s.startUtc).getTime();
        return sum + Math.max(0, Math.round(diff / 60000));
      }, 0);
  }

  todayFeedingCount(): number {
    return this.feedingRows().filter((x) => this.isToday(x.startUtc)).length;
  }

  todayDiapersCount(): number {
    return this.diaperRows().filter((x) => this.isToday(x.occurredUtc)).length;
  }

  nextReminder(): ReminderDto | null {
    const enabled = this.reminderRows().filter((r) => r.isEnabled);
    if (!enabled.length) return null;
    const nowM = this.minutesNow();
    const sorted = [...enabled].sort((a, b) => this.hmToMinutes(a.localTime) - this.hmToMinutes(b.localTime));
    const next = sorted.find((r) => this.hmToMinutes(r.localTime) >= nowM);
    return next ?? sorted[0];
  }

  timeline(): { at: Date; text: string }[] {
    const items: { at: Date; text: string }[] = [];
    this.sleepRows()
      .slice(0, 10)
      .forEach((s) => {
        items.push({ at: new Date(s.startUtc), text: `San: ${s.isNap ? 'Dremka' : 'Noćni san'} počeo` });
      });
    this.feedingRows()
      .slice(0, 10)
      .forEach((f) => {
        items.push({ at: new Date(f.startUtc), text: `Hrana: ${f.type}${f.amountMl ? ` (${f.amountMl} ml)` : ''}` });
      });
    this.diaperRows()
      .slice(0, 10)
      .forEach((d) => {
        items.push({ at: new Date(d.occurredUtc), text: `Pelena: ${d.type}` });
      });
    return items.sort((a, b) => b.at.getTime() - a.at.getTime()).slice(0, 12);
  }

  formatMinutes(min: number): string {
    if (min < 60) return `${min} min`;
    const h = Math.floor(min / 60);
    const m = min % 60;
    return m ? `${h}h ${m}min` : `${h}h`;
  }

  private isToday(iso: string): boolean {
    return new Date(iso).toDateString() === new Date().toDateString();
  }

  private hmToMinutes(localTime: string): number {
    const hm = localTime.slice(0, 5).split(':');
    return Number(hm[0]) * 60 + Number(hm[1]);
  }

  private minutesNow(): number {
    const n = new Date();
    return n.getHours() * 60 + n.getMinutes();
  }
}
