import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  resetPasswordForm: FormGroup;
  isLoading = false;
  isResettingPassword = false;
  hidePassword = true;
  showResetForm = false;

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['admin@sinseg.com', [Validators.required, Validators.email]],
      password: ['password123', [Validators.required, Validators.minLength(6)]]
    });

    this.resetPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    // Mock authentication check
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      // Mock login - redirect to reclamos after a short delay
      setTimeout(() => {
        this.router.navigate(['/reclamos']);
        this.isLoading = false;
      }, 1000);
    }
  }

  onResetPassword(): void {
    if (this.resetPasswordForm.valid) {
      this.isResettingPassword = true;
      // Mock reset password
      setTimeout(() => {
        this.showResetForm = false;
        this.resetPasswordForm.reset();
        this.isResettingPassword = false;
      }, 1000);
    }
  }

  toggleResetForm(): void {
    this.showResetForm = !this.showResetForm;
    if (this.showResetForm) {
      this.resetPasswordForm.reset();
    }
  }

  getErrorMessage(field: string): string {
    const form = this.showResetForm ? this.resetPasswordForm : this.loginForm;
    const control = form.get(field);
    if (control?.hasError('required')) {
      return `${field === 'email' ? 'Email' : 'Contraseña'} es requerido`;
    }
    if (control?.hasError('email')) {
      return 'Email no válido';
    }
    if (control?.hasError('minlength')) {
      return 'La contraseña debe tener al menos 6 caracteres';
    }
    return '';
  }
}
