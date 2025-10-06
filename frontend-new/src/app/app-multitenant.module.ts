// app.module.ts - Configuración principal para Multi-Tenancy

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

// Routing
import { AppRoutingModule } from './app-routing.module';

// Components
import { AppComponent } from './app.component';
import { TenantSelectorComponent } from './components/tenant-selector/tenant-selector.component';

// Services
import { TenantService } from './services/tenant.service';

// Guards
import { TenantGuard } from './guards/tenant.guard';

// Interceptors
import { TenantInterceptor } from './interceptors/tenant.interceptor';

// Material UI (opcional)
// import { MatToolbarModule } from '@angular/material/toolbar';
// import { MatButtonModule } from '@angular/material/button';
// import { MatFormFieldModule } from '@angular/material/form-field';
// import { MatInputModule } from '@angular/material/input';
// import { MatCardModule } from '@angular/material/card';
// import { MatSnackBarModule } from '@angular/material/snack-bar';
// import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@NgModule({
  declarations: [
    AppComponent,
    TenantSelectorComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    BrowserAnimationsModule
    
    // Material UI modules (descomenta si usas Angular Material)
    // MatToolbarModule,
    // MatButtonModule,
    // MatFormFieldModule,
    // MatInputModule,
    // MatCardModule,
    // MatSnackBarModule,
    // MatProgressSpinnerModule
  ],
  providers: [
    // Servicios
    TenantService,
    
    // Guards
    TenantGuard,
    
    // Interceptors
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TenantInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

// app-routing.module.ts - Configuración de rutas con Multi-Tenancy

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TenantGuard } from './guards/tenant.guard';
import { TenantSelectorComponent } from './components/tenant-selector/tenant-selector.component';

const routes: Routes = [
  // Ruta pública para seleccionar tenant
  {
    path: 'select-tenant',
    component: TenantSelectorComponent,
    data: { requiresTenant: false }
  },
  
  // Ruta por defecto redirige a selector de tenant
  {
    path: '',
    redirectTo: '/select-tenant',
    pathMatch: 'full'
  },
  
  // Rutas protegidas que requieren tenant
  {
    path: 'dashboard',
    canActivate: [TenantGuard],
    loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule),
    data: { requiresTenant: true }
  },
  
  {
    path: 'polizas',
    canActivate: [TenantGuard],
    loadChildren: () => import('./modules/polizas/polizas.module').then(m => m.PolizasModule),
    data: { requiresTenant: true }
  },
  
  {
    path: 'cobros',
    canActivate: [TenantGuard],
    loadChildren: () => import('./modules/cobros/cobros.module').then(m => m.CobrosModule),
    data: { requiresTenant: true }
  },
  
  {
    path: 'reclamos',
    canActivate: [TenantGuard],
    loadChildren: () => import('./modules/reclamos/reclamos.module').then(m => m.ReclamosModule),
    data: { requiresTenant: true }
  },
  
  {
    path: 'admin',
    canActivate: [TenantGuard],
    loadChildren: () => import('./modules/admin/admin.module').then(m => m.AdminModule),
    data: { requiresTenant: true, requiredRole: 'Admin' }
  },
  
  // Rutas de autenticación (pueden usar tenant si está disponible)
  {
    path: 'login',
    loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule),
    data: { requiresTenant: false }
  },
  
  // Rutas de error
  {
    path: 'tenant-error',
    loadChildren: () => import('./modules/error/error.module').then(m => m.ErrorModule),
    data: { requiresTenant: false }
  },
  
  {
    path: 'loading',
    loadChildren: () => import('./modules/loading/loading.module').then(m => m.LoadingModule),
    data: { requiresTenant: false }
  },
  
  // Wildcard route - debe ir al final
  {
    path: '**',
    redirectTo: '/select-tenant'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    enableTracing: false, // Solo para debugging
    preloadingStrategy: PreloadAllModules
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }

