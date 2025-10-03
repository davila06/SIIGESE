import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class TitleService {
  private readonly baseTitle = 'SIIGESE';
  
  private moduleNames: { [key: string]: string } = {
    'dashboard': 'Dashboard',
    'polizas': 'Gestión de Pólizas',
    'clientes': 'Gestión de Clientes',
    'cobros': 'Gestión de Cobros',
    'emails': 'Sistema de Emails',
    'reclamos': 'Gestión de Reclamos',
    'usuarios': 'Gestión de Usuarios',
    'reportes': 'Reportes y Estadísticas',
    'configuracion': 'Configuración del Sistema'
  };

  constructor(
    private titleService: Title,
    private router: Router
  ) {
    this.initTitleUpdates();
  }

  private initTitleUpdates(): void {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.updateTitle((event as NavigationEnd).url);
      });
  }

  updateTitle(url: string): void {
    const segments = url.split('/').filter(segment => segment);
    let moduleName = '';

    if (segments.length > 0) {
      const mainModule = segments[0];
      moduleName = this.moduleNames[mainModule] || this.capitalizeFirst(mainModule);
    }

    const title = moduleName 
      ? `${this.baseTitle} - ${moduleName}`
      : this.baseTitle;

    this.titleService.setTitle(title);
  }

  setCustomTitle(customTitle: string): void {
    const title = `${this.baseTitle} - ${customTitle}`;
    this.titleService.setTitle(title);
  }

  private capitalizeFirst(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }
}