import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService, LoginResponse, ResetPasswordResponse } from '../services/auth.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    standalone: false
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
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
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
    // Si ya está autenticado, redirigir
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/polizas']);
    }
    
    // Asegurar que los formularios estén limpios al cargar
    this.resetForms();
  }
  
  resetForms(): void {
    this.loginForm.reset();
    this.resetPasswordForm.reset();
    this.showResetForm = false;
    
    // Limpiar estados de validación
    [this.loginForm, this.resetPasswordForm].forEach(form => {
      Object.keys(form.controls).forEach(key => {
        const control = form.get(key);
        if (control) {
          control.markAsUntouched();
          control.markAsPristine();
          control.setErrors(null);
          control.updateValueAndValidity();
        }
      });
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      this.authService.login(email, password).subscribe({
        next: (response: LoginResponse) => {
          console.log('✅ Login successful, response:', response);
          console.log('✅ About to navigate to /polizas');
          this.showMessage('Inicio de sesión exitoso');
          this.router.navigate(['/polizas']).then(success => {
            console.log('✅ Navigation to /polizas result:', success);
          });
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Error de login:', error);
          let errorMessage = 'Credenciales inválidas';
          
          if (error.status === 401) {
            errorMessage = error.error?.message || 'Credenciales inválidas. Verifique su email y contraseña.';
          } else if (error.status === 400) {
            errorMessage = 'Datos de login incorrectos. Verifique los campos.';
          } else if (error.status === 500) {
            errorMessage = 'Error del servidor. Intente nuevamente más tarde.';
          } else if (!error.status) {
            errorMessage = 'Error de conexión. Verifique su conexión a internet.';
          }
          
          this.showMessage(errorMessage);
          this.isLoading = false;
        }
      });
    }
  }

  onResetPassword(): void {
    if (this.resetPasswordForm.valid) {
      this.isResettingPassword = true;
      const { email } = this.resetPasswordForm.value;

      this.authService.forgotPassword(email).subscribe({
        next: (response: any) => {
          this.showMessage('Si el email existe, recibirás instrucciones para restablecer tu contraseña');
          this.showResetForm = false;
          this.resetPasswordForm.reset();
          this.isResettingPassword = false;
        },
        error: (error: any) => {
          console.error('Error al solicitar reset de contraseña:', error);
          let errorMessage = 'Error al procesar la solicitud';
          
          if (error.status === 429) {
            errorMessage = 'Demasiados intentos. Intenta nuevamente más tarde.';
          } else if (error.status === 500) {
            errorMessage = 'Error del servidor. Intente nuevamente más tarde.';
          } else if (!error.status) {
            errorMessage = 'Error de conexión. Verifique su conexión a internet.';
          } else {
            // Por seguridad, siempre mostramos el mismo mensaje
            errorMessage = 'Si el email existe, recibirás instrucciones para restablecer tu contraseña';
          }
          
          this.showMessage(errorMessage);
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

  private showMessage(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
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