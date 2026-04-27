import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { BabyShellComponent } from './pages/baby-shell/baby-shell';
import { BabiesPage } from './pages/babies/babies';
import { DashboardPage } from './pages/dashboard/dashboard-page';
import { EducationPage } from './pages/education/education-page';
import { FeedingPage } from './pages/feeding/feeding-page';
import { GrowthPage } from './pages/growth/growth-page';
import { InsightsPage } from './pages/insights/insights-page';
import { LoginPage } from './pages/login/login';
import { MilestonesPage } from './pages/milestones/milestones-page';
import { RecipesPage } from './pages/recipes/recipes-page';
import { ReactionsPage } from './pages/reactions/reactions-page';
import { RegisterPage } from './pages/register/register';
import { RemindersPage } from './pages/reminders/reminders-page';
import { SleepPage } from './pages/sleep/sleep-page';
import { DiapersPage } from './pages/diapers/diapers-page';
import { GalleryPage } from './pages/gallery/gallery-page';

export const routes: Routes = [
  { path: 'login', component: LoginPage },
  { path: 'register', component: RegisterPage },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: 'babies', component: BabiesPage },
      {
        path: 'baby/:babyId',
        component: BabyShellComponent,
        children: [
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
          { path: 'dashboard', component: DashboardPage },
          { path: 'sleep', component: SleepPage },
          { path: 'feeding', component: FeedingPage },
          { path: 'diapers', component: DiapersPage },
          { path: 'gallery', component: GalleryPage },
          { path: 'recipes', component: RecipesPage },
          { path: 'reactions', component: ReactionsPage },
          { path: 'growth', component: GrowthPage },
          { path: 'milestones', component: MilestonesPage },
          { path: 'reminders', component: RemindersPage },
          { path: 'insights', component: InsightsPage },
        ],
      },
      { path: 'education', component: EducationPage },
      { path: '', pathMatch: 'full', redirectTo: 'babies' },
      { path: '**', redirectTo: 'babies' },
    ],
  },
];
