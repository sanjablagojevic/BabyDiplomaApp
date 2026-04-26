import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BabyApiService, ReminderDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-reminders-page',
  imports: [ReactiveFormsModule],
  templateUrl: './reminders-page.html',
  styleUrl: './reminders-page.scss',
})
export class RemindersPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);

  readonly rows = signal<ReminderDto[]>([]);

  form = this.fb.nonNullable.group({
    kind: [1, Validators.required],
    title: ['Obrok', Validators.required],
    localTime: ['08:00', Validators.required],
    vaccineName: [''],
    vaccineDue: [''],
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.reminders(id).subscribe({ next: (r) => this.rows.set(r) });
  }

  add(): void {
    const id = this.ctx.babyId();
    if (!id || this.form.invalid) return;
    const v = this.form.getRawValue();
    const t = v.localTime.length === 5 ? `${v.localTime}:00` : v.localTime;
    this.api
      .addReminder(id, {
        kind: Number(v.kind),
        title: v.title,
        localTime: t,
        isEnabled: true,
        vaccineName: Number(v.kind) === 3 ? v.vaccineName || null : null,
        vaccineDueDate: Number(v.kind) === 3 && v.vaccineDue ? v.vaccineDue : null,
      })
      .subscribe({ next: () => this.refresh() });
  }

  toggle(r: ReminderDto): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.patchReminder(id, r.id, { isEnabled: !r.isEnabled }).subscribe({ next: () => this.refresh() });
  }

  remove(r: ReminderDto): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.deleteReminder(id, r.id).subscribe({ next: () => this.refresh() });
  }
}
