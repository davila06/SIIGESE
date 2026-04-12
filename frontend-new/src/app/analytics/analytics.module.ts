import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatDividerModule } from '@angular/material/divider';

const routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/executive-dashboard/executive-dashboard.component').then(
        m => m.ExecutiveDashboardComponent
      )
  },
  {
    path: 'cobros',
    loadComponent: () =>
      import('./components/cobros-analytics/cobros-analytics.component').then(
        m => m.CobrosAnalyticsComponent
      )
  },
  {
    path: 'portfolio',
    loadComponent: () =>
      import('./components/portfolio-analytics/portfolio-analytics.component').then(
        m => m.PortfolioAnalyticsComponent
      )
  },
  {
    path: 'reclamos',
    loadComponent: () =>
      import('./components/reclamos-analytics/reclamos-analytics.component').then(
        m => m.ReclamosAnalyticsComponent
      )
  },
  {
    path: 'ventas',
    loadComponent: () =>
      import('./components/sales-funnel/sales-funnel.component').then(
        m => m.SalesFunnelComponent
      )
  },
  {
    path: 'emails',
    loadComponent: () =>
      import('./components/email-analytics/email-analytics.component').then(
        m => m.EmailAnalyticsComponent
      )
  },
  {
    path: 'operacional',
    loadComponent: () =>
      import('./components/operational-analytics/operational-analytics.component').then(
        m => m.OperationalAnalyticsComponent
      )
  },
  {
    path: 'predictivo',
    loadComponent: () =>
      import('./components/predictive-analytics/predictive-analytics.component').then(
        m => m.PredictiveAnalyticsComponent
      )
  },
  {
    path: 'agenda',
    loadComponent: () =>
      import('./components/agenda-inteligente/agenda-inteligente.component').then(
        m => m.AgendaInteligenteComponent
      )
  },
  {
    path: 'cliente360',
    loadComponent: () =>
      import('./components/cliente360/cliente360.component').then(
        m => m.Cliente360Component
      )
  },
  {
    path: 'reportes',
    loadComponent: () =>
      import('./components/reportes-exportables/reportes-exportables.component').then(
        m => m.ReportesExportablesComponent
      )
  }
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatTabsModule,
    MatChipsModule,
    MatTableModule,
    MatDividerModule
  ]
})
export class AnalyticsModule {}
