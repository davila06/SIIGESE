import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  CotizacionesFunnelDto,
  VelocidadBucketDto,
  TicketPromedioDto
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';

@Component({
  selector: 'app-sales-funnel',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatTableModule, MatSelectModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './sales-funnel.component.html',
  styleUrls: ['./sales-funnel.component.scss']
})
export class SalesFunnelComponent implements OnInit {
  data: CotizacionesFunnelDto | null = null;
  loading = true;
  error = '';

  // 4.1 Funnel stages data
  funnelStages: { label: string; count: number; pct: number; color: string }[] = [];
  funnelChart: any[] = [];
  // 4.2 Velocidad histogram
  velocidadChart: any[] = [];
  // 4.3 Pipeline valor lines
  pipelineChart: any[] = [];
  // 4.4 Perdidas (rechazadas) por aseguradora
  perdidasChart: any[] = [];
  // 4.5 Ticket promedio grouped bar 2D (TipoSeguro x Modalidad)
  ticketBarChart: any[] = [];
  // Tipo segment (conversiones)
  tipoChart: any[] = [];

  // Ticket table columns
  readonly ticketColumns = ['tipoSeguro', 'modalidad', 'primaPromedio', 'cantidad'];

  readonly legendBelow = LegendPosition.Below;

  readonly colorScheme: Color = {
    name: 'sales', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#10B981', '#8B5CF6', '#EF4444', '#F59E0B']
  };
  readonly pipelineColors: Color = {
    name: 'pipeline', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#F59E0B', '#10B981', '#00E5FF']
  };
  readonly perdidasColors: Color = {
    name: 'perdidas', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#EF4444', '#F87171', '#FCA5A5', '#FECACA', '#FEE2E2', '#991B1B']
  };
  readonly velocidadColors: Color = {
    name: 'velocidad', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#10B981', '#00E5FF', '#F59E0B', '#F97316', '#EF4444']
  };
  readonly ticketColors: Color = {
    name: 'ticket', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#8B5CF6', '#10B981', '#F59E0B', '#3B82F6']
  };

  constructor(private svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.svc.getCotizacionesFunnel().subscribe({
      next: d => {
        this.data = d;
        this.buildCharts(d);
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando datos del funnel de ventas.';
        this.loading = false;
      }
    });
  }

  private buildCharts(d: CotizacionesFunnelDto): void {
    // 4.1 Funnel stages with conversion rates
    const t = d.total || 1;
    this.funnelStages = [
      { label: 'Total creadas',   count: d.total,      pct: 100,                                         color: '#64748b' },
      { label: 'Pendientes',      count: d.pendientes,  pct: Math.round(d.pendientes / t * 100),          color: '#F59E0B' },
      { label: 'Aprobadas',       count: d.aprobadas,   pct: Math.round(d.aprobadas / t * 100),           color: '#10B981' },
      { label: 'Convertidas',     count: d.convertidas, pct: Math.round(d.convertidas / t * 100),         color: '#00E5FF' },
      { label: 'Rechazadas',      count: d.rechazadas,  pct: Math.round(d.rechazadas / t * 100),          color: '#EF4444' }
    ];
    this.funnelChart = this.funnelStages.map(s => ({ name: s.label, value: s.count }));

    // 4.2 Velocidad histogram
    this.velocidadChart = d.velocidadBuckets.map(b => ({ name: b.rango, value: b.cantidad }));

    // 4.3 Pipeline de valor mensual (multi-series lines)
    if (d.pipelineValor.length) {
      this.pipelineChart = [
        { name: 'Pendiente',  series: d.pipelineValor.map(p => ({ name: p.mes, value: p.valorPendiente })) },
        { name: 'Aprobado',   series: d.pipelineValor.map(p => ({ name: p.mes, value: p.valorAprobado })) },
        { name: 'Convertido', series: d.pipelineValor.map(p => ({ name: p.mes, value: p.valorConvertido })) }
      ];
    }

    // 4.4 Perdidas: rechazadas by aseguradora
    this.perdidasChart = d.porAseguradora.map(a => ({ name: a.name, value: a.value }));

    // 4.1 Conversiones by tipo de seguro (bar horizontal)
    this.tipoChart = d.porTipoSeguro.map(t => ({ name: t.name, value: t.value }));

    // 4.5 Ticket promedio grouped: series por Modalidad, groups por TipoSeguro
    const modalidades = [...new Set(d.ticketPromedio.map(t => t.modalidad))].sort();
    const tipos = [...new Set(d.ticketPromedio.map(t => t.tipoSeguro))].sort();
    this.ticketBarChart = tipos.map(tipo => ({
      name: tipo,
      series: modalidades.map(mod => {
        const row = d.ticketPromedio.find(r => r.tipoSeguro === tipo && r.modalidad === mod);
        return { name: mod, value: row?.primaPromedio ?? 0 };
      })
    }));
  }

  conversionRate(from: number, of: number): number {
    return of > 0 ? Math.round(from / of * 100) : 0;
  }

  velocidadClass(dias: number): string {
    if (dias <= 7)  return 'chip--success';
    if (dias <= 15) return 'chip--info';
    if (dias <= 30) return 'chip--warning';
    return 'chip--danger';
  }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v);
  }

  formatVelocidadBadge(v: number | null | undefined): string {
    return `${(v ?? 0).toFixed(1)} días prom`;
  }

  yAxisFmt = (v: number): string =>
    new Intl.NumberFormat('es-CR', { notation: 'compact', minimumFractionDigits: 0 }).format(v);
}
