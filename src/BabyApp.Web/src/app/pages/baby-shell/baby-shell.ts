import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { BabyApiService, ReminderDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-baby-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './baby-shell.html',
  styleUrl: './baby-shell.scss',
})
export class BabyShellComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly ctx = inject(BabyContextService);
  private readonly api = inject(BabyApiService);
  private sub?: Subscription;
  private reminderPoller?: number;
  private reminderMinuteTick?: number;
  private readonly dismissedKey = 'babyapp_reminder_dismissed';
  babyId = 0;
  dueReminders: ReminderDto[] = [];

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe((pm: ParamMap) => {
      const id = Number(pm.get('babyId'));
      this.babyId = id;
      this.ctx.setBabyId(id);
      this.loadReminders();
    });

    this.reminderMinuteTick = window.setInterval(() => this.evaluateDueReminders(), 15000);
    this.reminderPoller = window.setInterval(() => this.loadReminders(), 60000);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    if (this.reminderMinuteTick) window.clearInterval(this.reminderMinuteTick);
    if (this.reminderPoller) window.clearInterval(this.reminderPoller);
  }

  dismissReminder(reminderId: number): void {
    const key = this.todayKey();
    const map = this.getDismissedMap();
    map[`${key}|${reminderId}`] = true;
    localStorage.setItem(this.dismissedKey, JSON.stringify(map));
    this.dueReminders = this.dueReminders.filter((r) => r.id !== reminderId);
  }

  private remindersCache: ReminderDto[] = [];

  private loadReminders(): void {
    if (!this.babyId) return;
    this.api.reminders(this.babyId).subscribe({
      next: (rows) => {
        this.remindersCache = rows;
        this.evaluateDueReminders();
      },
    });
  }

  private evaluateDueReminders(): void {
    const now = new Date();
    const hh = String(now.getHours()).padStart(2, '0');
    const mm = String(now.getMinutes()).padStart(2, '0');
    const nowHm = `${hh}:${mm}`;
    const day = this.todayKey();
    const dismissed = this.getDismissedMap();

    this.dueReminders = this.remindersCache.filter((r) => {
      if (!r.isEnabled) return false;
      const local = r.localTime.slice(0, 5);
      if (local !== nowHm) return false;
      return !dismissed[`${day}|${r.id}`];
    });
  }

  private todayKey(): string {
    const now = new Date();
    const yy = now.getFullYear();
    const mm = String(now.getMonth() + 1).padStart(2, '0');
    const dd = String(now.getDate()).padStart(2, '0');
    return `${yy}-${mm}-${dd}`;
  }

  private getDismissedMap(): Record<string, boolean> {
    const raw = localStorage.getItem(this.dismissedKey);
    if (!raw) return {};
    try {
      return JSON.parse(raw) as Record<string, boolean>;
    } catch {
      return {};
    }
  }
}
