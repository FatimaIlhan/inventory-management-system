export interface Supplier {
    supplierId: number;
    companyName: string;
    contactPerson: string;
    phone: string;
    email: string;
    address: string;
    createdAtUtc: Date;
    updatedAtUtc?: Date;
}
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
export interface SupplierListQuery {
  page: number;
  pageSize: number;
  search?: string;
}
export interface CreateSupplierRequest {
  companyName: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
}
export interface UpdateSupplierRequest {
  companyName: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
}