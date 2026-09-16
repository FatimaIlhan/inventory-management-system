import { Routes } from '@angular/router';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const dashboardRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./dashboard-page.component')
      .then((module) => module.DashboardPageComponent),
    providers: [provideCharts(withDefaultRegisterables())],
    data: {
      title: 'Dashboard | Inventory Management System',
      description: 'Overview of account and inventory operations status.'
    }
  }
];