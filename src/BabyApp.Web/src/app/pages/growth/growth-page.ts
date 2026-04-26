import {
  AfterViewChecked,
  Component,
  ElementRef,
  inject,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { BabyApiService, GrowthDto, ReferenceBundleDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

Chart.register(...registerables);

@Component({
  selector: 'app-growth-page',
  imports: [ReactiveFormsModule],
  templateUrl: './growth-page.html',
  styleUrl: './growth-page.scss',
})
export class GrowthPage implements OnInit, AfterViewChecked {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);
  private readonly fb = inject(FormBuilder);

  readonly canvas = viewChild<ElementRef<HTMLCanvasElement>>('chart');

  readonly rows = signal<GrowthDto[]>([]);
  readonly ref = signal<ReferenceBundleDto | null>(null);

  refDate = new Date().toISOString().slice(0, 10);

  form = this.fb.nonNullable.group({
    measuredDate: [new Date().toISOString().slice(0, 10), Validators.required],
    weightKg: [''],
    heightCm: [''],
    headCm: [''],
    notes: [''],
  });

  private chart?: Chart;
  private needRedraw = false;

  ngOnInit(): void {
    this.refresh();
    this.loadRef();
  }

  ngAfterViewChecked(): void {
    if (this.needRedraw) {
      this.needRedraw = false;
      this.drawChart();
    }
  }

  refresh(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.growthMeasurements(id).subscribe({
      next: (g) => {
        this.rows.set(g);
        this.needRedraw = true;
      },
    });
  }

  loadRef(): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.growthReference(id, this.refDate).subscribe({ next: (r) => this.ref.set(r) });
  }

  add(): void {
    const id = this.ctx.babyId();
    if (!id || this.form.invalid) return;
    const v = this.form.getRawValue();
    this.api
      .addGrowth(id, {
        measuredDate: v.measuredDate,
        weightKg: v.weightKg ? Number(v.weightKg) : null,
        heightCm: v.heightCm ? Number(v.heightCm) : null,
        headCircumferenceCm: v.headCm ? Number(v.headCm) : null,
        notes: v.notes || null,
      })
      .subscribe({ next: () => this.refresh() });
  }

  private drawChart(): void {
    const el = this.canvas()?.nativeElement;
    if (!el) return;
    const data = this.rows().filter((x) => x.weightKg != null).sort((a, b) => a.measuredDate.localeCompare(b.measuredDate));
    const labels = data.map((d) => d.measuredDate);
    const weights = data.map((d) => Number(d.weightKg));

    this.chart?.destroy();
    if (!labels.length) return;

    const cfg: ChartConfiguration = {
      type: 'line',
      data: {
        labels,
        datasets: [{ label: 'Težina (kg)', data: weights, borderColor: '#2563eb', tension: 0.2 }],
      },
      options: {
        responsive: true,
        plugins: { legend: { display: true } },
        scales: { x: { display: true }, y: { display: true } },
      },
    };
    this.chart = new Chart(el, cfg);
  }
}
