import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BabyApiService, FeedingDto, MilkScheduleDto, SolidGuidanceDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-feeding-page',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './feeding-page.html',
  styleUrl: './feeding-page.scss',
})
export class FeedingPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);

  readonly milk = signal<MilkScheduleDto | null>(null);
  readonly solid = signal<SolidGuidanceDto | null>(null);
  readonly logs = signal<FeedingDto[]>([]);
  readonly feedType = signal(1);

  logForm = this.fb.nonNullable.group({
    start: ['', Validators.required],
    type: [1, Validators.required],
    end: [''],
    breastSide: [''],
    amountMl: [''],
    foodDescription: [''],
    notes: [''],
  });

  ngOnInit(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.milkHints(id).subscribe({ next: (m) => this.milk.set(m) });
    this.api.solidGuidance(id).subscribe({ next: (s) => this.solid.set(s) });
    this.refreshLogs();
    this.logForm.controls.type.valueChanges.subscribe((v) => this.feedType.set(Number(v)));
  }

  refreshLogs(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const from = new Date();
    from.setDate(from.getDate() - 7);
    this.api.feedingLogs(id, from.toISOString()).subscribe({ next: (l) => this.logs.set(l) });
  }

  addLog(): void {
    const id = this.ctx.babyId();
    if (!id || this.logForm.invalid) return;
    const v = this.logForm.getRawValue();
    const start = new Date(v.start);
    const end = v.end ? new Date(v.end) : null;
    const amount = v.amountMl ? Number(v.amountMl) : null;
    const type = Number(v.type);
    const breastSide = v.breastSide === '' ? null : Number(v.breastSide);
    this.api
      .addFeeding(id, {
        startUtc: start.toISOString(),
        endUtc: end ? end.toISOString() : null,
        type,
        breastSide: type === 0 ? breastSide : null,
        amountMl: type === 1 ? amount : null,
        foodDescription: type === 2 ? v.foodDescription || null : null,
        notes: v.notes || null,
      })
      .subscribe({
        next: () => {
          this.refreshLogs();
          this.logForm.patchValue({ end: '', breastSide: '', amountMl: '', foodDescription: '', notes: '' });
        },
      });
  }
}
