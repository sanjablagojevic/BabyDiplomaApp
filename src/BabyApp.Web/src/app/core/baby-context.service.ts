import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class BabyContextService {
  readonly babyId = signal<number | null>(null);

  setBabyId(id: number): void {
    this.babyId.set(id);
  }
}
