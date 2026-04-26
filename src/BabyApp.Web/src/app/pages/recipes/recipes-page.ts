import { Component, inject, OnInit, signal } from '@angular/core';
import { BabyApiService, RecipeDto } from '../../core/baby-api.service';

@Component({
  selector: 'app-recipes-page',
  templateUrl: './recipes-page.html',
  styleUrl: './recipes-page.scss',
})
export class RecipesPage implements OnInit {
  private readonly api = inject(BabyApiService);

  readonly items = signal<RecipeDto[]>([]);
  readonly maxAge = signal<number | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    const m = this.maxAge();
    this.api.recipes(m ?? undefined).subscribe({ next: (r) => this.items.set(r) });
  }

  setMaxAge(ev: Event): void {
    const v = Number((ev.target as HTMLInputElement).value);
    this.maxAge.set(Number.isFinite(v) && v > 0 ? v : null);
    this.reload();
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
