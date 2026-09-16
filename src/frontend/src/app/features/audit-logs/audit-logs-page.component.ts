import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
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
import { AuditLog } from '../../core/services/audit-log.models';
import { AuditLogService } from '../../core/services/audit-log.service';

@Component({
  selector: 'app-audit-logs-page',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatPaginatorModule, MatProgressBarModule, MatSelectModule, MatTableModule],
  templateUrl: './audit-logs-page.component.html',
  styleUrl: './audit-logs-page.component.scss'
})
export class AuditLogsPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auditLogService = inject(AuditLogService);
  private readonly destroyRef = inject(DestroyRef);
  private loadRequestSequence = 0;

  readonly displayedColumns = ['createdAtUtc', 'userEmail', 'entityType', 'action', 'description'];
  readonly auditLogs = signal<AuditLog[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly filterForm = this.formBuilder.nonNullable.group({
    search: [''],
    entityType: [''],
    action: [''],
    fromDate: [''],
    toDate: ['']
  });
  readonly entityTypes = ['Authentication', 'Product', 'Category', 'Supplier', 'PurchaseOrder', 'StockMovement'];
  readonly actions = ['Login', 'Created', 'Updated', 'Deleted', 'Submitted', 'Received'];

  ngOnInit(): void {
    this.filterForm.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pageIndex.set(0);
        void this.loadAuditLogsAsync();
      });

    void this.loadAuditLogsAsync();
  }

  async loadAuditLogsAsync(): Promise<void> {
    const requestId = ++this.loadRequestSequence;
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const value = this.filterForm.getRawValue();

    try {
      const result = await this.auditLogService.getPagedAsync({
        page: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        search: value.search,
        entityType: value.entityType,
        action: value.action,
        fromUtc: value.fromDate ? `${value.fromDate}T00:00:00.000Z` : undefined,
        toUtc: value.toDate ? `${value.toDate}T23:59:59.999Z` : undefined
      });

      if (requestId === this.loadRequestSequence) {
        this.auditLogs.set(result.items);
        this.totalCount.set(result.totalCount);
      }
    } catch (error: unknown) {
      if (requestId === this.loadRequestSequence) {
        this.errorMessage.set(this.readErrorMessage(error));
      }
    } finally {
      if (requestId === this.loadRequestSequence) {
        this.isLoading.set(false);
      }
    }
  }

  clearFilters(): void {
    this.filterForm.reset({ search: '', entityType: '', action: '', fromDate: '', toDate: '' });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    void this.loadAuditLogsAsync();
  }

  trackAuditLog(_: number, auditLog: AuditLog): number {
    return auditLog.auditLogId;
  }

  private readErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as ApiEnvelope<unknown> | null;
      return apiError?.errors?.[0] ?? apiError?.message ?? 'Failed to load audit history.';
    }

    return 'Failed to load audit history.';
  }
}