import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BabyApiService, BabyDto } from '../../core/baby-api.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-babies',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './babies.html',
  styleUrl: './babies.scss',
})
export class BabiesPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly babiesApi = inject(BabyApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly babies = signal<BabyDto[]>([]);
  readonly loadError = signal('');

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    sex: [0],
    infantMilkRoutine: [-1],
    solidMealsPerDayGoal: [0],
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loadError.set('');
    this.babiesApi.listBabies().subscribe({
      next: (rows) => this.babies.set(rows),
      error: () => this.loadError.set('Ne mogu učitati listu. Jesi li prijavljen?'),
    });
  }

  create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.babiesApi
      .createBaby({
        name: v.name,
        dateOfBirth: v.dateOfBirth,
        sex: Number(v.sex),
        infantMilkRoutine: Number(v.infantMilkRoutine) < 0 ? null : Number(v.infantMilkRoutine),
        solidMealsPerDayGoal: Number(v.solidMealsPerDayGoal),
      })
      .subscribe({
        next: () => {
          this.form.reset({ name: '', dateOfBirth: '', sex: 0, infantMilkRoutine: -1, solidMealsPerDayGoal: 0 });
          this.refresh();
        },
        error: () => this.loadError.set('Spremanje nije uspjelo.'),
      });
  }

  openBaby(id: number): void {
    void this.router.navigate(['/baby', id, 'dashboard']);
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }
}
