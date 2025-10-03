import { Component, OnInit, OnDestroy, AfterViewInit, HostListener, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatSidenav } from '@angular/material/sidenav';
import { AuthService, User } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('sidenav', { static: false }) sidenav!: MatSidenav;
  @ViewChild('sidenavContent', { static: false }) sidenavContent!: ElementRef;

  title = 'SINSEG - Sistema de Seguros';
  currentUser$: Observable<User | null>;
  isCollapsed = false;
  isMobile = false;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router,
    private renderer: Renderer2
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.checkScreenSize();
  }

  ngOnInit(): void {
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
}
