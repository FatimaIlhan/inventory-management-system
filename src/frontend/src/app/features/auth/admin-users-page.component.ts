import { Component, signal, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/services/auth.models';

@Component({
  selector: 'app-admin-users-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-users-page.component.html',
  styleUrl: './admin-users-page.component.scss'
})
export class AdminUsersPageComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly isSubmitting = signal(false);
  readonly resultMessage = signal<string | null>(null);
  readonly roles: UserRole[] = ['Admin', 'Manager', 'Employee'];

  readonly createUserForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Employee' as UserRole, [Validators.required]]
  });

  async createUser(): Promise<void> {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    this.resultMessage.set(null);
    this.isSubmitting.set(true);

    try {
      const createdUser = await this.authService.createUser(this.createUserForm.getRawValue());
      this.resultMessage.set(`Created user ${createdUser.email} with ${createdUser.role} role.`);
      this.createUserForm.patchValue({ password: '' });
    } catch {
      this.resultMessage.set('Failed to create user. Verify your inputs and permissions.');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
