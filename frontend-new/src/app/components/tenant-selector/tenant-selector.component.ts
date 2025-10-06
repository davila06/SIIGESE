// tenant-selector.component.ts - Componente para seleccionar tenant

import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TenantService, CreateTenantRequest } from '../../services/tenant.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-tenant-selector',
  template: `
    <div class="tenant-selector-container">
      <div class="selector-card">
        <div class="logo-section">
          <img src="/assets/logo.png" alt="SIINADSEG" class="logo" />
          <h1>SIINADSEG</h1>
        </div>
        
        <h2>Seleccionar Empresa</h2>
        
        <!-- Detección automática -->
        <div class="auto-detect" *ngIf="detectedTenantId">
          <div class="alert alert-info">
            <i class="icon-info"></i>
            <p>Se detectó la empresa: <strong>{{detectedTenantId}}</strong></p>
          </div>
          <button (click)="loadDetectedTenant()" class="btn btn-primary btn-block">
            <i class="icon-login" *ngIf="!loading"></i>
            <i class="icon-spinner" *ngIf="loading"></i>
            <span *ngIf="loading">Verificando...</span>
            <span *ngIf="!loading">Acceder a {{detectedTenantId}}</span>
          </button>
          <hr class="separator">
        </div>

        <!-- Acceso manual -->
        <div class="manual-access">
          <h3>Acceso Manual</h3>
          <form [formGroup]="accessForm" (ngSubmit)="accessTenant()">
            <div class="form-group">
              <label for="tenantId">ID de Empresa:</label>
              <input 
                type="text" 
                id="tenantId"
                formControlName="tenantId"
                placeholder="Ej: mi-empresa"
                class="form-control"
                autocomplete="off">
              <div class="form-help">Solo letras minúsculas, números y guiones</div>
              <div class="error" *ngIf="accessForm.get('tenantId')?.invalid && accessForm.get('tenantId')?.touched">
                ID de empresa requerido (formato: letras-minusculas-123)
              </div>
            </div>
            <button type="submit" [disabled]="accessForm.invalid || loading" class="btn btn-primary btn-block">
              <i class="icon-search" *ngIf="!loading"></i>
              <i class="icon-spinner" *ngIf="loading"></i>
              <span *ngIf="loading">Verificando...</span>
              <span *ngIf="!loading">Acceder</span>
            </button>
          </form>
        </div>

        <hr class="separator">

        <!-- Crear nueva empresa -->
        <div class="create-tenant">
          <h3>¿No tienes empresa registrada?</h3>
          <p class="create-description">Registra tu empresa y comienza a gestionar pólizas, cobros y reclamos.</p>
          
          <button 
            (click)="showCreateForm = !showCreateForm" 
            class="btn btn-outline btn-block"
            [class.active]="showCreateForm">
            <i class="icon-plus" *ngIf="!showCreateForm"></i>
            <i class="icon-minus" *ngIf="showCreateForm"></i>
            <span *ngIf="!showCreateForm">Crear Nueva Empresa</span>
            <span *ngIf="showCreateForm">Cancelar Registro</span>
          </button>
          
          <form *ngIf="showCreateForm" [formGroup]="createForm" (ngSubmit)="createTenant()" class="create-form">
            <div class="form-row">
              <div class="form-group">
                <label for="newTenantId">ID de Empresa <span class="required">*</span></label>
                <input 
                  type="text" 
                  id="newTenantId"
                  formControlName="tenantId"
                  placeholder="mi-empresa-123"
                  class="form-control"
                  autocomplete="off">
                <div class="form-help">Identificador único (solo letras minúsculas, números y guiones)</div>
              </div>
              <div class="form-group">
                <label for="companyName">Nombre de la Empresa <span class="required">*</span></label>
                <input 
                  type="text" 
                  id="companyName"
                  formControlName="companyName"
                  placeholder="Mi Empresa S.A."
                  class="form-control"
                  autocomplete="organization">
              </div>
            </div>
            
            <div class="form-row">
              <div class="form-group">
                <label for="adminEmail">Email del Administrador <span class="required">*</span></label>
                <input 
                  type="email" 
                  id="adminEmail"
                  formControlName="adminEmail"
                  placeholder="admin@miempresa.com"
                  class="form-control"
                  autocomplete="email">
              </div>
              <div class="form-group">
                <label for="adminFirstName">Nombre <span class="required">*</span></label>
                <input 
                  type="text" 
                  id="adminFirstName"
                  formControlName="adminFirstName"
                  placeholder="Juan"
                  class="form-control"
                  autocomplete="given-name">
              </div>
            </div>
            
            <div class="form-group">
              <label for="adminLastName">Apellido <span class="required">*</span></label>
              <input 
                type="text" 
                id="adminLastName"
                formControlName="adminLastName"
                placeholder="Pérez"
                class="form-control"
                autocomplete="family-name">
            </div>
            
            <div class="terms-section">
              <label class="checkbox-label">
                <input type="checkbox" formControlName="acceptTerms" required>
                <span class="checkmark"></span>
                Acepto los <a href="/terms" target="_blank">Términos y Condiciones</a> y la <a href="/privacy" target="_blank">Política de Privacidad</a>
              </label>
            </div>
            
            <button type="submit" [disabled]="createForm.invalid || creating" class="btn btn-success btn-block">
              <i class="icon-check" *ngIf="!creating"></i>
              <i class="icon-spinner" *ngIf="creating"></i>
              <span *ngIf="creating">Creando empresa...</span>
              <span *ngIf="!creating">Crear Empresa</span>
            </button>
          </form>
        </div>

        <!-- Footer info -->
        <div class="footer-info">
          <p>¿Necesitas ayuda? <a href="mailto:soporte@siinadseg.com">Contacta soporte</a></p>
          <p class="version">SIINADSEG v2.0 - Sistema Multi-Empresa</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .tenant-selector-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 1rem;
    }
    
    .selector-card {
      background: white;
      padding: 2.5rem;
      border-radius: 15px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.1);
      max-width: 600px;
      width: 100%;
      max-height: 90vh;
      overflow-y: auto;
    }
    
    .logo-section {
      text-align: center;
      margin-bottom: 2rem;
    }
    
    .logo {
      width: 80px;
      height: 80px;
      margin-bottom: 1rem;
    }
    
    .logo-section h1 {
      color: #1976d2;
      margin: 0;
      font-size: 2rem;
      font-weight: 700;
    }
    
    h2, h3 {
      color: #333;
      margin-bottom: 1rem;
    }
    
    .form-group {
      margin-bottom: 1.5rem;
    }
    
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }
    
    @media (max-width: 768px) {
      .form-row {
        grid-template-columns: 1fr;
      }
    }
    
    .create-form {
      margin-top: 1.5rem;
      padding-top: 1.5rem;
      border-top: 1px solid #eee;
      animation: slideDown 0.3s ease-out;
    }
    
    @keyframes slideDown {
      from {
        opacity: 0;
        transform: translateY(-10px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    label {
      font-weight: 600;
      color: #555;
      display: block;
      margin-bottom: 0.5rem;
    }
    
    .required {
      color: #f44336;
    }
    
    .form-help {
      font-size: 0.875rem;
      color: #666;
      margin-top: 0.25rem;
    }
    
    .btn {
      padding: 0.875rem 1.5rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 600;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      transition: all 0.2s ease;
      font-size: 1rem;
    }
    
    .btn-block {
      width: 100%;
    }
    
    .btn-primary {
      background: linear-gradient(135deg, #1976d2, #1565c0);
      color: white;
      box-shadow: 0 4px 15px rgba(25, 118, 210, 0.3);
    }
    
    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(25, 118, 210, 0.4);
    }
    
    .btn-outline {
      background: transparent;
      color: #1976d2;
      border: 2px solid #1976d2;
    }
    
    .btn-outline:hover:not(:disabled) {
      background: #1976d2;
      color: white;
    }
    
    .btn-outline.active {
      background: #1976d2;
      color: white;
    }
    
    .btn-success {
      background: linear-gradient(135deg, #4caf50, #45a049);
      color: white;
      box-shadow: 0 4px 15px rgba(76, 175, 80, 0.3);
    }
    
    .btn-success:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(76, 175, 80, 0.4);
    }
    
    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none !important;
    }
    
    .form-control {
      width: 100%;
      padding: 0.875rem;
      border: 2px solid #e0e0e0;
      border-radius: 8px;
      font-size: 1rem;
      transition: border-color 0.2s ease;
    }
    
    .form-control:focus {
      outline: none;
      border-color: #1976d2;
      box-shadow: 0 0 0 3px rgba(25, 118, 210, 0.1);
    }
    
    .error {
      color: #f44336;
      font-size: 0.875rem;
      margin-top: 0.5rem;
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }
    
    .alert {
      padding: 1rem;
      border-radius: 8px;
      margin-bottom: 1rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    
    .alert-info {
      background: #e3f2fd;
      color: #1976d2;
      border: 1px solid #bbdefb;
    }
    
    .separator {
      margin: 2rem 0;
      border: none;
      height: 1px;
      background: linear-gradient(to right, transparent, #ddd, transparent);
    }
    
    .create-description {
      color: #666;
      margin-bottom: 1.5rem;
      line-height: 1.5;
    }
    
    .terms-section {
      margin: 1.5rem 0;
    }
    
    .checkbox-label {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      cursor: pointer;
      font-weight: normal;
      line-height: 1.4;
    }
    
    .checkbox-label input[type="checkbox"] {
      display: none;
    }
    
    .checkmark {
      width: 20px;
      height: 20px;
      border: 2px solid #ddd;
      border-radius: 4px;
      position: relative;
      flex-shrink: 0;
      margin-top: 2px;
    }
    
    .checkbox-label input[type="checkbox"]:checked + .checkmark {
      background: #1976d2;
      border-color: #1976d2;
    }
    
    .checkbox-label input[type="checkbox"]:checked + .checkmark::after {
      content: '✓';
      position: absolute;
      color: white;
      font-size: 14px;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
    }
    
    .footer-info {
      text-align: center;
      margin-top: 2rem;
      padding-top: 1.5rem;
      border-top: 1px solid #eee;
      color: #666;
    }
    
    .footer-info a {
      color: #1976d2;
      text-decoration: none;
    }
    
    .footer-info a:hover {
      text-decoration: underline;
    }
    
    .version {
      font-size: 0.875rem;
      color: #999;
      margin-top: 0.5rem;
    }
    
    /* Icons usando Unicode */
    .icon-info::before { content: 'ℹ'; }
    .icon-login::before { content: '→'; }
    .icon-search::before { content: '🔍'; }
    .icon-plus::before { content: '+'; }
    .icon-minus::before { content: '−'; }
    .icon-check::before { content: '✓'; }
    .icon-spinner::before { 
      content: '⟳'; 
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `]
})
export class TenantSelectorComponent implements OnInit {
  detectedTenantId: string | null = null;
  showCreateForm = false;
  loading = false;
  creating = false;
  
