import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BabyApiService, MilestoneProgressDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-milestones-page',
  imports: [ReactiveFormsModule],
  templateUrl: './milestones-page.html',
  styleUrl: './milestones-page.scss',
})
export class MilestonesPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);

  readonly progress = signal<MilestoneProgressDto | null>(null);
  readonly history = computed(() => {
    const p = this.progress();
    if (!p) return [];
    return p.items
      .filter((i) => i.achieved && !!i.achievedOn)
      .map((i) => ({ title: i.title, achievedOn: i.achievedOn! }))
      .sort((a, b) => b.achievedOn.localeCompare(a.achievedOn));
  });

  form = this.fb.nonNullable.group({
    milestone: [0, Validators.required],
    achievedOn: [new Date().toISOString().slice(0, 10), Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.milestoneProgress(id).subscribe({ next: (p) => this.progress.set(p) });
  }

  save(): void {
    const id = this.ctx.babyId();
    if (!id || this.form.invalid) return;
    const v = this.form.getRawValue();
    this.api
      .addMilestone(id, {
        milestone: Number(v.milestone),
        achievedOn: v.achievedOn,
        notes: v.notes || null,
      })
      .subscribe({
        next: () => {
          this.reload();
          this.form.patchValue({ notes: '' });
        },
      });
  }
}
