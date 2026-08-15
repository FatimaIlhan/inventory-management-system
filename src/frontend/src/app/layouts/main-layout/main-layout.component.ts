import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = computed(() => this.authService.currentUser());
  readonly isAdmin = computed(() => 
    this.authService.currentRole() === 'Admin');

readonly profileRole = computed(() =>
  this.authService.currentRole() ?? ''
);
  readonly isSidebarCollapsed = signal(false);
  readonly isMobileSidebarOpen = signal(false);
  readonly profileName = computed(() => {
    const email = this.currentUser()?.email;
    if (!email || !email.includes('@')) {
      return 'Ahmed Khan';
    }

    return email.split('@')[0];
  });

  readonly masterDataNav = [
    { label: 'Categories', route: '/categories', icon: 'categories' },
    { label: 'Suppliers', route: '/suppliers', icon: 'suppliers' },
    { label: 'Products', route: '/dashboard', icon: 'products' }
  ] as const;

  readonly inventoryNav = [
    { label: 'Stock Management', route: '/dashboard', icon: 'stock' },
    { label: 'Stock Movements', route: '/dashboard', icon: 'movement' }
  ] as const;

  readonly purchaseNav = [{ label: 'Purchase Orders', route: '/dashboard', icon: 'purchase' }] as const;

  readonly reportNav = [
    { label: 'Reports', route: '/dashboard', icon: 'reports' },
    { label: 'Audit Logs', route: '/dashboard', icon: 'audit' }
  ] as const;

  readonly systemNav = [
    { label: 'Users', route: '/admin/users', icon: 'users' },
    { label: 'Roles & Permissions', route: '/dashboard', icon: 'roles' },
    { label: 'Settings', route: '/dashboard', icon: 'settings' }
  ] as const;

  getSidebarIconPath(icon: string): string {
    switch (icon) {
      case 'dashboard':
        return 'M3 4.5a1.5 1.5 0 0 1 1.5-1.5h5A1.5 1.5 0 0 1 11 4.5v5A1.5 1.5 0 0 1 9.5 11h-5A1.5 1.5 0 0 1 3 9.5zm10 0A1.5 1.5 0 0 1 14.5 3h5A1.5 1.5 0 0 1 21 4.5v2A1.5 1.5 0 0 1 19.5 8h-5A1.5 1.5 0 0 1 13 6.5zm0 8A1.5 1.5 0 0 1 14.5 11h5a1.5 1.5 0 0 1 1.5 1.5v7a1.5 1.5 0 0 1-1.5 1.5h-5a1.5 1.5 0 0 1-1.5-1.5zm-10 4A1.5 1.5 0 0 1 4.5 15h5a1.5 1.5 0 0 1 1.5 1.5v3A1.5 1.5 0 0 1 9.5 21h-5A1.5 1.5 0 0 1 3 19.5z';
      case 'categories':
        return 'M4 6a2 2 0 0 1 2-2h4l2 2h6a2 2 0 0 1 2 2v1H4zm0 3h16v9a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z';
      case 'suppliers':
        return 'M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4m-7 8a7 7 0 0 1 14 0zM18 7h4v2h-4zm0 4h3v2h-3z';
      case 'products':
        return 'm12 2 8 4v12l-8 4-8-4V6zm0 2.2L7 6.7v9l5 2.5 5-2.5v-9z';
      case 'stock':
        return 'M4 4h10v4H4zm0 6h16v4H4zm0 6h12v4H4z';
      case 'movement':
        return 'M6 7h12l-3-3m3 3-3 3M18 17H6l3 3m-3-3 3-3';
      case 'purchase':
        return 'M3 5h3l2.2 9.2a2 2 0 0 0 2 1.5h7.8a2 2 0 0 0 2-1.6L22 8H8m3 11a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3m8 0a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3';
      case 'reports':
        return 'M6 3h9l5 5v13a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1m8 1v5h5';
      case 'audit':
        return 'M11 4a8 8 0 1 0 8 8h-2a6 6 0 1 1-6-6zm1 1h8v8h-2V8.4l-5.8 5.8-1.4-1.4L16.6 7H12z';
      case 'users':
        return 'M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4m-7 8a7 7 0 0 1 14 0';
      case 'roles':
        return 'M12 2 4 6v6c0 5.2 3.4 9 8 10 4.6-1 8-4.8 8-10V6zm-1 6h2v5h-2zm0 6h2v2h-2z';
      case 'settings':
        return 'M19.4 13a7.8 7.8 0 0 0 0-2l2-1.5-2-3.5-2.3.8a7.7 7.7 0 0 0-1.7-1L15 3h-6l-.4 2.8a7.7 7.7 0 0 0-1.7 1L4.6 6 2.6 9.5 4.6 11a7.8 7.8 0 0 0 0 2l-2 1.5 2 3.5 2.3-.8a7.7 7.7 0 0 0 1.7 1L9 21h6l.4-2.8a7.7 7.7 0 0 0 1.7-1l2.3.8 2-3.5zM12 15.5A3.5 3.5 0 1 1 15.5 12 3.5 3.5 0 0 1 12 15.5';
      default:
        return 'M4 4h16v16H4z';
    }
  }

  toggleSidebarCollapse(): void {
    this.isSidebarCollapsed.update((isCollapsed) => !isCollapsed);
  }

  toggleMobileSidebar(): void {
    this.isMobileSidebarOpen.update((isOpen) => !isOpen);
  }

  closeMobileSidebar(): void {
    this.isMobileSidebarOpen.set(false);
  }

  async logout(): Promise<void> {
    await this.authService.logout();
    await this.router.navigate(['/auth/login']);
  }
}