  accessForm: FormGroup;
  createForm: FormGroup;

  constructor(
    private tenantService: TenantService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.accessForm = this.fb.group({
      tenantId: ['', [
        Validators.required, 
        Validators.pattern(/^[a-z0-9-]+$/),
        Validators.minLength(3),
        Validators.maxLength(50)
      ]]
    });

    this.createForm = this.fb.group({
      tenantId: ['', [
        Validators.required, 
        Validators.pattern(/^[a-z0-9-]+$/),
        Validators.minLength(3),
        Validators.maxLength(50)
      ]],
      companyName: ['', [Validators.required, Validators.minLength(2)]],
      adminEmail: ['', [Validators.required, Validators.email]],
      adminFirstName: ['', [Validators.required, Validators.minLength(2)]],
      adminLastName: ['', [Validators.required, Validators.minLength(2)]],
      acceptTerms: [false, Validators.requiredTrue]
    });
  }

  ngOnInit(): void {
    // Detectar tenant desde query parameters
    this.route.queryParams.subscribe(params => {
      if (params['tenant']) {
        this.detectedTenantId = params['tenant'];
        this.accessForm.patchValue({ tenantId: params['tenant'] });
      }
    });

    // Auto-focus en el primer campo
    setTimeout(() => {
      const firstInput = document.querySelector('input') as HTMLInputElement;
      if (firstInput) {
        firstInput.focus();
      }
    }, 100);
  }

