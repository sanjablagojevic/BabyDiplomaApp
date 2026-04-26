import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BabyApiService, ReactionDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-reactions-page',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './reactions-page.html',
  styleUrl: './reactions-page.scss',
})
export class ReactionsPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);

  readonly rows = signal<ReactionDto[]>([]);

  form = this.fb.nonNullable.group({
    when: ['', Validators.required],
    kind: [0, Validators.required],
    food: [''],
    notes: [''],
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const from = new Date();
    from.setDate(from.getDate() - 60);
    this.api.reactions(id, from.toISOString()).subscribe({ next: (r) => this.rows.set(r) });
  }

  add(): void {
    const id = this.ctx.babyId();
    if (!id || this.form.invalid) return;
    const v = this.form.getRawValue();
    this.api
      .addReaction(id, {
        occurredUtc: new Date(v.when).toISOString(),
        kind: Number(v.kind),
        foodTrigger: v.food || null,
        notes: v.notes || null,
      })
      .subscribe({ next: () => this.refresh() });
  }

  remove(x: ReactionDto): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.deleteReaction(id, x.id).subscribe({ next: () => this.refresh() });
  }
}
