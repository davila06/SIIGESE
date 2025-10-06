// tenant.service.ts - Servicio principal de Multi-Tenancy para Angular

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap, map } from 'rxjs/operators';

export interface TenantInfo {
  tenantId: string;
  companyName: string;
  domain?: string;
  logoUrl?: string;
  primaryColor: string;
  secondaryColor: string;
  customCss?: string;
  isActive: boolean;
  subscriptionPlan: string;
  maxUsers: number;
  maxPolizas: number;
  createdAt: Date;
  contactName?: string;
  contactEmail?: string;
}

export interface TenantBranding {
  companyName: string;
  logoUrl: string;
  primaryColor: string;
  secondaryColor: string;
  customCss: string;
}

export interface CreateTenantRequest {
  tenantId: string;
  companyName: string;
  adminEmail: string;
  adminFirstName: string;
  adminLastName: string;
  subscriptionPlan?: string;
  domain?: string;
  logoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
}

export interface TenantUsage {
  tenantId: string;
  companyName: string;
  currentUsers: number;
  maxUsers: number;
  currentPolizas: number;
  maxPolizas: number;
  storageUsedMB: number;
  monthlyFee: number;
  lastPaymentDate?: Date;
  nextBillingDate?: Date;
  subscriptionPlan: string;
}

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private readonly apiUrl = '/api/tenants';
  private tenantInfo$ = new BehaviorSubject<TenantInfo | null>(null);
  private branding$ = new BehaviorSubject<TenantBranding | null>(null);
  private isLoading$ = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) {
    this.initializeTenant();
  }

  // ================================
  // PUBLIC OBSERVABLES
  // ================================

  getTenantInfo(): Observable<TenantInfo | null> {
    return this.tenantInfo$.asObservable();
  }

  getBranding(): Observable<TenantBranding | null> {
    return this.branding$.asObservable();
  }

  getLoadingState(): Observable<boolean> {
    return this.isLoading$.asObservable();
  }

  // ================================
  // TENANT DETECTION & INITIALIZATION
  // ================================

  private async initializeTenant(): Promise<void> {
    this.isLoading$.next(true);

    try {
      // 1. Intentar desde localStorage (sesión previa)
      const storedTenant = this.getStoredTenant();
      if (storedTenant) {
        this.setTenantInfo(storedTenant);
      }

      // 2. Detectar tenant desde URL/subdomain
      const detectedTenantId = this.detectTenantFromUrl();
      if (detectedTenantId) {
        await this.loadTenantById(detectedTenantId);
      }

      // 3. Cargar branding independientemente
      await this.loadBranding();

    } catch (error) {
      console.error('Error initializing tenant:', error);
    } finally {
      this.isLoading$.next(false);
    }
  }

  private detectTenantFromUrl(): string | null {
    const hostname = window.location.hostname;
    
    // Detectar subdomain: tenant.siinadseg.com
    if (hostname.includes('.siinadseg.com')) {
      const parts = hostname.split('.');
      if (parts.length >= 3 && parts[0] !== 'www' && parts[0] !== 'api') {
        return parts[0];
      }
    }

    // Detectar desde query parameter: ?tenant=empresa-abc
    const urlParams = new URLSearchParams(window.location.search);
    const tenantParam = urlParams.get('tenant');
    if (tenantParam) {
      return tenantParam;
    }

    // Detectar desde path: /tenant/empresa-abc/dashboard
    const pathMatch = window.location.pathname.match(/^\/tenant\/([^\/]+)/);
    if (pathMatch) {
      return pathMatch[1];
    }

    return null;
  }

  private getStoredTenant(): TenantInfo | null {
    try {
      const stored = localStorage.getItem('tenant_info');
      if (stored) {
        const parsed = JSON.parse(stored);
        // Validar que no esté expirado (24 horas)
        const storedAt = new Date(parsed._storedAt || 0);
        const now = new Date();
        const diffHours = (now.getTime() - storedAt.getTime()) / (1000 * 60 * 60);
        
        if (diffHours < 24) {
          delete parsed._storedAt;
          return parsed;
        } else {
          localStorage.removeItem('tenant_info');
        }
      }
    } catch (error) {
      console.error('Error parsing stored tenant:', error);
      localStorage.removeItem('tenant_info');
    }
    return null;
  }

  // ================================
  // TENANT OPERATIONS
  // ================================

  async loadTenantById(tenantId: string): Promise<TenantInfo | null> {
    if (!tenantId) return null;

    try {
      this.isLoading$.next(true);
      
      const tenant = await this.http.get<TenantInfo>(`${this.apiUrl}/${tenantId}`, {
        headers: this.getHeaders(tenantId)
      }).toPromise();

      if (tenant) {
        this.setTenantInfo(tenant);
        this.storeTenant(tenant);
        return tenant;
      }
      
      return null;
    } catch (error) {
      console.error(`Error loading tenant ${tenantId}:`, error);
      return null;
    } finally {
      this.isLoading$.next(false);
    }
  }

  async loadCurrentTenant(): Promise<TenantInfo | null> {
    try {
      this.isLoading$.next(true);
      
      const tenant = await this.http.get<TenantInfo>(`${this.apiUrl}/current`, {
        headers: this.getHeaders()
      }).toPromise();

      if (tenant) {
        this.setTenantInfo(tenant);
        this.storeTenant(tenant);
        return tenant;
      }
      
      return null;
    } catch (error) {
      console.error('Error loading current tenant:', error);
      return null;
    } finally {
      this.isLoading$.next(false);
    }
  }

  async loadBranding(): Promise<TenantBranding | null> {
    try {
      const branding = await this.http.get<TenantBranding>(`${this.apiUrl}/current/branding`, {
        headers: this.getHeaders()
      }).toPromise();

      if (branding) {
        this.branding$.next(branding);
        this.applyBranding(branding);
        return branding;
      }
      
      return null;
    } catch (error) {
      console.error('Error loading branding:', error);
      // Aplicar branding por defecto
      const defaultBranding: TenantBranding = {
        companyName: 'SIINADSEG',
        logoUrl: '/assets/default-logo.png',
        primaryColor: '#1976d2',
        secondaryColor: '#424242',
        customCss: ''
      };
      this.branding$.next(defaultBranding);
      this.applyBranding(defaultBranding);
      return defaultBranding;
    }
  }

  async createTenant(request: CreateTenantRequest): Promise<TenantInfo | null> {
    try {
      this.isLoading$.next(true);
      
      const tenant = await this.http.post<TenantInfo>(`${this.apiUrl}`, request).toPromise();
      
      if (tenant) {
        console.log('Tenant created successfully:', tenant.tenantId);
        return tenant;
      }
      
      return null;
    } catch (error) {
      console.error('Error creating tenant:', error);
      throw error;
    } finally {
      this.isLoading$.next(false);
    }
  }

  async checkTenantAvailability(tenantId: string): Promise<{ available: boolean; tenantId: string }> {
    try {
      const result = await this.http.get<{ available: boolean; tenantId: string }>(
        `${this.apiUrl}/check-availability/${tenantId}`
      ).toPromise();
      
      return result || { available: false, tenantId };
    } catch (error) {
      console.error('Error checking tenant availability:', error);
      return { available: false, tenantId };
    }
  }

  async getTenantUsage(): Promise<TenantUsage | null> {
    try {
      const usage = await this.http.get<TenantUsage>(`${this.apiUrl}/current/usage`, {
        headers: this.getHeaders()
      }).toPromise();
      
      return usage || null;
    } catch (error) {
      console.error('Error getting tenant usage:', error);
      return null;
    }
  }

  // ================================
  // UTILITY METHODS
  // ================================

  getCurrentTenant(): TenantInfo | null {
    return this.tenantInfo$.value;
  }

  getCurrentTenantId(): string | null {
    return this.tenantInfo$.value?.tenantId || null;
  }

  hasTenant(): boolean {
    return this.tenantInfo$.value !== null;
  }

  isMultiTenantUrl(): boolean {
    const hostname = window.location.hostname;
    return hostname.includes('.siinadseg.com') || 
           window.location.search.includes('tenant=') ||
           window.location.pathname.includes('/tenant/');
  }

  switchTenant(tenantId: string): void {
    // Limpiar tenant actual
    this.clearTenant();
    
    // Redirigir con nuevo tenant
    if (window.location.hostname.includes('.siinadseg.com')) {
      // Cambiar subdomain
      const newUrl = `https://${tenantId}.siinadseg.com${window.location.pathname}`;
      window.location.href = newUrl;
    } else {
      // Usar query parameter
      const url = new URL(window.location.href);
      url.searchParams.set('tenant', tenantId);
      window.location.href = url.toString();
    }
  }

  clearTenant(): void {
    this.tenantInfo$.next(null);
    this.branding$.next(null);
    localStorage.removeItem('tenant_info');
    this.removeBrandingStyles();
  }

  // ================================
  // PRIVATE METHODS
  // ================================

  private setTenantInfo(tenant: TenantInfo): void {
    this.tenantInfo$.next(tenant);
  }

  private storeTenant(tenant: TenantInfo): void {
    try {
      const toStore = {
        ...tenant,
        _storedAt: new Date().toISOString()
      };
      localStorage.setItem('tenant_info', JSON.stringify(toStore));
    } catch (error) {
      console.error('Error storing tenant:', error);
    }
  }

  private getHeaders(tenantId?: string): HttpHeaders {
    let headers = new HttpHeaders();
    
    const currentTenantId = tenantId || this.getCurrentTenantId();
    if (currentTenantId) {
      headers = headers.set('X-Tenant-ID', currentTenantId);
    }
    
    return headers;
  }

  private applyBranding(branding: TenantBranding): void {
    try {
      // Aplicar CSS variables
      document.documentElement.style.setProperty('--primary-color', branding.primaryColor);
      document.documentElement.style.setProperty('--secondary-color', branding.secondaryColor);
      
      // Actualizar título de la página
      document.title = `${branding.companyName} - SIINADSEG`;
      
      // Actualizar favicon si hay logo
      if (branding.logoUrl) {
        this.updateFavicon(branding.logoUrl);
      }
      
      // Aplicar CSS custom
      if (branding.customCss) {
        this.applyCustonCSS(branding.customCss);
      }
      
      // Actualizar meta tags
      this.updateMetaTags(branding);
      
    } catch (error) {
      console.error('Error applying branding:', error);
    }
  }

  private updateFavicon(logoUrl: string): void {
    try {
      let link: HTMLLinkElement = document.querySelector("link[rel*='icon']") || document.createElement('link');
      link.type = 'image/x-icon';
      link.rel = 'shortcut icon';
      link.href = logoUrl;
      document.getElementsByTagName('head')[0].appendChild(link);
    } catch (error) {
      console.error('Error updating favicon:', error);
    }
  }

  private applyCustonCSS(customCss: string): void {
    try {
      // Remover CSS custom anterior
      const existingStyle = document.getElementById('tenant-custom-css');
      if (existingStyle) {
        existingStyle.remove();
      }
      
      // Agregar nuevo CSS custom
      if (customCss.trim()) {
        const style = document.createElement('style');
        style.id = 'tenant-custom-css';
        style.textContent = customCss;
        document.head.appendChild(style);
      }
    } catch (error) {
      console.error('Error applying custom CSS:', error);
    }
  }

  private updateMetaTags(branding: TenantBranding): void {
    try {
      // Update meta description
      let metaDescription = document.querySelector('meta[name="description"]') as HTMLMetaElement;
      if (metaDescription) {
        metaDescription.content = `${branding.companyName} - Sistema de gestión de seguros`;
      }
      
      // Update meta keywords
      let metaKeywords = document.querySelector('meta[name="keywords"]') as HTMLMetaElement;
      if (metaKeywords) {
        metaKeywords.content = `${branding.companyName}, seguros, pólizas, cobros, reclamos`;
      }
    } catch (error) {
      console.error('Error updating meta tags:', error);
    }
  }

  private removeBrandingStyles(): void {
    try {
      // Remover CSS variables custom
      document.documentElement.style.removeProperty('--primary-color');
      document.documentElement.style.removeProperty('--secondary-color');
      
      // Remover CSS custom
      const customStyle = document.getElementById('tenant-custom-css');
      if (customStyle) {
        customStyle.remove();
      }
      
      // Restaurar título por defecto
      document.title = 'SIINADSEG';
    } catch (error) {
      console.error('Error removing branding styles:', error);
    }
  }
}