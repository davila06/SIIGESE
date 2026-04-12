import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { LoggingService } from '../services/logging.service';
import { environment } from '../../environments/environment';

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
  loginError = '';
  readonly isDevelopment = !environment.production;
  readonly currentYear = new Date().getFullYear();

  private readonly logger = inject(LoggingService);

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
    // Si el usuario ya estÃ¡ autenticado, redirigir al dashboard
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.loginError = '';
      const { email, password } = this.loginForm.value;
      
      this.authService.login(email, password).subscribe({
        next: (response) => {
          this.router.navigate(['/dashboard']);
          this.isLoading = false;
        },
        error: (error) => {
          this.logger.error('âŒ Error en login:', error);
          this.loginError = error?.error?.message || 'No se pudo iniciar sesión. Verifica tus credenciales.';
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
          this.logger.error('âŒ Error en recuperaciÃ³n de contraseÃ±a:', err);
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
      return `${field === 'email' ? 'Email' : 'ContraseÃ±a'} es requerido`;
    }
    if (control?.hasError('email')) {
      return 'Email no vÃ¡lido';
    }
    if (control?.hasError('minlength')) {
      return 'La contraseÃ±a debe tener al menos 6 caracteres';
    }
    return '';
  }
}
