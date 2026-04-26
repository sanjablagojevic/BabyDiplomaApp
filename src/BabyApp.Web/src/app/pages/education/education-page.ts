import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BabyApiService } from '../../core/baby-api.service';

@Component({
  selector: 'app-education-page',
  imports: [RouterLink],
  templateUrl: './education-page.html',
  styleUrl: './education-page.scss',
})
export class EducationPage implements OnInit {
  private readonly api = inject(BabyApiService);

  readonly band = signal<number | null>(null);
  readonly articles = signal<{ id: number; ageBand: string; title: string; body: string }[]>([]);

  ngOnInit(): void {
    this.reload();
  }

  setBand(b: number | null): void {
    this.band.set(b);
    this.reload();
  }

  reload(): void {
    this.api.educationArticles(this.band() ?? undefined).subscribe({ next: (a) => this.articles.set(a) });
  }
}