// app.component.ts - Componente principal con soporte Multi-Tenant

import { Component, OnInit, OnDestroy } from '@angular/core';
import { TenantService, TenantInfo, TenantBranding } from './services/tenant.service';
import { Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  template: `
    <div class="app-container" [class.has-tenant]="currentTenant">
      <!-- Tenant Header (solo si hay tenant) -->
      <header class="tenant-header" *ngIf="currentTenant && !isPublicRoute">
        <div class="header-content">
          <div class="tenant-info">
            <img [src]="branding?.logoUrl || '/assets/default-logo.png'" 
                 [alt]="branding?.companyName" 
                 class="tenant-logo">
            <div class="tenant-details">
              <h1 class="company-name">{{ branding?.companyName || 'SIINADSEG' }}</h1>
              <span class="tenant-id">{{ currentTenant.tenantId }}</span>
            </div>
          </div>
          
          <div class="header-actions">
            <button class="btn-switch-tenant" (click)="switchTenant()" title="Cambiar empresa">
              <i class="icon-switch"></i>
              Cambiar Empresa
            </button>
            
            <div class="user-menu">
              <!-- Aquí irá el menú de usuario cuando esté implementado -->
              <span class="user-info">{{ currentTenant.subscriptionPlan }}</span>
            </div>
          </div>
        </div>
      </header>
      
      <!-- Navigation (solo si hay tenant) -->
      <nav class="main-nav" *ngIf="currentTenant && !isPublicRoute">
        <div class="nav-content">
          <a routerLink="/dashboard" routerLinkActive="active" class="nav-link">
            <i class="icon-dashboard"></i>
            Dashboard
          </a>
          <a routerLink="/polizas" routerLinkActive="active" class="nav-link">
            <i class="icon-policy"></i>
            Pólizas
          </a>
          <a routerLink="/cobros" routerLinkActive="active" class="nav-link">
            <i class="icon-payment"></i>
            Cobros
          </a>
          <a routerLink="/reclamos" routerLinkActive="active" class="nav-link">
            <i class="icon-claim"></i>
            Reclamos
          </a>
          <a routerLink="/admin" routerLinkActive="active" class="nav-link admin-link" *ngIf="isAdmin">
            <i class="icon-settings"></i>
            Administración
          </a>
        </div>
      </nav>
      
      <!-- Main Content -->
      <main class="main-content" [class.with-header]="currentTenant && !isPublicRoute">
        <!-- Loading Spinner -->
        <div class="loading-overlay" *ngIf="isLoading">
          <div class="spinner"></div>
          <p>Cargando información de la empresa...</p>
        </div>
        
        <!-- Router Outlet -->
        <router-outlet></router-outlet>
      </main>
      
      <!-- Tenant Status Indicator -->
      <div class="tenant-status" *ngIf="currentTenant">
        <span class="status-indicator" [class.active]="currentTenant.isActive"></span>
        <span class="status-text">
          {{ currentTenant.isActive ? 'Activa' : 'Inactiva' }}
        </span>
      </div>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background: #f5f5f5;
    }
    
    .tenant-header {
      background: white;
      border-bottom: 1px solid #e0e0e0;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      position: sticky;
      top: 0;
      z-index: 100;
    }
    
    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .tenant-info {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    
    .tenant-logo {
      width: 50px;
      height: 50px;
      border-radius: 8px;
      object-fit: cover;
    }
    
    .tenant-details h1 {
      margin: 0;
      font-size: 1.5rem;
      color: var(--primary-color, #1976d2);
    }
    
    .tenant-id {
      font-size: 0.875rem;
      color: #666;
      background: #f0f0f0;
      padding: 2px 8px;
      border-radius: 4px;
    }
    
    .header-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    
    .btn-switch-tenant {
      background: transparent;
      border: 2px solid var(--primary-color, #1976d2);
      color: var(--primary-color, #1976d2);
      padding: 0.5rem 1rem;
      border-radius: 6px;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      transition: all 0.2s ease;
    }
    
    .btn-switch-tenant:hover {
      background: var(--primary-color, #1976d2);
      color: white;
    }
    
    .main-nav {
      background: white;
      border-bottom: 1px solid #e0e0e0;
    }
    
    .nav-content {
      display: flex;
      padding: 0 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .nav-link {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem 1.5rem;
      text-decoration: none;
      color: #666;
      border-bottom: 3px solid transparent;
      transition: all 0.2s ease;
    }
    
    .nav-link:hover,
    .nav-link.active {
      color: var(--primary-color, #1976d2);
      border-bottom-color: var(--primary-color, #1976d2);
    }
    
    .admin-link {
      margin-left: auto;
      color: #ff9800;
    }
    
    .admin-link:hover,
    .admin-link.active {
      color: #f57c00;
      border-bottom-color: #f57c00;
    }
    
    .main-content {
      min-height: calc(100vh - 120px);
      position: relative;
    }
    
    .main-content.with-header {
      min-height: calc(100vh - 180px);
    }
    
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.9);
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }
    
    .spinner {
      width: 50px;
      height: 50px;
      border: 3px solid #f3f3f3;
      border-top: 3px solid var(--primary-color, #1976d2);
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin-bottom: 1rem;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    .tenant-status {
      position: fixed;
      bottom: 1rem;
      right: 1rem;
      background: white;
      padding: 0.5rem 1rem;
      border-radius: 20px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      z-index: 50;
    }
    
    .status-indicator {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #ccc;
    }
    
    .status-indicator.active {
      background: #4caf50;
    }
    
    /* Icons usando Unicode */
    .icon-switch::before { content: '⇄'; }
    .icon-dashboard::before { content: '📊'; }
    .icon-policy::before { content: '📋'; }
    .icon-payment::before { content: '💰'; }
    .icon-claim::before { content: '📞'; }
    .icon-settings::before { content: '⚙'; }
    
    /* Responsive */
    @media (max-width: 768px) {
      .header-content,
      .nav-content {
        padding: 1rem;
      }
      
      .nav-content {
        flex-wrap: wrap;
      }
      
      .tenant-details h1 {
        font-size: 1.25rem;
      }
      
      .header-actions {
        flex-direction: column;
        gap: 0.5rem;
      }
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  currentTenant: TenantInfo | null = null;
  branding: TenantBranding | null = null;
  isLoading = false;
  isPublicRoute = false;
  isAdmin = false;
  
  private destroy$ = new Subject<void>();

  constructor(
    private tenantService: TenantService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Suscribirse a cambios de tenant
    this.tenantService.getTenantInfo()
      .pipe(takeUntil(this.destroy$))
      .subscribe(tenant => {
        this.currentTenant = tenant;
        this.checkAdminRole();
      });

    // Suscribirse a cambios de branding
    this.tenantService.getBranding()
      .pipe(takeUntil(this.destroy$))
      .subscribe(branding => {
        this.branding = branding;
      });

    // Suscribirse a estado de carga
    this.tenantService.getLoadingState()
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.isLoading = loading;
      });

    // Detectar rutas públicas
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event: NavigationEnd) => {
        this.isPublicRoute = this.checkIfPublicRoute(event.url);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  switchTenant(): void {
    this.router.navigate(['/select-tenant'], {
      queryParams: { redirect: this.router.url }
    });
  }

  private checkIfPublicRoute(url: string): boolean {
    const publicRoutes = [
      '/select-tenant',
      '/login',
      '/register',
      '/tenant-error',
      '/loading',
      '/error'
    ];
    
    return publicRoutes.some(route => url.startsWith(route));
  }

  private checkAdminRole(): void {
    // Aquí verificarías el rol del usuario actual
    // Por ahora, asumimos que es admin si hay tenant
    this.isAdmin = !!this.currentTenant;
  }
}