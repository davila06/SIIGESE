import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class TitleService {
  private titleMap: { [key: string]: string } = {
    '/login': 'Iniciar Sesión',
    '/polizas': 'Gestión de Pólizas',
    '/polizas/upload': 'Cargar Pólizas Excel',
    '/cobros': 'Gestión de Cobros',
    '/reclamos': 'Gestión de Reclamos',
    '/cotizaciones': 'Gestión de Cotizaciones',
    '/emails': 'Notificaciones Email',
    '/analytics/reportes': 'Reportes Exportables',
    '/usuarios': 'Gestión de Usuarios',
    '/configuracion': 'Configuración del Sistema',
    '/change-password': 'Cambiar Contraseña'
  };

  constructor(
    private title: Title,
    private router: Router
  ) {
    // Escuchar cambios de ruta para actualizar el título automáticamente
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.updateTitle(event.url);
      });
  }

  updateTitle(url: string): void {
    // Obtener el módulo actual basado en la URL
    const moduleTitle = this.getModuleTitle(url);
    const fullTitle = `${moduleTitle} - IADS IAsistente Digital de Servicios`;
    this.title.setTitle(fullTitle);
  }

  getModuleTitle(url: string): string {
    // Buscar coincidencia exacta primero
    if (this.titleMap[url]) {
      return this.titleMap[url];
    }

    // Buscar coincidencia parcial para rutas dinámicas
    for (const route in this.titleMap) {
      if (url.startsWith(route) && route !== '/') {
        return this.titleMap[route];
      }
    }

    // Título por defecto
    return 'IADS IAsistente Digital de Servicios';
  }

  getCurrentModule(): string {
    const url = this.router.url;
    return this.getModuleTitle(url);
  }

  setCustomTitle(customTitle: string): void {
    const fullTitle = `${customTitle} - IADS IAsistente Digital de Servicios`;
    this.title.setTitle(fullTitle);
  }
}

