import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { BabyApiService, DashboardDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [DecimalPipe],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);

  readonly data = signal<DashboardDto | null>(null);
  readonly err = signal('');

  ngOnInit(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.dashboard(id).subscribe({
      next: (d) => this.data.set(d),
      error: () => this.err.set('Ne mogu učitati pregled.'),
    });
  }
}
