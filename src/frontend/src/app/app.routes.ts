import { Routes } from '@angular/router';
import { roleGuard } from './core/guards/role.guard';
import { authGuard } from './core/guards/auth.guard';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { LoginPageComponent } from './features/auth/login-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { AdminUsersPageComponent } from './features/auth/admin-users-page.component';
import { CategoriesPageComponent } from './features/categories/categories-page.component';
import { SupplierComponent } from './features/suppliers/supplier/supplier.component';
import { ProductComponent } from './features/products/product';
import { StockMovementsPageComponent } from './features/stock-movements/stock-movements-page.component';
import { StockManagementPageComponent } from './features/stock-management/stock-management-page.component';

export const routes: Routes = [
	{
		path: 'auth',
		component: AuthLayoutComponent,
		children: [
			{
				path: 'login',
				component: LoginPageComponent,
				data: {
					title: 'Sign In | Inventory Management System',
					description: 'Secure login for inventory operations and role-based access.'
				}
			},
			{ path: '', pathMatch: 'full', redirectTo: 'login' }
		]
	},
	{
		path: '',
		component: MainLayoutComponent,
		canActivate: [authGuard],
		children: [
			{
				path: 'dashboard',
				component: DashboardPageComponent,
				data: {
					title: 'Dashboard | Inventory Management System',
					description: 'Overview of account and inventory operations status.'
				}
			},
			{
				path: 'categories',
				component: CategoriesPageComponent,
				data: {
					title: 'Categories | Inventory Management System',
					description: 'Create, update, delete, and search product categories.'
				}
			},
				{
				path: 'suppliers',
				component: SupplierComponent,
				data: {
					title: 'Suppliers | Inventory Management System',
					description: 'Create, update, delete, and search suppliers.'
				}
			},
			{
				path: 'products',
				component: ProductComponent,
				data: {
					title: 'Products | Inventory Management System',
					description: 'Create, update, delete, and search products.'
				}
			},
			{
				path: 'stock-management',
				component: StockManagementPageComponent,
				data: {
					title: 'Stock Management | Inventory Management System',
					description: 'Monitor on-hand inventory and replenishment status.'
				}
			},
			{
				path: 'stock-movements',
				component: StockMovementsPageComponent,
				data: {
					title: 'Stock Movements | Inventory Management System',
					description: 'Record and audit all inventory stock changes.'
				}
			},
			{
				path: 'admin/users',
				component: AdminUsersPageComponent,
				canActivate: [roleGuard],
				data: {
					allowedRoles: ['Admin'],
					title: 'User Administration | Inventory Management System',
					description: 'Create and manage user accounts with role assignments.'
				}
			},
			{ path: '', pathMatch: 'full', redirectTo: 'dashboard' }
		]
	},
	{ path: '**', redirectTo: '' }
];
