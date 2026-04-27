import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { BabyApiService, RecipeDto } from '../../core/baby-api.service';

@Component({
  selector: 'app-recipes-page',
  templateUrl: './recipes-page.html',
  styleUrl: './recipes-page.scss',
})
export class RecipesPage implements OnInit {
  private readonly api = inject(BabyApiService);

  readonly allItems = signal<RecipeDto[]>([]);
  readonly maxAge = signal<number | null>(null);
  readonly query = signal('');
  readonly activeBand = signal<'all' | '6-8' | '9-11'>('all');

  readonly items = computed(() => {
    const max = this.maxAge();
    const q = this.query().trim().toLowerCase();
    const band = this.activeBand();

    return this.allItems().filter((r) => {
      if (max != null && r.minAgeMonths > max) return false;
      if (band === '6-8' && (r.minAgeMonths < 6 || r.minAgeMonths > 8)) return false;
      if (band === '9-11' && (r.minAgeMonths < 9 || r.minAgeMonths > 11)) return false;
      if (!q) return true;
      return (
        r.title.toLowerCase().includes(q) ||
        (r.summary ?? '').toLowerCase().includes(q)
      );
    });
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.api.recipes().subscribe({ next: (r) => this.allItems.set(r) });
  }

  setMaxAge(ev: Event): void {
    const raw = (ev.target as HTMLInputElement).value.trim();
    const v = Number(raw);
    this.maxAge.set(Number.isFinite(v) && v > 0 ? v : null);
  }

  setQuery(ev: Event): void {
    this.query.set((ev.target as HTMLInputElement).value);
  }

  setBand(band: 'all' | '6-8' | '9-11'): void {
    this.activeBand.set(band);
  }

  clearFilters(): void {
    this.maxAge.set(null);
    this.query.set('');
    this.activeBand.set('all');
  }

  download(r: RecipeDto): void {
    if (!r.pdfFileName) return;
    this.api.recipePdfBlob(r.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = r.pdfFileName ?? 'recept.pdf';
        a.click();
        URL.revokeObjectURL(url);
      },
    });
  }
}
