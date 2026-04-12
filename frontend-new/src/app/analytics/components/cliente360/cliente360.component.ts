import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { NgxChartsModule, Color, ScaleType } from '@swimlane/ngx-charts';
import { curveMonotoneX } from 'd3-shape';

import {
  AnalyticsService,
  Cliente360Dto,
  Cliente360SearchResultDto,
  Cliente360PolizaDto,
  Cliente360CobroDto,
  Cliente360ReclamoDto,
  ChartDataPoint
} from '../../services/analytics.service';

@Component({
  selector: 'app-cliente360',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatChipsModule, MatBadgeModule, MatTabsModule,
    MatTableModule, MatFormFieldModule, MatInputModule,
    MatDividerModule, NgxChartsModule
  ],
  templateUrl: './cliente360.component.html',
  styleUrl: './cliente360.component.scss'
})
export class Cliente360Component implements OnInit {

  // ── State ────────────────────────────────────────────────────────────────
  searchQuery     = '';
  searchResults: Cliente360SearchResultDto[] = [];
  searching       = false;
  searchError     = '';

  cliente: Cliente360Dto | null = null;
  loading         = false;
  loadError       = '';

  // ── Columnas de tablas ───────────────────────────────────────────────────
  readonly polizasColumns  = ['tipo', 'aseguradora', 'prima', 'frecuencia', 'vigencia', 'estado'];
  readonly cobrosColumns   = ['recibo', 'estado', 'monto', 'vencimiento', 'diasVencido'];
  readonly reclamosColumns = ['numero', 'tipo', 'estado', 'prioridad', 'montoClamado', 'montoAprobado', 'resolucion'];

  // ── ngx-charts ──────────────────────────────────────────────────────────
  ltvChartData: { name: string; series: ChartDataPoint[] }[] = [];
  readonly ltvCurve = curveMonotoneX;
  colorScheme: Color = {
    name: 'ltv',
    selectable: true,
    group: ScaleType.Ordinal,
    domain: ['#6366F1']
  };

  get hasLtvData(): boolean {
    return this.ltvChartData.length > 0 && (this.ltvChartData[0].series?.length ?? 0) > 0;
  }

  constructor(private readonly analyticsService: AnalyticsService) {}

  ngOnInit(): void { /* awaita para búsqueda con debounce */ }

  // ── Búsqueda ─────────────────────────────────────────────────────────────
  onSearch(): void {
    const q = (this.searchQuery ?? '').trim();
    if (q.length < 2) {
      this.searchResults = [];
      return;
    }
    this.searching   = true;
    this.searchError = '';
    this.analyticsService.searchClientes(q).subscribe({
      next: res => {
        this.searchResults = res;
        this.searching     = false;
      },
      error: () => {
        this.searchError = 'Error al buscar clientes. Intente nuevamente.';
        this.searching   = false;
      }
    });
  }

  onSelectCliente(cedula: string): void {
    this.searchResults = [];
    this.loadCliente(cedula);
  }

  loadCliente(cedula: string): void {
    this.loading   = true;
    this.loadError = '';
    this.cliente   = null;
    this.analyticsService.getCliente360(cedula).subscribe({
      next: data => {
        this.cliente     = data;
        this.ltvChartData = [{
          name: 'Prima cobrada mensual',
          series: (data.ltvTimeline ?? []).map((p: ChartDataPoint) => ({
            name:  p.name,
            value: p.value
          }))
        }];
        this.loading = false;
      },
      error: () => {
        this.loadError = 'No se pudo cargar la información del cliente.';
        this.loading   = false;
      }
    });
  }

  clearCliente(): void {
    this.cliente    = null;
    this.searchQuery = '';
    this.searchResults = [];
  }

  // ── Helpers para colores y texto ─────────────────────────────────────────
  getRiesgoClass(categoria: string): string {
    const map: Record<string, string> = {
      Verde:    'riesgo--verde',
      Amarillo: 'riesgo--amarillo',
      Rojo:     'riesgo--rojo'
    };
    return map[categoria] ?? '';
  }

  getEstadoCobroClass(estado: string): string {
    const map: Record<string, string> = {
      Cobrado:   'estado--cobrado',
      Pagado:    'estado--cobrado',
      Pendiente: 'estado--pendiente',
      Vencido:   'estado--vencido',
      Cancelado: 'estado--cancelado'
    };
    return map[estado] ?? '';
  }

  getPrioridadClass(prioridad: string): string {
    const map: Record<string, string> = {
      Critica: 'prioridad--critica',
      Alta:    'prioridad--alta',
      Media:   'prioridad--media',
      Baja:    'prioridad--baja'
    };
    return map[prioridad] ?? '';
  }

  getEstadoReclamoClass(estado: string): string {
    const map: Record<string, string> = {
      Resuelto:  'estado--resuelto',
      Cerrado:   'estado--resuelto',
      Rechazado: 'estado--rechazado',
      Pendiente: 'estado--pendiente',
      Aprobado:  'estado--aprobado'
    };
    return map[estado] ?? 'estado--pendiente';
  }

  formatCRC(value: number | undefined): string {
    if (value == null) return '—';
    return new Intl.NumberFormat('es-CR', {
      style: 'currency', currency: 'CRC', minimumFractionDigits: 0
    }).format(value);
  }

  formatFecha(dateStr: string | undefined): string {
    if (!dateStr) return '—';

    const raw = dateStr.trim();
    if (!raw || raw.startsWith('0001-01-01') || raw.startsWith('0000-00-00')) {
      return '—';
    }

    const d = new Date(raw);
    if (Number.isNaN(d.getTime()) || d.getFullYear() <= 1900) {
      return '—';
    }

    return d.toLocaleDateString('es-CR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  getPolizaEstadoLabel(p: Cliente360PolizaDto): string {
    if (!p.esActiva) return 'Inactiva';
    const dias = p.diasParaVencer;
    if (dias < 0)  return 'Vencida';
    if (dias <= 7) return `Vence en ${dias}d`;
    return 'Activa';
  }

  getPolizaEstadoClass(p: Cliente360PolizaDto): string {
    if (!p.esActiva) return 'badge--inactiva';
    if (p.diasParaVencer < 0)  return 'badge--vencida';
    if (p.diasParaVencer <= 7) return 'badge--alerta';
    return 'badge--activa';
  }

  getDiasVencidoLabel(dias: number): string {
    if (dias < 0) return `En ${Math.abs(dias)}d`;
    if (dias === 0) return 'Hoy';
    return `${dias}d vencido`;
  }

  getDiasVencidoClass(dias: number): string {
    if (dias < 0)   return 'dias--ok';
    if (dias > 30)  return 'dias--critico';
    if (dias > 0)   return 'dias--vencido';
    return 'dias--hoy';
  }

  getResolucionLabel(dias: number): string {
    return dias < 0 ? 'En proceso' : `${dias} días`;
  }

  getScoreLealtadClass(score: number): string {
    if (score >= 70) return 'score--verde';
    if (score >= 40) return 'score--amarillo';
    return 'score--rojo';
  }

  getScoreRiesgoClass(score: number): string {
    if (score >= 60) return 'score--rojo';
    if (score >= 30) return 'score--amarillo';
    return 'score--verde';
  }

  trackByPoliza(i: number, p: Cliente360PolizaDto): string { return p.numeroPoliza; }
  trackByCobro(i: number, c: Cliente360CobroDto): string   { return c.numeroRecibo; }
  trackByReclamo(i: number, r: Cliente360ReclamoDto): string { return r.numeroReclamo; }
  trackByResult(i: number, r: Cliente360SearchResultDto): string { return r.cedula; }
}
