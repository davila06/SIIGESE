import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';

import {
  AnalyticsService,
  AgendaDto,
  AgendaSeccionDto,
  AgendaItemDto
} from '../../services/analytics.service';

@Component({
  selector: 'app-agenda-inteligente',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatChipsModule, MatDividerModule, MatBadgeModule
  ],
  templateUrl: './agenda-inteligente.component.html',
  styleUrls: ['./agenda-inteligente.component.scss']
})
export class AgendaInteligenteComponent implements OnInit {

  agenda: AgendaDto | null = null;
  loading = true;
  error   = '';
  lastUpdated: Date | null = null;

  constructor(private readonly analyticsService: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error   = '';
    this.analyticsService.getAgenda().subscribe({
      next: data => {
        this.agenda      = data;
        this.lastUpdated = new Date();
        this.loading     = false;
      },
      error: () => {
        this.error   = 'No se pudo cargar la agenda. Intente de nuevo.';
        this.loading = false;
      }
    });
  }

  // ── Helpers ────────────────────────────────────────────────────────────────

  getNivelClass(nivel: string): string {
    switch (nivel) {
      case 'critico': return 'nivel-critico';
      case 'alerta':  return 'nivel-alerta';
      case 'aviso':   return 'nivel-aviso';
      default:        return 'nivel-info';
    }
  }

  getNivelIcon(nivel: string): string {
    switch (nivel) {
      case 'critico': return 'crisis_alert';
      case 'alerta':  return 'warning_amber';
      case 'aviso':   return 'notifications_active';
      default:        return 'info_outline';
    }
  }

  getTipoIcon(tipo: string): string {
    switch (tipo) {
      case 'cobro':      return 'payments';
      case 'poliza':     return 'policy';
      case 'reclamo':    return 'report_problem';
      case 'lead':       return 'local_fire_department';
      case 'cotizacion': return 'request_quote';
      default:           return 'task_alt';
    }
  }

  formatCRC(value?: number): string {
    if (value == null) return '';
    return '₡' + new Intl.NumberFormat('es-CR').format(Math.round(value));
  }

  formatFecha(iso: string): string {
    return new Date(iso).toLocaleDateString('es-CR', {
      weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
    });
  }

  trackByTitulo(_: number, item: AgendaItemDto): string {
    return item.titulo;
  }

  trackBySeccion(_: number, s: AgendaSeccionDto): string {
    return s.titulo;
  }
}
