import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ApiEnvelope } from '../../core/services/auth.models';
import { Category } from '../../core/services/category.models';
import { CategoryService } from '../../core/services/category.service';
import { Product, ProductStatus, ProductListQuery } from '../../core/services/product.models';
import { ProductService } from '../../core/services/product.service';
import { Supplier } from '../../core/services/supplier.models';
import { SupplierService } from '../../core/services/supplier.service';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatSelectModule,
    DatePipe
  ],
  templateUrl: './product.html',
  styleUrl: './product.scss',
})
export class ProductComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly supplierService = inject(SupplierService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private loadRequestSequence = 0;

  readonly canManageProducts = computed(() => this.authService.canAccessRole(['Admin', 'Manager']));
  readonly displayedColumns = computed<string[]>(() =>
    this.canManageProducts()
      ? ['sku', 'name', 'unitPrice', 'currentStock', 'reorderLevel', 'status', 'categoryId', 'supplierId', 'createdAtUtc', 'updatedAtUtc', 'actions']
      : ['sku', 'name', 'unitPrice', 'currentStock', 'reorderLevel', 'status', 'categoryId', 'supplierId', 'createdAtUtc', 'updatedAtUtc']
  );

  readonly products = signal<Product[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly editingProductId = signal<number | null>(null);
  readonly isFormModalOpen = signal(false);
  readonly categories = signal<Category[]>([]);
  readonly suppliers = signal<Supplier[]>([]);

  readonly searchForm = this.formBuilder.nonNullable.group({
    search: ['']
  });

  readonly productForm = this.formBuilder.nonNullable.group({
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    unitPrice: [0, [Validators.required, Validators.min(0.01)]],
    currentStock: [0, [Validators.required, Validators.min(0)]],
    reorderLevel: [0, [Validators.required, Validators.min(0)]],
    status: [ProductStatus.Active, [Validators.required]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    supplierId: [0, [Validators.required, Validators.min(1)]]
  });

  readonly searchControl = this.searchForm.controls.search;
  readonly statusOptions = [
    { value: ProductStatus.Active, label: 'Active' },
    { value: ProductStatus.Inactive, label: 'Inactive' }
  ];

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pageIndex.set(0);
        void this.loadProductsAsync();
      });

    void this.loadProductsAsync();
    void this.loadProductReferencesAsync();
  }

  async loadProductReferencesAsync(): Promise<void> {
    try {
      const [categoryResult, supplierResult] = await Promise.all([
        this.categoryService.getPagedAsync({ page: 1, pageSize: 100 }),
        this.supplierService.getPagedAsync({ page: 1, pageSize: 100 })
      ]);

      this.categories.set(categoryResult.items);
      this.suppliers.set(supplierResult.items);
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to load categories and suppliers.'));
    }
  }

  async loadProductsAsync(): Promise<void> {
    const requestId = ++this.loadRequestSequence;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const page = this.pageIndex() + 1;
    const pageSize = this.pageSize();
    const search = this.searchControl.value;

    try {
      const query: ProductListQuery = { page, pageSize, search };
      let result = await this.productService.getPagedAsync(query);

      if (requestId !== this.loadRequestSequence) {
        return;
      }

      if (!Array.isArray(result.items) || typeof result.totalCount !== 'number') {
        throw new Error('Invalid product response payload from API.');
      }

      this.products.set(result.items);
      this.totalCount.set(result.totalCount);
    } catch (error: unknown) {
      if (requestId !== this.loadRequestSequence) {
        return;
      }

      this.errorMessage.set(this.readErrorMessage(error, 'Failed to load products.'));
    } finally {
      if (requestId === this.loadRequestSequence) {
        this.isLoading.set(false);
      }
    }
  }

  async submitAsync(): Promise<void> {
    if (!this.canManageProducts()) {
      return;
    }

    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const formValue = this.productForm.getRawValue();
    const payload = {
      sku: formValue.sku.trim(),
      name: formValue.name.trim(),
      description: formValue.description.trim() || undefined,
      unitPrice: Number(formValue.unitPrice),
      currentStock: Number(formValue.currentStock),
      reorderLevel: Number(formValue.reorderLevel),
      status: Number(formValue.status),
      categoryId: Number(formValue.categoryId),
      supplierId: Number(formValue.supplierId)
    };

    try {
      const editingId = this.editingProductId();

      if (editingId === null) {
        await this.productService.createAsync(payload);
        this.snackBar.open('Product created successfully.', 'Close', { duration: 2600 });
        this.pageIndex.set(0);
      } else {
        await this.productService.updateAsync(editingId, payload);
        this.snackBar.open('Product updated successfully.', 'Close', { duration: 2600 });
      }

      this.closeFormModal();
      await this.loadProductsAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to save product.'));
    } finally {
      this.isSaving.set(false);
    }
  }

  openCreateForm(): void {
    if (!this.canManageProducts()) {
      return;
    }

    this.editingProductId.set(null);
    this.resetForm();
    this.isFormModalOpen.set(true);
  }

  editProduct(product: Product): void {
    if (!this.canManageProducts()) {
      return;
    }

    this.editingProductId.set(product.productId);
    this.productForm.setValue({
      sku: product.sku,
      name: product.name,
      description: product.description ?? '',
      unitPrice: product.unitPrice,
      currentStock: product.currentStock,
      reorderLevel: product.reorderLevel,
      status: product.status,
      categoryId: product.categoryId,
      supplierId: product.supplierId
    });
    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    this.isFormModalOpen.set(false);
    this.resetForm();
  }

  async deleteProductAsync(productId: number): Promise<void> {
    if (!this.canManageProducts()) {
      return;
    }

    this.isDeleting.set(true);
    this.errorMessage.set(null);

    try {
      await this.productService.deleteAsync(productId);
      this.snackBar.open('Product deleted successfully.', 'Close', { duration: 2600 });

      const nextPageIndex = this.pageIndex();
      const currentItems = this.products();
      if (currentItems.length === 1 && nextPageIndex > 0) {
        this.pageIndex.set(nextPageIndex - 1);
      }

      await this.loadProductsAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to delete product.'));
    } finally {
      this.isDeleting.set(false);
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    void this.loadProductsAsync();
  }

  trackProduct(_: number, product: Product): number {
    return product.productId;
  }

  getStatusLabel(status: ProductStatus): string {
    return this.statusOptions.find(option => option.value === status)?.label ?? 'Unknown';
  }

  resetForm(): void {
    this.editingProductId.set(null);
    this.productForm.reset({
      sku: '',
      name: '',
      description: '',
      unitPrice: 0,
      currentStock: 0,
      reorderLevel: 0,
      status: ProductStatus.Active,
      categoryId: 0,
      supplierId: 0
    });
  }

  private readErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as ApiEnvelope<unknown> | null;

      if (apiError?.errors && Array.isArray(apiError.errors) && apiError.errors.length > 0) {
        return apiError.errors[0];
      }

      if (apiError?.message) {
        return apiError.message;
      }

      if (error.status === 0) {
        return 'Cannot reach server. Check API and proxy configuration.';
      }

      return `Request failed (${error.status}).`;
    }

    if (error instanceof Error && error.message.trim().length > 0) {
      return error.message;
    }

    return fallbackMessage;
  }
}
