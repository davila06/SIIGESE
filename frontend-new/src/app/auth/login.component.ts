import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

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
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly authService: AuthService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.resetPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    // Si el usuario ya está autenticado, redirigir al dashboard
    if (this.authService.isAuthenticated()) {
      console.log('✅ Usuario ya autenticado, redirigiendo a dashboard desde login');
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;
      
      this.authService.login(email, password).subscribe({
        next: (response) => {
          console.log('✅ Login exitoso:', response);
          this.router.navigate(['/dashboard']);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('❌ Error en login:', error);
          this.isLoading = false;
        }
      });
    }
  }

  onResetPassword(): void {
    if (this.resetPasswordForm.valid) {
      this.isResettingPassword = true;
      const { email } = this.resetPasswordForm.value;

      this.authService.resetPassword(email).subscribe({
        next: () => {
          this.showResetForm = false;
          this.resetPasswordForm.reset();
          this.isResettingPassword = false;
        },
        error: (err) => {
          console.error('❌ Error en recuperación de contraseña:', err);
          this.isResettingPassword = false;
        }
      });
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
