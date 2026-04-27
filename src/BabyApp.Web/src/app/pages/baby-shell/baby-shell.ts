import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { BabyContextService } from '../../core/baby-context.service';

@Component({
  selector: 'app-baby-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './baby-shell.html',
  styleUrl: './baby-shell.scss',
})
export class BabyShellComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly ctx = inject(BabyContextService);
  private sub?: Subscription;
  babyId = 0;

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe((pm: ParamMap) => {
      const id = Number(pm.get('babyId'));
      this.babyId = id;
      this.ctx.setBabyId(id);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
