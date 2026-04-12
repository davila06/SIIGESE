import { Component, OnInit, OnDestroy, AfterViewInit, HostListener, ViewChild, ElementRef, Renderer2 } from '@angular/core';
import { trigger, transition, style, animate, query } from '@angular/animations';
import { Router, NavigationEnd } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { MatSidenav } from '@angular/material/sidenav';
import { AuthService } from './services/auth.service';
import { User } from './interfaces/user.interface';
import { TitleService } from './services/title.service';
import { ThemeService } from './services/theme.service';

interface AnalyticsMenuItem {
  label: string;
  icon: string;
  route: string;
  valueScore: number;
  tier: 'core' | 'explore';
}

interface SidebarMenuItem {
  label: string;
  icon: string;
  route: string;
  valueScore: number;
  access?: 'all' | 'admin' | 'upload';
}

interface QuickAccessItem {
  label: string;
  icon: string;
  route: string;
  valueScore: number;
  section: 'gestion' | 'analitica' | 'sistema';
}

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
  @ViewChild('chatFabButton', { static: false }) chatFabButton?: ElementRef<HTMLButtonElement>;

  title = 'IADS IAsistente Digital de Servicios';
  currentUser$: Observable<User | null>;
  currentRoute = '';
  isCollapsed = false;
  isMobile = false;
  isChatOpen = false;
  isDraggingChatFab = false;
  analyticsExpanded = false;
  chatFabRight = 24;
  chatFabBottom = 24;
  navGroups = {
    gestion: true,
    analitica: true,
    sistema: true
  };
  private readonly analyticsExpandedStorageKey = 'iads-nav-analytics-expanded';
  private readonly analyticsUsageStorageKey = 'iads-nav-analytics-usage';
  private readonly pinnedRoutesStorageKey = 'iads-nav-pinned-routes';
  private analyticsUsage: Record<string, number> = {};
  private pinnedRoutes: string[] = [];
  readonly analyticsMenuItems: AnalyticsMenuItem[] = [
    { label: 'Dashboard Ejecutivo', icon: 'dashboard', route: '/analytics', valueScore: 100, tier: 'core' },
    { label: 'Cobros y Cashflow', icon: 'payments', route: '/analytics/cobros', valueScore: 95, tier: 'core' },
    { label: 'Reclamos y SLA', icon: 'support', route: '/analytics/reclamos', valueScore: 92, tier: 'core' },
    { label: 'Portfolio Pólizas', icon: 'folder_open', route: '/analytics/portfolio', valueScore: 88, tier: 'core' },
    { label: 'Cliente 360°', icon: 'manage_accounts', route: '/analytics/cliente360', valueScore: 86, tier: 'explore' },
    { label: 'Analítica Predictiva', icon: 'auto_graph', route: '/analytics/predictivo', valueScore: 82, tier: 'explore' },
    { label: 'Agenda Inteligente', icon: 'today', route: '/analytics/agenda', valueScore: 80, tier: 'explore' },
    { label: 'Funnel Ventas', icon: 'trending_up', route: '/analytics/ventas', valueScore: 78, tier: 'explore' },
    { label: 'Operacional', icon: 'group_work', route: '/analytics/operacional', valueScore: 76, tier: 'explore' },
    { label: 'Reportes Exportables', icon: 'description', route: '/analytics/reportes', valueScore: 74, tier: 'explore' },
    { label: 'Emails Analytics', icon: 'analytics', route: '/analytics/emails', valueScore: 70, tier: 'explore' }
  ];
  readonly gestionMenuItemsBase: SidebarMenuItem[] = [
    { label: 'Pólizas', icon: 'business', route: '/polizas', valueScore: 98 },
    { label: 'Cobros', icon: 'payment', route: '/cobros', valueScore: 95 },
    { label: 'Reclamos', icon: 'report_problem', route: '/reclamos', valueScore: 93 },
    { label: 'Cotizaciones', icon: 'request_quote', route: '/cotizaciones', valueScore: 85 },
    { label: 'Notificaciones', icon: 'email', route: '/emails', valueScore: 82 }
  ];
  readonly sistemaMenuItemsBase: SidebarMenuItem[] = [
    { label: 'Configuración', icon: 'settings', route: '/configuracion', valueScore: 92, access: 'all' },
    { label: 'Usuarios', icon: 'people', route: '/usuarios', valueScore: 88, access: 'admin' },
    { label: 'Subir Pólizas Excel', icon: 'cloud_upload', route: '/polizas/upload', valueScore: 80, access: 'upload' }
  ];
  readonly quickAccessLimit = 5;
  private chatFabDragState = {
    active: false,
    moved: false,
    startX: 0,
    startY: 0,
    startRight: 24,
    startBottom: 24,
    pointerId: -1
  };
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router,
    private renderer: Renderer2,
    private titleService: TitleService,
    public themeService: ThemeService
  ) {
    this.currentUser$ = this.authService.currentUser$;
    this.checkScreenSize();
  }

  ngOnInit(): void {
    this.analyticsExpanded = this.loadAnalyticsExpandedState();
    this.analyticsUsage = this.loadAnalyticsUsage();
    this.pinnedRoutes = this.loadPinnedRoutes();

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
    this.keepFabInViewport();
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

  private keepFabInViewport(): void {
    const fabWidth = this.chatFabButton?.nativeElement?.offsetWidth ?? (window.innerWidth <= 520 ? 56 : 64);
    const fabHeight = this.chatFabButton?.nativeElement?.offsetHeight ?? (window.innerWidth <= 520 ? 56 : 64);
    const minGap = window.innerWidth <= 520 ? 12 : 16;
    const maxRight = Math.max(minGap, window.innerWidth - fabWidth - minGap);
    const maxBottom = Math.max(minGap, window.innerHeight - fabHeight - minGap);

    this.chatFabRight = Math.min(Math.max(this.chatFabRight, minGap), maxRight);
    this.chatFabBottom = Math.min(Math.max(this.chatFabBottom, minGap), maxBottom);
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

  get chatDialogTop(): number | null {
    const metrics = this.getChatDialogMetrics();
    return metrics.openBelowFab ? metrics.top : null;
  }

  get chatDialogBottom(): number | null {
    const metrics = this.getChatDialogMetrics();
    return metrics.openBelowFab ? null : metrics.bottom;
  }

  onChatFabPointerDown(event: PointerEvent): void {
    if (event.button !== 0) {
      return;
    }

    this.chatFabDragState.active = true;
    this.chatFabDragState.moved = false;
    this.chatFabDragState.startX = event.clientX;
    this.chatFabDragState.startY = event.clientY;
    this.chatFabDragState.startRight = this.chatFabRight;
    this.chatFabDragState.startBottom = this.chatFabBottom;
    this.chatFabDragState.pointerId = event.pointerId;
    this.isDraggingChatFab = false;

    const button = this.chatFabButton?.nativeElement;
    button?.setPointerCapture?.(event.pointerId);
  }

  onChatFabClick(event: MouseEvent): void {
    if (this.chatFabDragState.moved) {
      event.preventDefault();
      event.stopPropagation();
      this.chatFabDragState.moved = false;
      return;
    }

    this.toggleChat();
  }

  @HostListener('document:pointermove', ['$event'])
  onDocumentPointerMove(event: PointerEvent): void {
    if (!this.chatFabDragState.active) {
      return;
    }

    const fabWidth = this.chatFabButton?.nativeElement?.offsetWidth ?? 64;
    const fabHeight = this.chatFabButton?.nativeElement?.offsetHeight ?? 64;
    const minGap = window.innerWidth <= 520 ? 12 : 16;

    const deltaX = event.clientX - this.chatFabDragState.startX;
    const deltaY = event.clientY - this.chatFabDragState.startY;

    if (Math.abs(deltaX) > 4 || Math.abs(deltaY) > 4) {
      this.chatFabDragState.moved = true;
      this.isDraggingChatFab = true;
    }

    const maxRight = Math.max(minGap, window.innerWidth - fabWidth - minGap);
    const maxBottom = Math.max(minGap, window.innerHeight - fabHeight - minGap);
    const nextRight = this.chatFabDragState.startRight - deltaX;
    const nextBottom = this.chatFabDragState.startBottom - deltaY;

    this.chatFabRight = Math.min(Math.max(nextRight, minGap), maxRight);
    this.chatFabBottom = Math.min(Math.max(nextBottom, minGap), maxBottom);
  }

  @HostListener('document:pointerup', ['$event'])
  onDocumentPointerUp(event: PointerEvent): void {
    if (!this.chatFabDragState.active) {
      return;
    }

    const button = this.chatFabButton?.nativeElement;
    if (this.chatFabDragState.pointerId !== -1) {
      button?.releasePointerCapture?.(this.chatFabDragState.pointerId);
    }

    this.chatFabDragState.active = false;
    this.chatFabDragState.pointerId = -1;

    // Reset cursor state after click dispatch can inspect drag intent.
    if (event.type === 'pointerup') {
      this.isDraggingChatFab = false;
      if (this.chatFabDragState.moved) {
        window.setTimeout(() => {
          this.chatFabDragState.moved = false;
        }, 120);
      }
    }
  }

  closeChat(): void {
    this.isChatOpen = false;
  }

  handleDialogCancel(event: Event): void {
    event.preventDefault();
    this.closeChat();
  }

  private updateContentMargin(): void {
    // Las clases CSS se manejan automáticamente en el template
    // Este método se mantiene para compatibilidad y triggers futuros
  }

  private getChatDialogMetrics(): { openBelowFab: boolean; top: number; bottom: number } {
    const viewportHeight = window.innerHeight;
    const isSmallScreen = window.innerWidth <= 520;
    const safeTop = isSmallScreen ? 8 : 12;
    const safeBottom = isSmallScreen ? 84 : 96;
    const fabHeight = this.chatFabButton?.nativeElement?.offsetHeight ?? (isSmallScreen ? 56 : 64);
    const gap = isSmallScreen ? 10 : 12;

    const dialogHeight = isSmallScreen
      ? Math.max(320, viewportHeight - 104)
      : Math.min(660, viewportHeight - 120);

    const fabTop = viewportHeight - this.chatFabBottom - fabHeight;
    const fabCenterY = fabTop + (fabHeight / 2);
    const openBelowFab = fabCenterY < (viewportHeight / 2);

    const maxTop = Math.max(safeTop, viewportHeight - dialogHeight - safeBottom);
    const desiredTop = fabTop + fabHeight + gap;
    const top = Math.min(Math.max(desiredTop, safeTop), maxTop);

    const maxBottom = Math.max(safeBottom, viewportHeight - dialogHeight - safeTop);
    const desiredBottom = this.chatFabBottom + fabHeight + gap;
    const bottom = Math.min(Math.max(desiredBottom, safeBottom), maxBottom);

    return { openBelowFab, top, bottom };
  }

  onNavItemClick(): void {
    // Cerrar sidebar en móvil después de navegar
    if (this.isMobile && this.sidenav) {
      this.sidenav.close();
    }
  }

  onValueNavClick(route: string): void {
    this.recordAnalyticsUsage(route);

    if (this.isMobile && this.sidenav) {
      this.sidenav.close();
    }
  }

  onAnalyticsNavClick(route: string): void {
    this.recordAnalyticsUsage(route);

    // Cerrar sidebar en móvil después de navegar
    if (this.isMobile && this.sidenav) {
      this.sidenav.close();
    }
  }

  toggleNavGroup(group: 'gestion' | 'analitica' | 'sistema'): void {
    if (this.isCollapsed && !this.isMobile) {
      return;
    }

    this.navGroups[group] = !this.navGroups[group];
  }

  toggleAnalyticsExpanded(): void {
    this.analyticsExpanded = !this.analyticsExpanded;
    this.persistAnalyticsExpandedState(this.analyticsExpanded);
  }

  get analyticsCoreItems(): AnalyticsMenuItem[] {
    return this.getOrderedAnalyticsItems('core');
  }

  get analyticsExploreItems(): AnalyticsMenuItem[] {
    return this.getOrderedAnalyticsItems('explore');
  }

  get gestionMenuItems(): SidebarMenuItem[] {
    return this.getOrderedSidebarItems(this.gestionMenuItemsBase);
  }

  get sistemaMenuItems(): SidebarMenuItem[] {
    const visibleItems = this.sistemaMenuItemsBase.filter(item => {
      const access = item.access ?? 'all';
      if (access === 'all') {
        return true;
      }
      if (access === 'admin') {
        return this.isAdmin();
      }
      if (access === 'upload') {
        return this.canUploadExcel();
      }
      return true;
    });

    return this.getOrderedSidebarItems(visibleItems);
  }

  get quickAccessItems(): QuickAccessItem[] {
    const unique = this.getQuickAccessSourceItems().reduce((acc, item) => {
      if (!acc.some(existing => existing.route === item.route)) {
        acc.push(item);
      }
      return acc;
    }, [] as QuickAccessItem[]);

    const pinned = this.pinnedRoutes
      .map(route => unique.find(item => item.route === route))
      .filter((item): item is QuickAccessItem => !!item);

    const autoRanked = unique
      .filter(item => !this.pinnedRoutes.includes(item.route))
      .sort((a, b) => {
        const usageA = this.analyticsUsage[a.route] ?? 0;
        const usageB = this.analyticsUsage[b.route] ?? 0;
        const scoreA = a.valueScore + Math.min(usageA, 20);
        const scoreB = b.valueScore + Math.min(usageB, 20);
        return scoreB - scoreA;
      });

    return [...pinned, ...autoRanked]
      .slice(0, this.quickAccessLimit);
  }

  isPinnedRoute(route: string): boolean {
    return this.pinnedRoutes.includes(route);
  }

  togglePinnedRoute(route: string, event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (this.isPinnedRoute(route)) {
      this.pinnedRoutes = this.pinnedRoutes.filter(itemRoute => itemRoute !== route);
    } else {
      this.pinnedRoutes = [route, ...this.pinnedRoutes].slice(0, this.quickAccessLimit);
    }

    this.persistPinnedRoutes();
  }

  trackByAnalyticsRoute(_: number, item: AnalyticsMenuItem): string {
    return item.route;
  }

  trackByMenuRoute(_: number, item: SidebarMenuItem): string {
    return item.route;
  }

  trackByQuickAccessRoute(_: number, item: QuickAccessItem): string {
    return item.route;
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isChatOpen) {
      this.closeChat();
    }
  }

  navigateToChangePassword(): void {
    this.router.navigate(['/change-password']);
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

    return moduleMap[segments[0]] || 'IADS IAsistente Digital de Servicios';
  }

  private loadAnalyticsExpandedState(): boolean {
    try {
      const raw = localStorage.getItem(this.analyticsExpandedStorageKey);
      return raw === '1';
    } catch {
      return false;
    }
  }

  private persistAnalyticsExpandedState(expanded: boolean): void {
    try {
      localStorage.setItem(this.analyticsExpandedStorageKey, expanded ? '1' : '0');
    } catch {
      // ignore storage errors
    }
  }

  private loadAnalyticsUsage(): Record<string, number> {
    try {
      const raw = localStorage.getItem(this.analyticsUsageStorageKey);
      if (!raw) {
        return {};
      }
      const parsed = JSON.parse(raw) as Record<string, number>;
      return parsed ?? {};
    } catch {
      return {};
    }
  }

  private loadPinnedRoutes(): string[] {
    try {
      const raw = localStorage.getItem(this.pinnedRoutesStorageKey);
      if (!raw) {
        return [];
      }
      const parsed = JSON.parse(raw) as string[];
      if (!Array.isArray(parsed)) {
        return [];
      }
      return parsed.filter(route => typeof route === 'string');
    } catch {
      return [];
    }
  }

  private persistPinnedRoutes(): void {
    try {
      localStorage.setItem(this.pinnedRoutesStorageKey, JSON.stringify(this.pinnedRoutes));
    } catch {
      // ignore storage errors
    }
  }

  private recordAnalyticsUsage(route: string): void {
    const current = this.analyticsUsage[route] ?? 0;
    this.analyticsUsage[route] = current + 1;

    try {
      localStorage.setItem(this.analyticsUsageStorageKey, JSON.stringify(this.analyticsUsage));
    } catch {
      // ignore storage errors
    }
  }

  private getOrderedAnalyticsItems(tier: 'core' | 'explore'): AnalyticsMenuItem[] {
    return this.analyticsMenuItems
      .filter(item => item.tier === tier)
      .sort((a, b) => {
        const usageA = this.analyticsUsage[a.route] ?? 0;
        const usageB = this.analyticsUsage[b.route] ?? 0;
        const scoreA = a.valueScore + Math.min(usageA, 20);
        const scoreB = b.valueScore + Math.min(usageB, 20);
        return scoreB - scoreA;
      });
  }

  private getOrderedSidebarItems(items: SidebarMenuItem[]): SidebarMenuItem[] {
    return [...items].sort((a, b) => {
      const usageA = this.analyticsUsage[a.route] ?? 0;
      const usageB = this.analyticsUsage[b.route] ?? 0;
      const scoreA = a.valueScore + Math.min(usageA, 20);
      const scoreB = b.valueScore + Math.min(usageB, 20);
      return scoreB - scoreA;
    });
  }

  private getQuickAccessSourceItems(): QuickAccessItem[] {
    const analyticsCore: QuickAccessItem[] = this.analyticsCoreItems.map(item => ({
      label: item.label,
      icon: item.icon,
      route: item.route,
      valueScore: item.valueScore,
      section: 'analitica'
    }));

    const gestion: QuickAccessItem[] = this.gestionMenuItems.map(item => ({
      label: item.label,
      icon: item.icon,
      route: item.route,
      valueScore: item.valueScore,
      section: 'gestion'
    }));

    const sistema: QuickAccessItem[] = this.sistemaMenuItems.map(item => ({
      label: item.label,
      icon: item.icon,
      route: item.route,
      valueScore: item.valueScore,
      section: 'sistema'
    }));

    return [...analyticsCore, ...gestion, ...sistema];
  }
}

