import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { AdminUsersPageComponent } from './features/auth/admin-users-page.component';

export const routes: Routes = [
	{
		path: 'auth',
		component: AuthLayoutComponent,
		children: [
			{ path: 'login', component: LoginPageComponent },
			{ path: '', pathMatch: 'full', redirectTo: 'login' }
		]
	},
	{
		path: '',
		component: MainLayoutComponent,
		canActivate: [authGuard],
		children: [
			{ path: 'dashboard', component: DashboardPageComponent },
			{
				path: 'admin/users',
				component: AdminUsersPageComponent,
				canActivate: [roleGuard],
				data: { allowedRoles: ['Admin'] }
			},
			{ path: '', pathMatch: 'full', redirectTo: 'dashboard' }
		]
	},
	{ path: '**', redirectTo: '' }
];
