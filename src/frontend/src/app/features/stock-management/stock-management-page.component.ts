import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { ApiEnvelope } from '../../core/services/auth.models';
import { Product, ProductStatus } from '../../core/services/product.models';
import { ProductService } from '../../core/services/product.service';

type StockFilter = 'all' | 'attention' | 'out-of-stock' | 'available';

@Component({
  selector: 'app-stock-management-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    MatSelectModule,
    MatTableModule
  ],
  templateUrl: './stock-management-page.component.html',
  styleUrl: './stock-management-page.component.scss'
})
export class StockManagementPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly displayedColumns = ['product', 'currentStock', 'reorderLevel', 'shortfall', 'status', 'action'];
  readonly products = signal<Product[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly filtersForm = this.formBuilder.nonNullable.group({
    search: [''],
    stockFilter: ['all' as StockFilter]
  });
  readonly searchControl = this.filtersForm.controls.search;
  readonly stockFilterControl = this.filtersForm.controls.stockFilter;

  readonly activeProducts = computed(() => this.products().filter((product) => product.status === ProductStatus.Active));
  readonly outOfStockCount = computed(() => this.activeProducts().filter((product) => product.currentStock === 0).length);
  readonly lowStockCount = computed(() => this.activeProducts().filter((product) => this.isLowStock(product)).length);
  readonly availableCount = computed(() => this.activeProducts().filter((product) => product.currentStock > product.reorderLevel).length);
  readonly totalUnits = computed(() => this.activeProducts().reduce((total, product) => total + product.currentStock, 0));
  readonly filteredProducts = computed(() => {
    const stockFilter = this.stockFilterControl.value;
    return this.activeProducts().filter((product) => {
      if (stockFilter === 'attention') return this.isLowStock(product);
      if (stockFilter === 'out-of-stock') return product.currentStock === 0;
      if (stockFilter === 'available') return product.currentStock > product.reorderLevel;
      return true;
    });
  });

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => void this.loadProductsAsync());
    this.stockFilterControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => undefined);
    void this.loadProductsAsync();
  }

  async loadProductsAsync(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      const result = await this.productService.getPagedAsync({
        page: 1,
        pageSize: 100,
        search: this.searchControl.value,
        sortBy: 'currentstock',
        descending: false
      });
      this.products.set(result.items);
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error));
    } finally {
      this.isLoading.set(false);
    }
  }

  async recordMovement(): Promise<void> {
    await this.router.navigate(['/stock-movements']);
  }

  trackProduct(_: number, product: Product): number {
    return product.productId;
  }

  isLowStock(product: Product): boolean {
    return product.currentStock <= product.reorderLevel;
  }

  getStatusLabel(product: Product): string {
    if (product.currentStock === 0) return 'Out of stock';
    if (this.isLowStock(product)) return 'Reorder required';
    return 'In stock';
  }

  getStockGap(product: Product): number {
    return Math.max(product.reorderLevel - product.currentStock, 0);
  }

  private readErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as ApiEnvelope<unknown> | null;
      if (apiError?.message) return apiError.message;
      return error.status === 0 ? 'Cannot reach server. Check API and proxy configuration.' : `Request failed (${error.status}).`;
    }
    return error instanceof Error && error.message.trim().length > 0 ? error.message : 'Failed to load stock levels.';
  }
}