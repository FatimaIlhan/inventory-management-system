import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { ChartData, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { ApiEnvelope } from '../../core/services/auth.models';
import { AuthService } from '../../core/services/auth.service';
import {
  DashboardRecentActivity,
  DashboardSummary
} from '../../core/services/dashboard.models';
import { DashboardService } from '../../core/services/dashboard.service';
import { StockMovementType } from '../../core/services/inventory-movement.models';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatButtonModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly dashboardService = inject(DashboardService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly currentUser = computed(() => this.authService.currentUser());
  readonly dashboard = signal<DashboardSummary | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly dateRangeForm = this.formBuilder.nonNullable.group({
    fromUtc: [this.getDateInputValue(-29)],
    toUtc: [this.getDateInputValue(0)]
  });
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

  readonly statCards = computed(() => {
    const summary = this.dashboard();
    return [
      { title: 'Products', value: summary?.totalProducts ?? 0, tone: 'blue', icon: 'box' },
      { title: 'Categories', value: summary?.totalCategories ?? 0, tone: 'green', icon: 'grid' },
      { title: 'Suppliers', value: summary?.totalSuppliers ?? 0, tone: 'violet', icon: 'users' },
      { title: 'Low Stock Items', value: summary?.lowStockItemCount ?? 0, tone: 'amber', icon: 'alert' },
      { title: 'Purchase Orders', value: summary?.totalPurchaseOrders ?? 0, tone: 'cyan', icon: 'cart' },
      { title: 'Recent Activity', value: summary?.recentActivityCount ?? 0, tone: 'teal', icon: 'activity' }
    ] as const;
  });

  readonly movementTrendData = computed<ChartData<'line'>>(() => {
    const trends = this.dashboard()?.movementTrends ?? [];
    return {
      labels: trends.map((trend) => this.formatDate(trend.dateUtc)),
      datasets: [
        { data: trends.map((trend) => trend.stockInQuantity), label: 'Stock In', borderColor: '#16b15d', backgroundColor: '#16b15d', tension: 0.3 },
        { data: trends.map((trend) => trend.stockOutQuantity), label: 'Stock Out', borderColor: '#e53e3e', backgroundColor: '#e53e3e', tension: 0.3 },
        { data: trends.map((trend) => trend.adjustmentQuantity), label: 'Adjustments', borderColor: '#2f7bff', backgroundColor: '#2f7bff', tension: 0.3 }
      ]
    };
  });

  readonly inventoryByCategoryData = computed<ChartData<'doughnut'>>(() => {
    const categories = this.dashboard()?.inventoryByCategory ?? [];
    return {
      labels: categories.map((category) => category.categoryName),
      datasets: [{
        data: categories.map((category) => category.currentStock),
        backgroundColor: ['#2f7bff', '#17b15f', '#f0a51f', '#e05353', '#7658c9', '#43a5c9']
      }]
    };
  });

  readonly topMovingProductsData = computed<ChartData<'bar'>>(() => {
    const products = this.dashboard()?.topMovingProducts ?? [];
    return {
      labels: products.map((product) => product.productName),
      datasets: [{
        data: products.map((product) => product.totalQuantityMoved),
        label: 'Units moved',
        backgroundColor: '#2f7bff',
        borderRadius: 4
      }]
    };
  });

  readonly lineChartOptions: ChartOptions<'line'> = { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true } } };
  readonly doughnutChartOptions: ChartOptions<'doughnut'> = { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } };
  readonly barChartOptions: ChartOptions<'bar'> = { responsive: true, maintainAspectRatio: false, indexAxis: 'y', scales: { x: { beginAtZero: true } }, plugins: { legend: { display: false } } };

  ngOnInit(): void {
    void this.loadDashboardAsync();
  }

  async loadDashboardAsync(): Promise<void> {
    if (this.dateRangeForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const dateRange = this.dateRangeForm.getRawValue();
      this.dashboard.set(await this.dashboardService.getSummaryAsync(dateRange));
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error));
    } finally {
      this.isLoading.set(false);
    }
  }

  async viewAllActivityAsync(): Promise<void> {
    await this.router.navigate(['/stock-movements']);
  }

  async viewLowStockAsync(): Promise<void> {
    await this.router.navigate(['/stock-management']);
  }

  getMovementLabel(movementType: StockMovementType): string {
    return movementType === StockMovementType.StockIn
      ? 'Stock in'
      : movementType === StockMovementType.StockOut
        ? 'Stock out'
        : 'Stock adjustment';
  }

  getMovementTone(activity: DashboardRecentActivity): string {
    return activity.movementType === StockMovementType.StockIn
      ? 'in'
      : activity.movementType === StockMovementType.StockOut
        ? 'out'
        : 'adjustment';
  }

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
      case 'activity':
        return 'M3 12h4l2.5-6 4.5 12 2.5-6H21';
      default:
        return 'M4 4h16v16H4z';
    }
  }

  private getDateInputValue(daysFromToday: number): string {
    const date = new Date();
    date.setDate(date.getDate() + daysFromToday);
    return date.toISOString().slice(0, 10);
  }

  private formatDate(value: string): string {
    return new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric' }).format(new Date(value));
  }

  private readErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as ApiEnvelope<unknown> | null;
      if (apiError?.message) {
        return apiError.message;
      }
      return error.status === 0 ? 'Cannot reach server. Check API and proxy configuration.' : `Request failed (${error.status}).`;
    }

    return error instanceof Error && error.message.trim().length > 0 ? error.message : 'Failed to load dashboard data.';
  }
}
