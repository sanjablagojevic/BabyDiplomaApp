import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { BabyApiService, GallerySlotDto } from '../../core/baby-api.service';
import { BabyContextService } from '../../core/baby-context.service';

type GalleryTile = {
  monthSlot: number;
  label: string;
  imageUrl: string | null;
  uploadedUtc: string | null;
};

@Component({
  selector: 'app-gallery-page',
  imports: [DatePipe],
  templateUrl: './gallery-page.html',
  styleUrl: './gallery-page.scss',
})
export class GalleryPage implements OnInit {
  private readonly api = inject(BabyApiService);
  private readonly ctx = inject(BabyContextService);

  readonly tiles = signal<GalleryTile[]>([]);
  readonly uploadingSlot = signal<number | null>(null);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    const id = this.ctx.babyId();
    if (!id) return;

    this.api.gallerySlots(id).subscribe({
      next: (slots) => {
        const map = new Map<number, GallerySlotDto>();
        slots.forEach((s) => map.set(s.monthSlot, s));
        this.tiles.set(
          this.baseSlots().map((b) => ({
            ...b,
            imageUrl: map.get(b.monthSlot)?.imageUrl ?? null,
            uploadedUtc: map.get(b.monthSlot)?.uploadedUtc ?? null,
          })),
        );
      },
    });
  }

  onPickFile(slot: number, ev: Event): void {
    const id = this.ctx.babyId();
    if (!id) return;
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.uploadingSlot.set(slot);
    this.api.uploadGallerySlot(id, slot, file).subscribe({
      next: () => {
        this.uploadingSlot.set(null);
        input.value = '';
        this.refresh();
      },
      error: () => {
        this.uploadingSlot.set(null);
        input.value = '';
      },
    });
  }

  clearSlot(slot: number): void {
    const id = this.ctx.babyId();
    if (!id) return;
    this.api.deleteGallerySlot(id, slot).subscribe({ next: () => this.refresh() });
  }

  private baseSlots(): GalleryTile[] {
    return [
      { monthSlot: 0, label: 'Just born', imageUrl: null, uploadedUtc: null },
      { monthSlot: 1, label: '1 month', imageUrl: null, uploadedUtc: null },
      { monthSlot: 2, label: '2 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 3, label: '3 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 4, label: '4 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 5, label: '5 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 6, label: '6 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 7, label: '7 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 8, label: '8 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 9, label: '9 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 10, label: '10 months', imageUrl: null, uploadedUtc: null },
      { monthSlot: 11, label: '11 months', imageUrl: null, uploadedUtc: null },
    ];
  }
}
