import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { AuthService } from './core/services/auth.service';
import { SeoService } from './core/services/seo.service';

const authServiceMock = {
  loadCurrentUser: () => Promise.resolve(),
  isAuthenticated: () => false,
  refreshSession: () => Promise.resolve(false)
};

const seoServiceMock = {
  initializeRouteMetadataSync: () => undefined
};

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: SeoService, useValue: seoServiceMock }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
