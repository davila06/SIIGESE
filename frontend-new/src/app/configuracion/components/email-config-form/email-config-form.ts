import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EmailConfigService } from '../../services/email-config.service';
import { EmailConfig, EmailConfigCreate, EmailConfigTestRequest } from '../../models/email-config.model';

@Component({
  selector: 'app-email-config-form',
  standalone: false,
  templateUrl: './email-config-form.html',
  styleUrl: './email-config-form.scss'
})
export class EmailConfigForm implements OnInit {
  emailConfigForm: FormGroup;
  isEditMode = false;
  configId: number | null = null;
  loading = false;
  testing = false;
  hidePassword = true;

  // Configuraciones predefinidas comunes
  commonConfigs = [
    {
      name: 'Gmail',
      smtpServer: 'smtp.gmail.com',
      smtpPort: 587,
      useSSL: false,
      useTLS: true
    },
    {
      name: 'Outlook/Hotmail',
      smtpServer: 'smtp-mail.outlook.com',
      smtpPort: 587,
      useSSL: false,
      useTLS: true
    },
    {
      name: 'Yahoo',
      smtpServer: 'smtp.mail.yahoo.com',
      smtpPort: 587,
      useSSL: false,
      useTLS: true
    },
    {
      name: 'Custom SMTP',
      smtpServer: '',
      smtpPort: 587,
      useSSL: false,
      useTLS: true
    }
  ];