  async loadDetectedTenant(): Promise<void> {
    if (!this.detectedTenantId) return;
    
    this.loading = true;
    try {
      const tenant = await this.tenantService.loadTenantById(this.detectedTenantId);
      if (tenant) {
        this.redirectAfterSuccess();
      } else {
        this.showError('Empresa no encontrada o inactiva');
      }
    } catch (error) {
      console.error('Error loading tenant:', error);
      this.showError('Error cargando la empresa');
    } finally {
      this.loading = false;
    }
  }

  async accessTenant(): Promise<void> {
    if (this.accessForm.invalid) return;
    
    const tenantId = this.accessForm.value.tenantId;
    this.loading = true;
    
    try {
      const tenant = await this.tenantService.loadTenantById(tenantId);
      if (tenant) {
        this.redirectAfterSuccess();
      } else {
        this.showError('Empresa no encontrada o inactiva');
      }
    } catch (error) {
      console.error('Error accessing tenant:', error);
      this.showError('Error accediendo a la empresa');
    } finally {
      this.loading = false;
    }
  }

  async createTenant(): Promise<void> {
    if (this.createForm.invalid) return;
    
    this.creating = true;
    try {
      // Verificar disponibilidad primero
      const availability = await this.tenantService.checkTenantAvailability(this.createForm.value.tenantId);
      if (!availability.available) {
        this.showError('El ID de empresa ya está en uso. Prueba con otro.');
        return;
      }

      const request: CreateTenantRequest = {
        tenantId: this.createForm.value.tenantId,
        companyName: this.createForm.value.companyName,
        adminEmail: this.createForm.value.adminEmail,
        adminFirstName: this.createForm.value.adminFirstName,
        adminLastName: this.createForm.value.adminLastName,
        subscriptionPlan: 'Basic'
      };
      
      const tenant = await this.tenantService.createTenant(request);
      
      if (tenant) {
        this.showSuccess(`¡Empresa "${tenant.companyName}" creada exitosamente!`);
        
        // Esperar un momento antes de redirigir
        setTimeout(() => {
          this.tenantService.switchTenant(tenant.tenantId);
        }, 2000);
      } else {
        this.showError('Error creando la empresa. El ID ya existe.');
      }
    } catch (error: any) {
      console.error('Error creating tenant:', error);
      const errorMessage = error.error?.message || 'Error creando la empresa';
      this.showError(errorMessage);
    } finally {
      this.creating = false;
    }
  }

  private redirectAfterSuccess(): void {
    const redirect = this.route.snapshot.queryParams['redirect'] || '/dashboard';
    this.router.navigateByUrl(redirect);
  }

  private showError(message: string): void {
    // Aquí podrías usar un servicio de notificaciones más sofisticado
    alert(`Error: ${message}`);
  }

  private showSuccess(message: string): void {
    // Aquí podrías usar un servicio de notificaciones más sofisticado
    alert(message);
  }

  // Validador personalizado para verificar disponibilidad de tenant ID
  async checkTenantIdAvailability(): Promise<void> {
    const tenantIdControl = this.createForm.get('tenantId');
    if (!tenantIdControl || tenantIdControl.invalid) return;

    try {
      const availability = await this.tenantService.checkTenantAvailability(tenantIdControl.value);
      if (!availability.available) {
        tenantIdControl.setErrors({ notAvailable: true });
      }
    } catch (error) {
      console.error('Error checking availability:', error);
    }
  }
}