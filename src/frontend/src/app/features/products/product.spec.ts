import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { AuthService } from '../../core/services/auth.service';
import { CategoryService } from '../../core/services/category.service';
import { Product, ProductStatus } from '../../core/services/product.models';
import { ProductService } from '../../core/services/product.service';
import { SupplierService } from '../../core/services/supplier.service';
import { ConfirmationService } from '../../shared/services/confirmation.service';
import { NotificationService } from '../../shared/services/notification.service';
import { ProductComponent } from './product';

describe('ProductComponent', () => {
  const product: Product = {
    productId: 1,
    sku: 'LAPTOP-15',
    name: 'Business Laptop 15-inch',
    unitPrice: 800,
    currentStock: 10,
    reorderLevel: 2,
    status: ProductStatus.Active,
    categoryId: 201,
    supplierId: 301,
    categoryName: 'Computers',
    supplierName: 'Business Electronics',
    createdAtUtc: new Date('2026-10-01T10:00:00Z')
  };
  const productServiceMock = {
    getPagedAsync: async () => ({ items: [product], totalCount: 1 }),
    createAsync: vi.fn(async () => product),
    updateAsync: vi.fn(async () => product)
  };
  const authServiceMock = { canAccessRole: vi.fn(() => true) };

  beforeEach(async () => {
    vi.clearAllMocks();
    authServiceMock.canAccessRole.mockReturnValue(true);
    await TestBed.configureTestingModule({
      imports: [ProductComponent],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: ProductService, useValue: productServiceMock },
        { provide: CategoryService, useValue: { getPagedAsync: async () => ({ items: [] }) } },
        { provide: SupplierService, useValue: { getPagedAsync: async () => ({ items: [] }) } },
        { provide: ConfirmationService, useValue: {} },
        { provide: NotificationService, useValue: { success: vi.fn() } }
      ]
    }).compileComponents();
  });

  it('accepts names with numbers and hyphens while retaining required and length validation', () => {
    const component = TestBed.createComponent(ProductComponent).componentInstance;

    for (const name of ['Business Laptop 15-inch', 'Café Laptop 15-inch', "Vendor's Laptop 2.0"]) {
      component.nameControl.setValue(name);
      expect(component.nameControl.valid).toBe(true);
    }

    component.nameControl.setValue('');
    expect(component.nameControl.hasError('required')).toBe(true);
    component.nameControl.setValue('A'.repeat(101));
    expect(component.nameControl.hasError('maxlength')).toBe(true);
    component.nameControl.setValue('Laptop@15');
    expect(component.nameControl.hasError('pattern')).toBe(true);
  });

  it('submits the requested name and reference IDs when creating and editing', async () => {
    const component = TestBed.createComponent(ProductComponent).componentInstance;
    component.productForm.patchValue({
      sku: product.sku,
      name: product.name,
      unitPrice: product.unitPrice,
      categoryId: product.categoryId,
      supplierId: product.supplierId
    });

    await component.submitAsync();
    expect(productServiceMock.createAsync).toHaveBeenCalledWith(expect.objectContaining({
      name: 'Business Laptop 15-inch',
      categoryId: 201,
      supplierId: 301
    }));

    component.editProduct(product);
    await component.submitAsync();
    expect(productServiceMock.updateAsync).toHaveBeenCalledWith(1, expect.objectContaining({
      name: 'Business Laptop 15-inch',
      categoryId: 201,
      supplierId: 301
    }));
  });

  for (const canManage of [true, false]) {
    it(`displays reference names without dropdown lookups for ${canManage ? 'managers' : 'staff'}`, async () => {
      authServiceMock.canAccessRole.mockReturnValue(canManage);
      const fixture = TestBed.createComponent(ProductComponent);
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();

      const element: HTMLElement = fixture.nativeElement;
      expect(element.querySelector('td.mat-column-categoryName')?.textContent?.trim()).toBe('Computers');
      expect(element.querySelector('td.mat-column-supplierName')?.textContent?.trim()).toBe('Business Electronics');
      expect(fixture.componentInstance.categories()).toEqual([]);
      expect(fixture.componentInstance.suppliers()).toEqual([]);
    });
  }
});
