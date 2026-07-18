import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [MatButtonModule],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent {
  private readonly authService = inject(AuthService);

  readonly currentUser = computed(() => this.authService.currentUser());
  readonly displayName = computed(() => {
    const email = this.currentUser()?.email?.trim();
    if (!email || !email.includes('@')) {
      return 'Ahmed';
    }

    const [localPart] = email.split('@');
    return localPart
      .split(/[._-]/)
      .filter((segment) => segment.length > 0)
      .map((segment) => segment.charAt(0).toUpperCase() + segment.slice(1))
      .join(' ');
  });

  readonly statCards = [
    { title: 'Products', value: '1,248', delta: '+12.5%', trend: 'up', tone: 'blue', icon: 'box' },
    { title: 'Categories', value: '48', delta: '+5.2%', trend: 'up', tone: 'green', icon: 'grid' },
    { title: 'Suppliers', value: '56', delta: '+8.1%', trend: 'up', tone: 'violet', icon: 'users' },
    { title: 'Low Stock Items', value: '23', delta: '-4.3%', trend: 'down', tone: 'amber', icon: 'alert' },
    { title: 'Purchase Orders', value: '12', delta: '+16.7%', trend: 'up', tone: 'cyan', icon: 'cart' }
  ] as const;

  readonly lineLabels = ['May 18', 'May 19', 'May 20', 'May 21', 'May 23', 'May 24', 'May 25'] as const;

  readonly categories = [
    { name: 'Electronics', percent: 35, count: 435, color: '#2f7bff' },
    { name: 'Office Supplies', percent: 25, count: 310, color: '#17b15f' },
    { name: 'Furniture', percent: 20, count: 248, color: '#7d4ddd' },
    { name: 'Accessories', percent: 15, count: 186, color: '#f0a51f' },
    { name: 'Others', percent: 5, count: 69, color: '#b6c2d5' }
  ] as const;

  readonly recentActivity = [
    { action: 'Stock In', reference: 'SI-2024-0056', user: 'Ahmed Khan', dateTime: 'May 25, 2024 10:24 AM', type: 'in' },
    { action: 'Stock Out', reference: 'SO-2024-0032', user: 'Fatima Ali', dateTime: 'May 25, 2024 09:15 AM', type: 'out' },
    {
      action: 'Purchase Order Received',
      reference: 'PO-2024-0018',
      user: 'Ahmed Khan',
      dateTime: 'May 24, 2024 04:42 PM',
      type: 'order'
    },
    { action: 'Product Updated', reference: 'PRD-000124', user: 'Fatima Ali', dateTime: 'May 24, 2024 11:08 AM', type: 'update' },
    {
      action: 'Category Created',
      reference: 'CAT-000048',
      user: 'Ahmed Khan',
      dateTime: 'May 23, 2024 03:21 PM',
      type: 'category'
    }
  ] as const;

  readonly lowStockItems = [
    { product: 'Wireless Earbuds', sku: 'SKU-001245', currentStock: 5, reorderLevel: 10 },
    { product: '24" Monitor', sku: 'SKU-000987', currentStock: 3, reorderLevel: 8 },
    { product: 'Mechanical Keyboard', sku: 'SKU-001102', currentStock: 2, reorderLevel: 5 },
    { product: 'Office Chair', sku: 'SKU-000654', currentStock: 4, reorderLevel: 6 },
    { product: 'A4 Paper (Box)', sku: 'SKU-000321', currentStock: 6, reorderLevel: 10 }
  ] as const;

  getStatIconPath(icon: string): string {
    switch (icon) {
      case 'box':
        return 'm12 2 8 4v12l-8 4-8-4V6zm0 2.2L7 6.7v9l5 2.5 5-2.5v-9z';
      case 'grid':
        return 'M4 4h7v7H4zm9 0h7v7h-7zM4 13h7v7H4zm9 0h7v7h-7z';
      case 'users':
        return 'M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4m-7 8a7 7 0 0 1 14 0';
      case 'alert':
        return 'M12 3 2.5 20h19zM12 9v5m0 3h.01';
      case 'cart':
        return 'M3 5h3l2.2 9.2a2 2 0 0 0 2 1.5h7.8a2 2 0 0 0 2-1.6L22 8H8m3 11a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3m8 0a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3';
      default:
        return 'M4 4h16v16H4z';
    }
  }
}
