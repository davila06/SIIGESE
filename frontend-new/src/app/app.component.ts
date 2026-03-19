import { Component, OnInit, OnDestroy, AfterViewInit, HostListener, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { trigger, transition, style, animate, query } from '@angular/animations';
import { Router, NavigationEnd } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { MatSidenav } from '@angular/material/sidenav';
import { AuthService } from './services/auth.service';
import { User } from './interfaces/user.interface';
import { TitleService } from './services/title.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: false,
  animations: [
    // Route page-enter transition
    trigger('routeAnimations', [
      transition('* <=> *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(10px)' }),
          animate('260ms cubic-bezier(0.4, 0, 0.2, 1)',
            style({ opacity: 1, transform: 'translateY(0)' }))
        ], { optional: true })
      ])
    ]),
    trigger('aiDialogAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(28px) scale(0.94)' }),
        animate('280ms cubic-bezier(0.34, 1.32, 0.64, 1)',
          style({ opacity: 1, transform: 'translateY(0) scale(1)' }))
      ]),
      transition(':leave', [
        animate('180ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({ opacity: 0, transform: 'translateY(16px) scale(0.95)' }))
      ])
    ])
  ]
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('sidenav', { static: false }) sidenav!: MatSidenav;
  @ViewChild('sidenavContent', { static: false }) sidenavContent!: ElementRef;

  title = 'SIIGESE - Sistema Integral de Gestión de Seguros';
  currentUser$: Observable<User | null>;
  currentRoute = '';
  isCollapsed = false;
  isMobile = false;
  isChatOpen = false;
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

    // Track current route for page-enter animation trigger
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntil(this.destroy$)
    ).subscribe((e: any) => {
      this.currentRoute = (e as NavigationEnd).urlAfterRedirects;
    });

    // Verificar autenticación al iniciar la app
    this.currentUser$.pipe(takeUntil(this.destroy$)).subscribe(user => {
      if (!user && !this.router.url.includes('login')) {
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
  onResize(_event: Event): void {
    this.checkScreenSize();
  }

  private checkScreenSize(): void {
    const width = window.innerWidth;
    this.isMobile = width <= 768;
    if (this.isMobile) {
      this.isCollapsed = false; // En móvil no usar collapsed state
    } else if (width < 1100) {
      // Tablet (769–1099px): auto-collapse sidebar to 70px icon-only mode
      this.isCollapsed = true;
    }
    this.updateContentMargin();
  }

  toggleSidebar(): void {
    if (!this.isMobile) {
      this.isCollapsed = !this.isCollapsed;
      setTimeout(() => {
        this.updateContentMargin();
      }, 50);
    }
  }

  toggleChat(): void {
    this.isChatOpen = !this.isChatOpen;
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
    return this.authService.hasAnyRole(['Admin', 'DataLoader']);
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
