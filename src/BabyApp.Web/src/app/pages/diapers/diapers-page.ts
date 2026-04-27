import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { BabyApiService, DiaperLogDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-diapers-page',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './diapers-page.html',
  styleUrl: './diapers-page.scss',
})
export class DiapersPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);

  readonly rows = signal<DiaperLogDto[]>([]);

  readonly stats = computed(() => {
    const list = this.rows();
    return {
      wet: list.filter((x) => x.type === 'Wet').length,
      dirty: list.filter((x) => x.type === 'Dirty').length,
      mixed: list.filter((x) => x.type === 'Mixed').length,
    };
  });

  form = this.fb.nonNullable.group({
    occurred: [new Date().toISOString().slice(0, 16)],
    notes: [''],
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const from = new Date();
    from.setDate(from.getDate() - 7);
    this.api.diaperLogs(id, from.toISOString()).subscribe({ next: (r) => this.rows.set(r) });
  }

  add(type: number): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const v = this.form.getRawValue();
    const when = v.occurred ? new Date(v.occurred) : new Date();
    this.api
      .addDiaperLog(id, {
        occurredUtc: when.toISOString(),
        type,
        notes: v.notes || null,
      })
      .subscribe({
        next: () => {
          this.refresh();
          this.form.patchValue({ notes: '' });
        },
      });
  }

  remove(row: DiaperLogDto): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.deleteDiaperLog(id, row.id).subscribe({ next: () => this.refresh() });
  }
}
