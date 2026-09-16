import { PagedResult } from './product.models';

export interface AuditLog {
  auditLogId: number;
  userId: number;
  userEmail: string;
  entityType: string;
  entityId: number | null;
  action: string;
  description: string;
  createdAtUtc: string;
}

export interface AuditLogListQuery {
  page: number;
  pageSize: number;
  search?: string;
  entityType?: string;
  action?: string;
  fromUtc?: string;
  toUtc?: string;
}

export type PagedAuditLogs = PagedResult<AuditLog>;