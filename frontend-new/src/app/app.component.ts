import { Component, OnInit, OnDestroy, AfterViewInit, HostListener, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatSidenav } from '@angular/material/sidenav';
import { AuthService, User } from './services/auth.service';
import { TitleService } from './services/title.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: false
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('sidenav', { static: false }) sidenav!: MatSidenav;
  @ViewChild('sidenavContent', { static: false }) sidenavContent!: ElementRef;

  title = 'SIIGESE - Sistema Integral de Gestión de Seguros';
  currentUser$: Observable<User | null>;
  isCollapsed = false;
  isMobile = false;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router,
    private renderer: Renderer2,
    private titleService: TitleService
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.checkScreenSize();
  }

  ngOnInit(): void {
    // Inicializar el servicio de título para actualizaciones automáticas
    this.titleService.updateTitle(this.router.url);
    
    // Verificar autenticación al iniciar la app
    this.currentUser$.pipe(takeUntil(this.destroy$)).subscribe(user => {
      console.log('🔄 App.component - User state changed:', user);
      console.log('🔄 App.component - Current URL:', this.router.url);
      
      if (!user && !this.router.url.includes('login')) {
        console.log('🔄 App.component - Redirecting to login');
        this.router.navigate(['/login']);
      }
    });
  }

  ngAfterViewInit(): void {
    // Asegurar que el margin se configure correctamente después de la inicialización
    setTimeout(() => {
      this.updateContentMargin();
    }, 100);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any): void {
    this.checkScreenSize();
  }

  private checkScreenSize(): void {
    this.isMobile = window.innerWidth <= 768;
    if (this.isMobile) {
      this.isCollapsed = false; // En móvil no usar collapsed state
    }
    console.log('Screen size check:', { isMobile: this.isMobile, isCollapsed: this.isCollapsed });
    this.updateContentMargin();
  }

  toggleSidebar(): void {
    if (!this.isMobile) {
      this.isCollapsed = !this.isCollapsed;
      console.log('Sidebar toggled:', this.isCollapsed ? 'collapsed' : 'expanded');
      setTimeout(() => {
        this.updateContentMargin();
      }, 50);
    }
  }

  private updateContentMargin(): void {
    // Las clases CSS se manejan automáticamente en el template
    // Este método se mantiene para compatibilidad y triggers futuros
  }

  onNavItemClick(): void {
    // Cerrar sidebar en móvil después de navegar
    if (this.isMobile && this.sidenav) {
      this.sidenav.close();
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  navigateToPolizas(): void {
    this.router.navigate(['/polizas']);
  }

  navigateToUsuarios(): void {
    this.router.navigate(['/usuarios']);
  }

  isAdmin(): boolean {
    return this.authService.hasAnyRole(['Admin']);
  }

  canUploadExcel(): boolean {
    const user = this.authService.getCurrentUser();
    const hasRole = this.authService.hasAnyRole(['Admin', 'DataLoader']);
    console.log('🔍 canUploadExcel check:', {
      user: user,
      userRoles: user?.roles,
      hasRole: hasRole,
      lookingFor: ['Admin', 'DataLoader']
    });
    return hasRole;
  }

  getCurrentModule(): string {
    const url = this.router.url;
    const segments = url.split('/').filter(segment => segment);
    
    if (segments.length === 0) return 'Dashboard';
    
    const moduleMap: { [key: string]: string } = {
      'polizas': 'Gestión de Pólizas',
      'clientes': 'Gestión de Clientes', 
      'cobros': 'Gestión de Cobros',
      'emails': 'Sistema de Emails',
      'reclamos': 'Gestión de Reclamos',
      'usuarios': 'Gestión de Usuarios',
      'reportes': 'Reportes',
      'configuracion': 'Configuración',
      'dashboard': 'Dashboard'
    };

    return moduleMap[segments[0]] || 'SIIGESE';
  }
}
