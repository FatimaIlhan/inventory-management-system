import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
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
import { Product, ProductStatus } from '../../core/services/product.models';
import { ProductService } from '../../core/services/product.service';
import {
  PurchaseOrder,
  PurchaseOrderListQuery,
  PurchaseOrderSortField,
  PurchaseOrderStatus
} from '../../core/services/purchase-order.models';
import { PurchaseOrderService } from '../../core/services/purchase-order.service';
import { Supplier } from '../../core/services/supplier.models';
import { SupplierService } from '../../core/services/supplier.service';
import { ConfirmationService } from '../../shared/services/confirmation.service';
import { NotificationService } from '../../shared/services/notification.service';
import { FIELD_LIMITS } from '../../shared/validation/form-validation';

@Component({
  selector: 'app-purchase-orders-page',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatTableModule
  ],
  templateUrl: './purchase-orders-page.component.html',
  styleUrl: './purchase-orders-page.component.scss'
})
export class PurchaseOrdersPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly purchaseOrderService = inject(PurchaseOrderService);
  private readonly productService = inject(ProductService);
  private readonly supplierService = inject(SupplierService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private loadRequestSequence = 0;

  readonly canManagePurchaseOrders = computed(() => this.authService.canAccessRole(['Admin', 'Manager']));
  readonly displayedColumns = computed(() => this.canManagePurchaseOrders()
    ? ['orderNumber', 'supplierName', 'status', 'createdAtUtc', 'submittedAtUtc', 'receivedAtUtc', 'actions']
    : ['orderNumber', 'supplierName', 'status', 'createdAtUtc', 'submittedAtUtc', 'receivedAtUtc']);
  readonly purchaseOrders = signal<PurchaseOrder[]>([]);
  readonly products = signal<Product[]>([]);
  readonly suppliers = signal<Supplier[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly formErrorMessage = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly isTransitioning = signal(false);
  readonly editingPurchaseOrderId = signal<number | null>(null);
  readonly selectedPurchaseOrder = signal<PurchaseOrder | null>(null);
  readonly isFormModalOpen = signal(false);

  readonly filtersForm = this.formBuilder.nonNullable.group({
    search: [''],
    sortBy: ['createdAtUtc' as PurchaseOrderSortField],
    descending: [true]
  });

  readonly purchaseOrderForm = this.formBuilder.nonNullable.group({
    orderNumber: ['', [Validators.required, Validators.maxLength(FIELD_LIMITS.purchaseOrder.orderNumber)]],
    supplierId: [0, [Validators.required, Validators.min(1)]],
    items: this.formBuilder.array([this.createItemFormGroup()])
  });

  readonly searchControl = this.filtersForm.controls.search;
  readonly sortByControl = this.filtersForm.controls.sortBy;
  readonly descendingControl = this.filtersForm.controls.descending;
  readonly orderNumberControl = this.purchaseOrderForm.controls.orderNumber;
  readonly supplierIdControl = this.purchaseOrderForm.controls.supplierId;
  readonly sortOptions: ReadonlyArray<{ value: PurchaseOrderSortField; label: string }> = [
    { value: 'createdAtUtc', label: 'Created date' },
    { value: 'orderNumber', label: 'Order number' },
    { value: 'supplierName', label: 'Supplier' },
    { value: 'status', label: 'Status' },
    { value: 'submittedAtUtc', label: 'Submitted date' },
    { value: 'receivedAtUtc', label: 'Received date' }
  ];

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.resetToFirstPageAndLoad());

    void this.loadPurchaseOrdersAsync();
    void this.loadReferencesAsync();
  }

  get itemForms(): FormArray {
    return this.purchaseOrderForm.controls.items;
  }

  async loadReferencesAsync(): Promise<void> {
    try {
      const [productResult, supplierResult] = await Promise.all([
        this.productService.getPagedAsync({ page: 1, pageSize: 100, status: ProductStatus.Active }),
        this.supplierService.getPagedAsync({ page: 1, pageSize: 100 })
      ]);
      this.products.set(productResult.items);
      this.suppliers.set(supplierResult.items);
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to load products and suppliers.'));
    }
  }

  async loadPurchaseOrdersAsync(): Promise<void> {
    const requestId = ++this.loadRequestSequence;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const query: PurchaseOrderListQuery = {
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        search: this.searchControl.value,
        sortBy: this.sortByControl.value,
        descending: this.descendingControl.value
      };
      const result = await this.purchaseOrderService.getPagedAsync(query);

      if (requestId !== this.loadRequestSequence) {
        return;
      }

      if (!Array.isArray(result.items) || typeof result.totalCount !== 'number') {
        throw new Error('Invalid purchase order response payload from API.');
      }

      this.purchaseOrders.set(result.items);
      this.totalCount.set(result.totalCount);
    } catch (error: unknown) {
      if (requestId === this.loadRequestSequence) {
        this.errorMessage.set(this.readErrorMessage(error, 'Failed to load purchase orders.'));
      }
    } finally {
      if (requestId === this.loadRequestSequence) {
        this.isLoading.set(false);
      }
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    void this.loadPurchaseOrdersAsync();
  }

  openCreateForm(): void {
    if (!this.canManagePurchaseOrders()) {
      return;
    }

    this.editingPurchaseOrderId.set(null);
    this.resetForm();
    this.isFormModalOpen.set(true);
  }

  editPurchaseOrder(purchaseOrder: PurchaseOrder): void {
    if (!this.canManagePurchaseOrders() || !this.isDraft(purchaseOrder)) {
      return;
    }

    this.editingPurchaseOrderId.set(purchaseOrder.purchaseOrderId);
    this.purchaseOrderForm.controls.orderNumber.setValue(purchaseOrder.orderNumber);
    this.purchaseOrderForm.controls.supplierId.setValue(purchaseOrder.supplierId);
    this.purchaseOrderForm.controls.items.clear();
    purchaseOrder.items.forEach((item) => this.itemForms.push(this.createItemFormGroup(item.productId, item.quantity, item.unitPrice)));
    this.formErrorMessage.set(null);
    this.isFormModalOpen.set(true);
  }

  viewPurchaseOrder(purchaseOrder: PurchaseOrder): void {
    this.selectedPurchaseOrder.set(purchaseOrder);
  }

  closeFormModal(): void {
    if (!this.isSaving()) {
      this.isFormModalOpen.set(false);
      this.resetForm();
    }
  }

  addItem(): void {
    this.itemForms.push(this.createItemFormGroup());
  }

  removeItem(index: number): void {
    if (this.itemForms.length > 1) {
      this.itemForms.removeAt(index);
    }
  }

  lineTotal(index: number): number {
    const item = this.itemForms.at(index).getRawValue();
    return Number(item.quantity) * Number(item.unitPrice);
  }

  hasItemError(index: number, controlName: 'productId' | 'quantity' | 'unitPrice'): boolean {
    const control = this.itemForms.at(index).get(controlName);
    return control !== null && control.invalid && control.touched;
  }

  orderTotal(): number {
    return this.itemForms.controls.reduce((total, _, index) => total + this.lineTotal(index), 0);
  }

  async submitAsync(): Promise<void> {
    if (!this.canManagePurchaseOrders()) {
      return;
    }

    if (this.purchaseOrderForm.invalid) {
      this.purchaseOrderForm.markAllAsTouched();
      return;
    }

    const formValue = this.purchaseOrderForm.getRawValue();
    const productIds = formValue.items.map((item) => Number(item.productId));
    if (new Set(productIds).size !== productIds.length) {
      this.formErrorMessage.set('A product can appear only once in a purchase order.');
      return;
    }

    this.isSaving.set(true);
    this.formErrorMessage.set(null);
    const request = {
      orderNumber: formValue.orderNumber.trim(),
      supplierId: Number(formValue.supplierId),
      items: formValue.items.map((item) => ({
        productId: Number(item.productId),
        quantity: Number(item.quantity),
        unitPrice: Number(item.unitPrice)
      }))
    };

    try {
      const editingId = this.editingPurchaseOrderId();
      if (editingId === null) {
        await this.purchaseOrderService.createAsync(request);
        this.pageIndex.set(0);
        this.notificationService.success('Purchase order created successfully.');
      } else {
        await this.purchaseOrderService.updateAsync(editingId, request);
        this.notificationService.success('Purchase order updated successfully.');
      }
      this.isFormModalOpen.set(false);
      this.resetForm();
      await this.loadPurchaseOrdersAsync();
    } catch (error: unknown) {
      this.formErrorMessage.set(this.readErrorMessage(error, 'Failed to save purchase order.'));
    } finally {
      this.isSaving.set(false);
    }
  }

  async confirmDelete(purchaseOrder: PurchaseOrder): Promise<void> {
    if (!this.canManagePurchaseOrders() || !this.isDraft(purchaseOrder) || this.isDeleting()) {
      return;
    }

    const isConfirmed = await this.confirmationService.confirm({
      title: 'Delete Purchase Order',
      message: `Delete draft order ${purchaseOrder.orderNumber}? This action cannot be undone.`,
      confirmLabel: 'Delete',
      tone: 'danger'
    });

    if (isConfirmed) {
      await this.deletePurchaseOrderAsync(purchaseOrder);
    }
  }

  private async deletePurchaseOrderAsync(purchaseOrder: PurchaseOrder): Promise<void> {
    if (!this.isDraft(purchaseOrder)) {
      return;
    }

    this.isDeleting.set(true);
    this.errorMessage.set(null);
    try {
      await this.purchaseOrderService.deleteAsync(purchaseOrder.purchaseOrderId);
      this.notificationService.success('Purchase order deleted successfully.');
      if (this.purchaseOrders().length === 1 && this.pageIndex() > 0) {
        this.pageIndex.update((pageIndex) => pageIndex - 1);
      }
      await this.loadPurchaseOrdersAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to delete purchase order.'));
    } finally {
      this.isDeleting.set(false);
    }
  }

  async submitPurchaseOrderAsync(purchaseOrder: PurchaseOrder): Promise<void> {
    if (!this.canManagePurchaseOrders() || !this.isDraft(purchaseOrder)) {
      return;
    }

    await this.applyStatusTransitionAsync(
      purchaseOrder,
      () => this.purchaseOrderService.submitAsync(purchaseOrder.purchaseOrderId),
      'Purchase order submitted successfully.',
      'Failed to submit purchase order.');
  }

  async receivePurchaseOrderAsync(purchaseOrder: PurchaseOrder): Promise<void> {
    if (!this.canManagePurchaseOrders() || !this.isSubmitted(purchaseOrder)) {
      return;
    }

    const isConfirmed = await this.confirmationService.confirm({
      title: 'Receive Purchase Order',
      message: `Receive ${purchaseOrder.orderNumber}? Product stock will be updated.`,
      confirmLabel: 'Receive order',
      tone: 'primary'
    });

    if (!isConfirmed) {
      return;
    }

    await this.applyStatusTransitionAsync(
      purchaseOrder,
      () => this.purchaseOrderService.receiveAsync(purchaseOrder.purchaseOrderId),
      'Purchase order received successfully. Stock has been updated.',
      'Failed to receive purchase order.');
  }

  trackPurchaseOrder(_: number, purchaseOrder: PurchaseOrder): number {
    return purchaseOrder.purchaseOrderId;
  }

  getStatusLabel(status: PurchaseOrderStatus): string {
    return PurchaseOrderStatus[status] ?? 'Unknown';
  }

  isDraft(purchaseOrder: PurchaseOrder): boolean {
    return purchaseOrder.status === PurchaseOrderStatus.Draft;
  }

  isSubmitted(purchaseOrder: PurchaseOrder): boolean {
    return purchaseOrder.status === PurchaseOrderStatus.Submitted;
  }

  private async applyStatusTransitionAsync(
    purchaseOrder: PurchaseOrder,
    transition: () => Promise<PurchaseOrder>,
    successMessage: string,
    failureMessage: string): Promise<void> {
    this.isTransitioning.set(true);
    this.errorMessage.set(null);

    try {
      const updatedPurchaseOrder = await transition();
      this.purchaseOrders.update((purchaseOrders) => purchaseOrders.map((item) =>
        item.purchaseOrderId === updatedPurchaseOrder.purchaseOrderId ? updatedPurchaseOrder : item));

      if (this.selectedPurchaseOrder()?.purchaseOrderId === updatedPurchaseOrder.purchaseOrderId) {
        this.selectedPurchaseOrder.set(updatedPurchaseOrder);
      }

      this.notificationService.success(successMessage);
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, failureMessage));
    } finally {
      this.isTransitioning.set(false);
    }
  }

  private createItemFormGroup(productId = 0, quantity = 1, unitPrice = 0) {
    return this.formBuilder.nonNullable.group({
      productId: [productId, [Validators.required, Validators.min(1)]],
      quantity: [quantity, [Validators.required, Validators.min(1)]],
      unitPrice: [unitPrice, [Validators.required, Validators.min(0.01)]]
    });
  }

  private resetForm(): void {
    this.purchaseOrderForm.reset({ orderNumber: '', supplierId: 0 });
    this.itemForms.clear();
    this.itemForms.push(this.createItemFormGroup());
    this.formErrorMessage.set(null);
  }

  private resetToFirstPageAndLoad(): void {
    this.pageIndex.set(0);
    void this.loadPurchaseOrdersAsync();
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