import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})
export class ChangePasswordComponent implements OnInit {
  changePasswordForm!: FormGroup;
  isFirstLogin = false;
  isResetPassword = false;
  isLoading = false;
  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;
  resetToken?: string;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.isFirstLogin = params['firstLogin'] === 'true';
      this.isResetPassword = params['reset'] === 'true';
      this.resetToken = params['token'];
    });

    this.initializeForm();
  }

  private initializeForm() {
    const formConfig: any = {
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        this.passwordStrengthValidator
      ]],
      confirmPassword: ['', Validators.required]
    };

    if (!this.isResetPassword) {
      formConfig.currentPassword = ['', Validators.required];
    }

    this.changePasswordForm = this.fb.group(formConfig, {
      validators: this.passwordMatchValidator
    });
  }

  private passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasNumber = /[0-9]/.test(value);
    const hasUpper = /[A-Z]/.test(value);
    const hasLower = /[a-z]/.test(value);
    const hasSpecial = /[#?!@$%^&*-]/.test(value);

    const valid = hasNumber && hasUpper && hasLower && hasSpecial;
    
    if (!valid) {
      return {
        passwordStrength: {
          hasNumber,
          hasUpper,
          hasLower,
          hasSpecial
        }
      };
    }
    
    return null;
  }

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    
    if (newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    
    return null;
  }

  onSubmit() {
    if (this.changePasswordForm.valid) {
      this.isLoading = true;
      const formValue = this.changePasswordForm.value;

      if (this.isResetPassword) {
        this.resetPassword(formValue.newPassword);
      } else {
        this.changePassword(formValue.currentPassword, formValue.newPassword);
      }
    }
  }

  private changePassword(currentPassword: string, newPassword: string) {
    this.authService.changePassword(currentPassword, newPassword).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open('Contraseña cambiada exitosamente', 'Cerrar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        
        if (this.isFirstLogin) {
          this.router.navigate(['/polizas']);
        } else {
          this.router.navigate(['/polizas']);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.snackBar.open(
          error.error?.message || 'Error al cambiar la contraseña',
          'Cerrar',
          {
            duration: 5000,
            panelClass: ['error-snackbar']
          }
        );
      }
    });
  }

  private resetPassword(newPassword: string) {
    this.authService.resetPasswordWithToken(this.resetToken!, newPassword).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open('Contraseña restablecida exitosamente', 'Cerrar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isLoading = false;
        this.snackBar.open(
          error.error?.message || 'Error al restablecer la contraseña',
          'Cerrar',
          {
            duration: 5000,
            panelClass: ['error-snackbar']
          }
        );
      }
    });
  }

  getPasswordStrengthErrors(): string[] {
    const errors = this.changePasswordForm.get('newPassword')?.errors?.['passwordStrength'];
    if (!errors) return [];

    const messages: string[] = [];
    if (!errors.hasNumber) messages.push('Debe contener al menos un número');
    if (!errors.hasUpper) messages.push('Debe contener al menos una mayúscula');
    if (!errors.hasLower) messages.push('Debe contener al menos una minúscula');
    if (!errors.hasSpecial) messages.push('Debe contener al menos un carácter especial (#?!@$%^&*-)');

    return messages;
  }

  goBack() {
    if (this.isFirstLogin || this.isResetPassword) {
      this.router.navigate(['/login']);
    } else {
      this.router.navigate(['/polizas']);
    }
  }

  getTitle(): string {
    if (this.isFirstLogin) return 'Cambiar Contraseña - Primer Ingreso';
    if (this.isResetPassword) return 'Restablecer Contraseña';
    return 'Cambiar Contraseña';
  }

  getSubtitle(): string {
    if (this.isFirstLogin) return 'Por seguridad, debes cambiar tu contraseña antes de continuar';
    if (this.isResetPassword) return 'Ingresa tu nueva contraseña';
    return 'Actualiza tu contraseña actual';
  }
}