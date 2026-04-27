import { Component, inject, OnInit, signal } from '@angular/core';
import { BabyApiService } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-insights-page',
  templateUrl: './insights-page.html',
  styleUrl: './insights-page.scss',
})
export class InsightsPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);

  readonly lines = signal<string[]>([]);

  ngOnInit(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.insights(id).subscribe({ next: (x) => this.lines.set(x) });
  }

  toneFor(line: string): 'warn' | 'ok' | 'info' {
    const t = line.toLowerCase();
    if (t.includes('upozoren') || t.includes('niž') || t.includes('reakcij') || t.includes('posavjetuj')) {
      return 'warn';
    }
    if (t.includes('nema automatskih upozorenja')) {
      return 'ok';
    }
    return 'info';
  }
}
