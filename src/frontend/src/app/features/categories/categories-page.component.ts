import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ApiEnvelope } from '../../core/services/auth.models';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/services/category.models';

@Component({
  selector: 'app-categories-page',
  standalone: true,
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
    MatSnackBarModule
  ],
  templateUrl: './categories-page.component.html',
  styleUrl: './categories-page.component.scss'
})
export class CategoriesPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly categoryService = inject(CategoryService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private loadRequestSequence = 0;

  readonly canManageCategories = computed(() => this.authService.canAccessRole(['Admin', 'Manager']));
  readonly displayedColumns = computed<string[]>(() =>
    this.canManageCategories()
      ? ['name', 'description', 'createdAtUtc', 'updatedAtUtc', 'actions']
      : ['name', 'description', 'createdAtUtc', 'updatedAtUtc']
  );

  readonly categories = signal<Category[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly editingCategoryId = signal<number | null>(null);
  readonly isFormModalOpen = signal(false);
  readonly pendingDeleteCategory = signal<Category | null>(null);

  readonly searchForm = this.formBuilder.nonNullable.group({
    search: ['']
  });

  readonly categoryForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]]
  });

  readonly searchControl = this.searchForm.controls.search;
  readonly nameControl = this.categoryForm.controls.name;
  readonly descriptionControl = this.categoryForm.controls.description;

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pageIndex.set(0);
        void this.loadCategoriesAsync();
      });

    void this.loadCategoriesAsync();
  }

  async loadCategoriesAsync(): Promise<void> {
    const requestId = ++this.loadRequestSequence;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const page = this.pageIndex() + 1;
    const pageSize = this.pageSize();
    const search = this.searchControl.value;

    try {
      let result = await this.categoryService.getPagedAsync({ page, pageSize, search });

      const shouldRetryEmptyResult =
        result.totalCount === 0 &&
        page === 1 &&
        search.trim().length === 0;

      if (shouldRetryEmptyResult) {
        // Retry once to avoid stale empty responses when navigating quickly.
        result = await this.categoryService.getPagedAsync({ page, pageSize, search });
      }

      if (requestId !== this.loadRequestSequence) {
        return;
      }

      if (!Array.isArray(result.items) || typeof result.totalCount !== 'number') {
        throw new Error('Invalid category response payload from API.');
      }

      this.categories.set(result.items);
      this.totalCount.set(result.totalCount);
    } catch (error: unknown) {
      if (requestId !== this.loadRequestSequence) {
        return;
      }

      this.errorMessage.set(this.readErrorMessage(error, 'Failed to load categories.'));
    } finally {
      if (requestId === this.loadRequestSequence) {
        this.isLoading.set(false);
      }
    }
  }

  async submitAsync(): Promise<void> {
    if (!this.canManageCategories()) {
      return;
    }

    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const formValue = this.categoryForm.getRawValue();
    const payload = {
      name: formValue.name.trim(),
      description: formValue.description.trim() || null
    };

    try {
      const editingId = this.editingCategoryId();
      if (editingId === null) {
        await this.categoryService.createAsync(payload);
        this.snackBar.open('Category created successfully.', 'Close', { duration: 2600 });
        this.pageIndex.set(0);
      } else {
        await this.categoryService.updateAsync(editingId, payload);
        this.snackBar.open('Category updated successfully.', 'Close', { duration: 2600 });
      }

      this.closeFormModal();
      await this.loadCategoriesAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to save category.'));
    } finally {
      this.isSaving.set(false);
    }
  }

  openCreateForm(): void {
    if (!this.canManageCategories()) {
      return;
    }

    this.editingCategoryId.set(null);
    this.categoryForm.reset({ name: '', description: '' });
    this.isFormModalOpen.set(true);
  }

  editCategory(category: Category): void {
    if (!this.canManageCategories()) {
      return;
    }

    this.editingCategoryId.set(category.id);
    this.categoryForm.setValue({
      name: category.name,
      description: category.description ?? ''
    });

    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    this.resetForm();
    this.isFormModalOpen.set(false);
  }

  requestDeleteCategory(category: Category): void {
    if (!this.canManageCategories()) {
      return;
    }

    this.pendingDeleteCategory.set(category);
  }

  cancelDelete(): void {
    if (this.isDeleting()) {
      return;
    }

    this.pendingDeleteCategory.set(null);
  }

  async confirmDeleteAsync(): Promise<void> {
    const category = this.pendingDeleteCategory();
    if (!category || !this.canManageCategories()) {
      return;
    }

    this.isDeleting.set(true);
    this.errorMessage.set(null);

    try {
      await this.categoryService.deleteAsync(category.id);
      this.snackBar.open('Category deleted successfully.', 'Close', { duration: 2600 });
      this.pendingDeleteCategory.set(null);

      const nextPageIndex = this.pageIndex();
      const currentItems = this.categories();
      if (currentItems.length === 1 && nextPageIndex > 0) {
        this.pageIndex.set(nextPageIndex - 1);
      }

      await this.loadCategoriesAsync();
    } catch (error: unknown) {
      this.errorMessage.set(this.readErrorMessage(error, 'Failed to delete category.'));
    } finally {
      this.isDeleting.set(false);
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    void this.loadCategoriesAsync();
  }

  trackCategory(_: number, category: Category): number {
    return category.id;
  }

  private resetForm(): void {
    this.editingCategoryId.set(null);
    this.categoryForm.reset({ name: '', description: '' });
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
