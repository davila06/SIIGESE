import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  PortfolioDistribucionDto,
  VencimientosTimelineDto,
  VencimientoDetalleMesDto,
  PortfolioBreakdownDto
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-portfolio-analytics',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatSelectModule, MatFormFieldModule, MatTableModule, MatTabsModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './portfolio-analytics.component.html',
  styleUrls: ['./portfolio-analytics.component.scss']
})
export class PortfolioAnalyticsComponent implements OnInit {
  portfolio: PortfolioDistribucionDto | null = null;
  vencimientos: VencimientosTimelineDto | null = null;
  vencimientoDetalle: VencimientoDetalleMesDto | null = null;
  loading = true;
  error = '';
  detailLoading = false;
  vencimientosMeses = 12;
  portfolioMetricMode: 'prima' | 'count' = 'prima';

  readonly legendBelow = LegendPosition.Below;
  readonly colorScheme: Color = {
    name: 'portfolio', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#8B5CF6', '#10B981', '#F59E0B', '#EF4444', '#3B82F6']
  };
  readonly vencColorScheme: Color = {
    name: 'venc', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#10B981', '#F59E0B', '#8B5CF6']
  };
  readonly trendColorScheme: Color = {
    name: 'trend', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#EF4444']
  };
  readonly retentionColorScheme: Color = {
    name: 'retention', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#10B981', '#EF4444']
  };

  aseguradoraChart: any[] = [];
  tipoSeguroChart: any[] = [];
  modalidadChart: any[] = [];
  frecuenciaChart: any[] = [];
  radarMatrixChart: any[] = [];
  vencimientosStackedChart: any[] = [];
  vencimientosRiesgoLine: any[] = [];
  retencionFunnelChart: any[] = [];
  retencionMensualLine: any[] = [];
  retencionPorTipoChart: any[] = [];
  churnAseguradoraChart: any[] = [];
  histogramaChart: any[] = [];
  riesgoBubbleChart: any[] = [];
  radarTableData: any[] = [];

  readonly composicionColumns = ['name', 'totalPolizas', 'primaMensual', 'porcentajePolizas', 'porcentajePrima'];
  readonly radarColumns = ['aseguradora', 'numPolizas', 'primaPromedio', 'tasaReclamos', 'tasaCobro', 'antiguedadPromedioDias'];
  readonly riesgoColumns = ['segmento', 'primaUnitariaPromedio', 'montoAseguradoProxy', 'numeroClientes'];
  readonly detalleColumns = ['numeroPoliza', 'clienteNombre', 'aseguradora', 'tipoSeguro', 'fechaVigencia', 'primaMensualNormalizada'];

  composicionActiva: PortfolioBreakdownDto[] = [];

  constructor(private readonly svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      portfolio: this.svc.getPortfolio(),
      venc: this.svc.getVencimientos(this.vencimientosMeses)
    }).subscribe({
      next: ({ portfolio, venc }) => {
        this.portfolio = portfolio;
        this.vencimientos = venc;
        this.buildCharts();
        this.onCompositionTabChange(0);
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando analítica de portfolio.';
        this.loading = false;
      }
    });
  }

  private buildCharts(): void {
    if (!this.portfolio) {
      return;
    }

    const valueMapper = (rows: PortfolioBreakdownDto[]) => rows.map(r => ({
      name: r.name,
      value: this.portfolioMetricMode === 'prima' ? r.primaMensual : r.totalPolizas
    }));

    this.aseguradoraChart = valueMapper(this.portfolio.composicionPorAseguradora || []);
    this.tipoSeguroChart = valueMapper(this.portfolio.composicionPorTipoSeguro || []);
    this.modalidadChart = valueMapper(this.portfolio.composicionPorModalidad || []);
    this.frecuenciaChart = valueMapper(this.portfolio.composicionPorFrecuencia || []);

    this.radarMatrixChart = this.portfolio.radarAseguradoras.map(r => ({
      name: r.aseguradora,
      series: [
        { name: 'Prima promedio', value: Number((r.primaPromedio / 1000).toFixed(1)) },
        { name: 'N° pólizas', value: r.numPolizas },
        { name: 'Tasa reclamos %', value: r.tasaReclamos },
        { name: 'Tasa cobro %', value: r.tasaCobro },
        { name: 'Antigüedad (días)', value: Number((r.antiguedadPromedioDias / 30).toFixed(1)) }
      ]
    }));

    this.radarTableData = (this.portfolio.radarAseguradoras || []).map((r: any) => ({
      ...r,
      antiguedadPromedioDias: r.antiguedadPromedioDias ?? 0
    }));

    if (this.vencimientos) {
      this.vencimientosStackedChart = this.vencimientos.meses.map(m => ({
        name: m.mesLabel,
        series: [
          { name: 'AUTO', value: m.auto },
          { name: 'VIDA', value: m.vida },
          { name: 'HOGAR', value: m.hogar },
          { name: 'EMPRESARIAL', value: m.empresarial }
        ]
      }));

      this.vencimientosRiesgoLine = [
        {
          name: 'Prima en riesgo',
          series: this.vencimientos.meses.map(m => ({ name: m.mesLabel, value: m.primaEnRiesgo }))
        }
      ];
    }

    this.retencionFunnelChart = this.portfolio.retencion?.funnel || [];
    this.retencionMensualLine = [
      {
        name: 'Retención mensual %',
        series: this.portfolio.retencion?.tasaRetencionMensual || []
      }
    ];
    this.retencionPorTipoChart = this.portfolio.retencion?.retencionPorTipoSeguro || [];
    this.churnAseguradoraChart = this.portfolio.retencion?.churnPorAseguradora || [];

    this.histogramaChart = (this.portfolio.histogramaPrima || []).map(b => ({ name: b.rango, value: b.cantidad }));

    this.riesgoBubbleChart = (this.portfolio.mapaRiesgo || [])
      .reduce((acc, item) => {
        let serie = acc.find(s => s.name === item.tipoSeguro);
        if (!serie) {
          serie = { name: item.tipoSeguro, series: [] as any[] };
          acc.push(serie);
        }
        serie.series.push({
          name: item.segmento,
          x: item.primaUnitariaPromedio,
          y: item.montoAseguradoProxy,
          r: Math.max(4, item.numeroClientes)
        });
        return acc;
      }, [] as any[]);
  }

  onMetricModeChange(): void {
    this.buildCharts();
  }

  onCompositionTabChange(index: number): void {
    if (!this.portfolio) {
      this.composicionActiva = [];
      return;
    }

    const tabs = [
      this.portfolio.composicionPorAseguradora,
      this.portfolio.composicionPorTipoSeguro,
      this.portfolio.composicionPorModalidad,
      this.portfolio.composicionPorFrecuencia
    ];

    this.composicionActiva = tabs[index] || [];
  }

  onTimelineSelect(event: any): void {
    const monthLabel = event?.name || event?.series;
    if (!monthLabel || !this.vencimientos) {
      return;
    }

    const month = this.vencimientos.meses.find(m => m.mesLabel === monthLabel);
    if (!month) {
      return;
    }

    this.detailLoading = true;
    this.svc.getVencimientosDetalle(month.mes).subscribe({
      next: detail => {
        this.vencimientoDetalle = detail;
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
      }
    });
  }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v);
  }

  formatPercent(v: number): string {
    return `${v.toFixed(1)}%`;
  }

  yAxisFmt = (v: number) =>
    new Intl.NumberFormat('es-CR', { notation: 'compact', minimumFractionDigits: 0 }).format(v);
}
