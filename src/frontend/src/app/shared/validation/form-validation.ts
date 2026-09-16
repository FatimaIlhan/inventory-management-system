export const FIELD_LIMITS = {
  category: {
    name: 100,
    description: 500
  },
  product: {
    sku: 50,
    name: 100,
    description: 500
  },
  supplier: {
    companyName: 200,
    contactPerson: 100,
    phone: 20,
    email: 255,
    address: 300
  },
  purchaseOrder: {
    orderNumber: 50
  },
  user: {
    email: 256,
    password: 8
  }
} as const;

export const PASSWORD_COMPLEXITY_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;