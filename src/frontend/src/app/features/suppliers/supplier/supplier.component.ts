import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { SupplierService } from '../../../core/services/supplier.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { AuthService } from '../../../core/services/auth.service';
import { Supplier } from '../../../core/services/supplier.models';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { ApiEnvelope } from '../../../core/services/auth.models';
import { HttpErrorResponse } from '@angular/common/http';
@Component({
  selector: 'app-supplier',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSnackBarModule],
  templateUrl: './supplier.component.html',
  styleUrl: './supplier.component.scss',
})
export class SupplierComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly supplierService = inject(SupplierService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private loadRequestSequence = 0;

  readonly canManageSuppliers = computed(() => this.authService.canAccessRole(['Admin', 'Manager']));
  readonly displayedColumns = computed<string[]>(() =>
    this.canManageSuppliers()
      ? ['companyName', 'contactPerson', 'phone', 'email', 'address', 'createdAtUtc', 'updatedAtUtc', 'actions']
      : ['companyName', 'contactPerson', 'phone', 'email', 'address', 'createdAtUtc', 'updatedAtUtc']
  );

  readonly suppliers = signal<Supplier[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly editingSupplierId = signal<number | null>(null);
  readonly isFormModalOpen = signal(false);
  readonly pendingDeleteSupplier = signal<Supplier | null>(null);

  readonly searchForm = this.formBuilder.nonNullable.group({
    search: ['']
  });

  readonly supplierForm = this.formBuilder.nonNullable.group({
    companyName: ['', [Validators.required, Validators.maxLength(100)]],
    contactPerson: ['', [Validators.maxLength(500)]],
    phone: ['', [Validators.maxLength(20)]],
    email: ['', [Validators.email, Validators.maxLength(100)]],
    address: ['', [Validators.maxLength(200)]]
  });
  readonly searchControl = this.searchForm.controls.search;
  readonly companyNameControl = this.supplierForm.controls.companyName;
  readonly contactPersonControl = this.supplierForm.controls.contactPerson;
  readonly phoneControl = this.supplierForm.controls.phone;
  readonly emailControl = this.supplierForm.controls.email;
  readonly addressControl = this.supplierForm.controls.address;


  ngOnInit(): void {
     this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pageIndex.set(0);
         void this.loadSuppliersAsync();
      });

    void this.loadSuppliersAsync();
  }

  async loadSuppliersAsync(): Promise<void> {
const requestId = ++this.loadRequestSequence;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const page = this.pageIndex() + 1;
    const pageSize = this.pageSize();
    const search = this.searchControl.value;

    try {
      let result = await this.supplierService.getPagedAsync({ page, pageSize, search });

      const shouldRetryEmptyResult =
        result.totalCount === 0 &&
        page === 1 &&
        search.trim().length === 0;

      if (shouldRetryEmptyResult) {
        // Retry once to avoid stale empty responses when navigating quickly.
        result = await this.supplierService.getPagedAsync({ page, pageSize, search });
      }

      if (requestId !== this.loadRequestSequence) {
        return;
      }

      if (!Array.isArray(result.items) || typeof result.totalCount !== 'number') {
        throw new Error('Invalid supplier response payload from API.');
      }

      this.suppliers.set(result.items);
      this.totalCount.set(result.totalCount);
    } catch (error: unknown) {
      if (requestId !== this.loadRequestSequence) {
        return;
      }

      this.errorMessage.set(this.readErrorMessage(error, 'Failed to load suppliers.'));
    } finally {
      if (requestId === this.loadRequestSequence) {
        this.isLoading.set(false);
      }
    }
  }

async submitAsync(): Promise<void> {
    if (!this.canManageSuppliers()) {
      return;
    }

    if (this.supplierForm.invalid) {
      this.supplierForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const formValue = this.supplierForm.getRawValue();
    const payload = {
      companyName: formValue.companyName.trim(),
      contactPerson: formValue.contactPerson.trim(),
      phone: formValue.phone.trim(),
      email: formValue.email.trim(),
      address: formValue.address.trim()
    };

    try {
      const editingId = this.editingSupplierId();
      if (editingId === null) {
        await this.supplierService.createAsync(payload);
        this.snackBar.open('Supplier created successfully.', 'Close', { duration: 2600 });
        this.pageIndex.set(0);
      } else {
        await this.supplierService.updateAsync(editingId, payload);
        this.snackBar.open('Supplier updated successfully.', 'Close', { duration: 2600 });
      }

      this.closeFormModal();
      await this.loadSuppliersAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to save supplier.'));
    } finally {
      this.isSaving.set(false);
    }
  }

  openCreateForm(): void {
    if (!this.canManageSuppliers()) {
      return;
    }

    this.editingSupplierId.set(null);
    this.supplierForm.reset({ companyName: '', contactPerson: '', phone: '', email: '', address: '' });
    this.isFormModalOpen.set(true);
  }

  editSupplier(supplier: Supplier): void {
    if (!this.canManageSuppliers()) {
      return;
    }

    this.editingSupplierId.set(supplier.supplierId);
    this.supplierForm.setValue({
      companyName: supplier.companyName,
      contactPerson: supplier.contactPerson ?? '',
      phone: supplier.phone ?? '',
      email: supplier.email ?? '',
      address: supplier.address ?? ''
    });

    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    this.resetForm();
    this.isFormModalOpen.set(false);
  }

  requestDeleteSupplier(supplier: Supplier): void {
    if (!this.canManageSuppliers()) {
      return;
    }

    this.pendingDeleteSupplier.set(supplier);
  }

  cancelDelete(): void {
    if (this.isDeleting()) {
      return;
    }

    this.pendingDeleteSupplier.set(null);
  }

  async confirmDeleteAsync(): Promise<void> {
    const supplier = this.pendingDeleteSupplier();
    if (!supplier || !this.canManageSuppliers()) {
      return;
    }

    this.isDeleting.set(true);
    this.errorMessage.set(null);

    try {
      await this.supplierService.deleteAsync(supplier.supplierId);
      this.snackBar.open('Supplier deleted successfully.', 'Close', { duration: 2600 });
      this.pendingDeleteSupplier.set(null);

      const nextPageIndex = this.pageIndex();
      const currentItems = this.suppliers();
      if (currentItems.length === 1 && nextPageIndex > 0) {
        this.pageIndex.set(nextPageIndex - 1);
      }

      await this.loadSuppliersAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to delete supplier.'));
    } finally {
      this.isDeleting.set(false);
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    void this.loadSuppliersAsync();
  }

  trackSupplier(_: number, supplier: Supplier): number {
    return supplier.supplierId;
  }

  private resetForm(): void {
    this.editingSupplierId.set(null);
    this.supplierForm.reset({ companyName: '', contactPerson: '', phone: '', email: '', address: '' });
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
