import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
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
import { MatTableModule } from '@angular/material/table';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { ApiEnvelope } from '../../core/services/auth.models';
import { AuthService } from '../../core/services/auth.service';
import {
  InventoryMovement,
  StockMovementType
} from '../../core/services/inventory-movement.models';
import { InventoryMovementService } from '../../core/services/inventory-movement.service';
import { Product, ProductStatus } from '../../core/services/product.models';
import { ProductService } from '../../core/services/product.service';
import { ConfirmationService } from '../../shared/services/confirmation.service';
import { NotificationService } from '../../shared/services/notification.service';

@Component({
  selector: 'app-stock-movements-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatTableModule
  ],
  templateUrl: './stock-movements-page.component.html',
  styleUrl: './stock-movements-page.component.scss'
})
export class StockMovementsPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly inventoryMovementService = inject(InventoryMovementService);
  private readonly productService = inject(ProductService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private loadRequestSequence = 0;

  readonly canRecordMovements = computed(() => this.authService.canAccessRole(['Admin', 'Manager']));
  readonly displayedColumns = ['createdAtUtc', 'productName', 'movementType', 'quantity', 'previousStock', 'newStock', 'performedByUserName', 'reason'];
  readonly movements = signal<InventoryMovement[]>([]);
  readonly products = signal<Product[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isFormModalOpen = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly formErrorMessage = signal<string | null>(null);

  readonly searchForm = this.formBuilder.nonNullable.group({ search: [''] });
  readonly movementForm = this.formBuilder.nonNullable.group({
    productId: [0, [Validators.required, Validators.min(1)]],
    movementType: [StockMovementType.StockIn, [Validators.required]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    reason: ['', [Validators.required, Validators.maxLength(500)]]
  });

  readonly searchControl = this.searchForm.controls.search;
  readonly productIdControl = this.movementForm.controls.productId;
  readonly movementTypeControl = this.movementForm.controls.movementType;
  readonly quantityControl = this.movementForm.controls.quantity;
  readonly reasonControl = this.movementForm.controls.reason;
  readonly movementTypes = [
    { value: StockMovementType.StockIn, label: 'Stock in' },
    { value: StockMovementType.StockOut, label: 'Stock out' },
    { value: StockMovementType.Adjustment, label: 'Set stock level' }
  ];

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pageIndex.set(0);
        void this.loadMovementsAsync();
      });

    void this.loadMovementsAsync();
    void this.loadProductsAsync();
  }

  async loadMovementsAsync(): Promise<void> {
    const requestId = ++this.loadRequestSequence;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const result = await this.inventoryMovementService.getPagedAsync({
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        search: this.searchControl.value,
        sortBy: 'createdAtUtc',
        descending: true
      });

      if (requestId !== this.loadRequestSequence) {
        return;
      }

      this.movements.set(result.items);
      this.totalCount.set(result.totalCount);
    } catch (error: unknown) {
      if (requestId === this.loadRequestSequence) {
        this.errorMessage.set(this.readErrorMessage(error, 'Failed to load stock movements.'));
      }
    } finally {
      if (requestId === this.loadRequestSequence) {
        this.isLoading.set(false);
      }
    }
  }

  async loadProductsAsync(): Promise<void> {
    try {
      const result = await this.productService.getPagedAsync({ page: 1, pageSize: 100, status: ProductStatus.Active });
      this.products.set(result.items);
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to load active products.'));
    }
  }

  openCreateForm(): void {
    if (!this.canRecordMovements()) {
      return;
    }

    this.formErrorMessage.set(null);
    this.movementForm.reset({
      productId: 0,
      movementType: StockMovementType.StockIn,
      quantity: 1,
      reason: ''
    });
    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    if (!this.isSaving()) {
      this.formErrorMessage.set(null);
      this.isFormModalOpen.set(false);
    }
  }

  async submitAsync(): Promise<void> {
    if (!this.canRecordMovements()) {
      return;
    }

    if (this.movementForm.invalid) {
      this.movementForm.markAllAsTouched();
      return;
    }

    const value = this.movementForm.getRawValue();

    if (value.movementType !== StockMovementType.StockIn) {
      const movementLabel = value.movementType === StockMovementType.StockOut ? 'stock out' : 'stock adjustment';
      const isConfirmed = await this.confirmationService.confirm({
        title: 'Confirm Stock Change',
        message: `Record a ${movementLabel} of ${value.quantity} for the selected product?`,
        confirmLabel: 'Record change',
        tone: 'primary'
      });

      if (!isConfirmed) {
        return;
      }
    }

    this.isSaving.set(true);
    this.formErrorMessage.set(null);

    try {
      await this.inventoryMovementService.createAsync({
        productId: Number(value.productId),
        movementType: Number(value.movementType),
        quantity: Number(value.quantity),
        reason: value.reason.trim()
      });
      this.notificationService.success('Stock movement recorded successfully.');
      this.isFormModalOpen.set(false);
      this.pageIndex.set(0);
      await this.loadMovementsAsync();
    } catch (error: unknown) {
      this.formErrorMessage.set(this.readErrorMessage(error, 'Failed to record stock movement.'));
    } finally {
      this.isSaving.set(false);
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    void this.loadMovementsAsync();
  }

  trackMovement(_: number, movement: InventoryMovement): number {
    return movement.inventoryMovementId;
  }

  getMovementTypeLabel(movementType: StockMovementType): string {
    return this.movementTypes.find((type) => type.value === movementType)?.label ?? 'Unknown';
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
      return error.status === 0 ? 'Cannot reach server. Check API and proxy configuration.' : `Request failed (${error.status}).`;
    }

    return error instanceof Error && error.message.trim().length > 0 ? error.message : fallbackMessage;
  }
}