  constructor(
    private fb: FormBuilder,
    private emailConfigService: EmailConfigService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {
    this.emailConfigForm = this.createForm();
  }

  ngOnInit(): void {
    this.configId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEditMode = !!this.configId;

    if (this.isEditMode) {
      this.loadEmailConfig();
    } else {
      // Asegurar que el formulario esté limpio para nuevas configuraciones
      this.resetForm();
    }
  }
  
  resetForm(): void {
    this.emailConfigForm.reset();
    
    // Restablecer valores por defecto
    this.emailConfigForm.patchValue({
      smtpPort: 587,
      useSSL: false,
      useTLS: true,
      fromName: 'SIIGESE Sistema',
      companyName: 'SIIGESE',
      timeoutSeconds: 30,
      maxRetries: 3,
      isActive: true,
      isDefault: false
    });
    
    // Limpiar completamente los estados de validación
    Object.keys(this.emailConfigForm.controls).forEach(key => {
      const control = this.emailConfigForm.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      // Información básica
      configName: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      
      // Configuración SMTP
      smtpServer: ['', [Validators.required]],
      smtpPort: [587, [Validators.required, Validators.min(1), Validators.max(65535)]],
      useSSL: [false],
      useTLS: [true],
      
      // Autenticación
      username: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      
      // Remitente
      fromEmail: ['', [Validators.required, Validators.email]],
      fromName: ['SIIGESE Sistema', [Validators.required]],
      
      // Configuración de empresa
      companyName: ['SIIGESE'],
      companyAddress: [''],
      companyPhone: [''],
      companyWebsite: [''],
      companyLogo: [''],
      
      // Configuraciones avanzadas
      timeoutSeconds: [30, [Validators.required, Validators.min(5), Validators.max(300)]],
      maxRetries: [3, [Validators.required, Validators.min(1), Validators.max(10)]],
      
      // Estado
      isActive: [true],
      isDefault: [false]
    });
  }

  applyPresetConfig(config: any): void {
    this.emailConfigForm.patchValue({
      smtpServer: config.smtpServer,
      smtpPort: config.smtpPort,
      useSSL: config.useSSL,
      useTLS: config.useTLS
    });
    
    // Show a message
    this.showMessage(`Configuración ${config.name} aplicada`, 'info');
  }

  private loadEmailConfig(): void {
    if (!this.configId) return;

    this.loading = true;
    this.emailConfigService.getById(this.configId).subscribe({
      next: (response: any) => {
        const config = response.data || response;
        this.emailConfigForm.patchValue({
          configName: config.configName,
          description: config.description,
          smtpServer: config.smtpServer,
          smtpPort: config.smtpPort,
          useSSL: config.useSSL,
          useTLS: config.useTLS,
          username: config.username,
          fromEmail: config.fromEmail,
          fromName: config.fromName,
          companyName: config.companyName,
          companyAddress: config.companyAddress,
          companyPhone: config.companyPhone,
          companyWebsite: config.companyWebsite,
          companyLogo: config.companyLogo,
          timeoutSeconds: config.timeoutSeconds,
          maxRetries: config.maxRetries,
          isActive: config.isActive,
          isDefault: config.isDefault
        });
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading email config:', error);
        this.showMessage('Error al cargar la configuración', 'error');
        this.loading = false;
      }
    });
  }

  onProviderChange(provider: any): void {
    if (provider.name !== 'Custom SMTP') {
      this.emailConfigForm.patchValue({
        smtpServer: provider.smtpServer,
        smtpPort: provider.smtpPort,
        useSSL: provider.useSSL,
        useTLS: provider.useTLS
      });
    }
  }

  onTestConnection(): void {
    if (this.emailConfigForm.invalid) {
      this.emailConfigForm.markAllAsTouched();
      this.showMessage('Por favor, complete todos los campos requeridos', 'error');
      return;
    }

    this.testing = true;
    const formData = this.emailConfigForm.value;
    
    const testRequest: EmailConfigTestRequest = {
      smtpServer: formData.smtpServer,
      smtpPort: formData.smtpPort,
      username: formData.username,
      password: formData.password,
      useSSL: formData.useSSL,
      useTLS: formData.useTLS,
      fromEmail: formData.fromEmail,
      fromName: formData.fromName,
      testEmail: formData.fromEmail
    };

    this.emailConfigService.testConfigurationDirect(testRequest).subscribe({
      next: (result: any) => {
        const response = result.data || result;
        if (response.success) {
          this.showMessage('✅ Conexión SMTP exitosa', 'success');
        } else {
          this.showMessage(`❌ Error en la conexión: ${response.errorMessage}`, 'error');
        }
        this.testing = false;
      },
      error: (error: any) => {
        console.error('Error testing email config:', error);
        this.showMessage('Error al probar la conexión', 'error');
        this.testing = false;
      }
    });
  }

  onSubmit(): void {
    if (this.emailConfigForm.invalid) {
      this.emailConfigForm.markAllAsTouched();
      this.showMessage('Por favor, complete todos los campos requeridos', 'error');
      return;
    }

    this.loading = true;
    const formData = this.emailConfigForm.value;

    const request = this.isEditMode 
      ? this.emailConfigService.update(this.configId!, formData)
      : this.emailConfigService.create(formData);

    request.subscribe({
      next: (response) => {
        const message = this.isEditMode 
          ? 'Configuración actualizada exitosamente'
          : 'Configuración creada exitosamente';
        this.showMessage(message, 'success');
        this.router.navigate(['/configuracion/email']);
      },
      error: (error) => {
        console.error('Error saving email config:', error);
        this.showMessage('Error al guardar la configuración', 'error');
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/configuracion/email']);
  }

  private showMessage(message: string, type: 'success' | 'error' | 'info' = 'info'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: type === 'error' ? ['error-snackbar'] : 
                  type === 'success' ? ['success-snackbar'] : ['info-snackbar']
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.emailConfigForm.get(fieldName);
    if (control?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} es requerido`;
    }
    if (control?.hasError('email')) {
      return 'Ingrese un email válido';
    }
    if (control?.hasError('minlength')) {
      return `Mínimo ${control.errors?.['minlength'].requiredLength} caracteres`;
    }
    if (control?.hasError('min')) {
      return `Valor mínimo: ${control.errors?.['min'].min}`;
    }
    if (control?.hasError('max')) {
      return `Valor máximo: ${control.errors?.['max'].max}`;
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      configName: 'Nombre de configuración',
      smtpServer: 'Servidor SMTP',
      smtpPort: 'Puerto SMTP',
      username: 'Usuario',
      password: 'Contraseña',
      fromEmail: 'Email remitente',
      fromName: 'Nombre remitente',
      timeoutSeconds: 'Timeout',
      maxRetries: 'Máx. reintentos'
    };
    return labels[fieldName] || fieldName;
  }
}
