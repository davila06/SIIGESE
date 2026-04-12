import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-chart-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  template: `
    <mat-card class="chart-card" [class.chart-card--loading]="loading">
      <mat-card-header>
        <div class="chart-card__header">
          <div class="chart-card__title-row">
            <mat-icon *ngIf="icon" class="chart-card__icon">{{ icon }}</mat-icon>
            <div>
              <h3 class="chart-card__title">{{ title }}</h3>
              <p *ngIf="subtitle" class="chart-card__subtitle">{{ subtitle }}</p>
            </div>
          </div>
          <div class="chart-card__actions" *ngIf="badge || tooltip">
            <span *ngIf="badge" class="chart-card__badge" [class]="'chart-card__badge--' + badgeType">
              {{ badge }}
            </span>
            <mat-icon *ngIf="tooltip" class="chart-card__info" [matTooltip]="tooltip" matTooltipPosition="left">
              info_outline
            </mat-icon>
          </div>
        </div>
      </mat-card-header>
      <mat-card-content class="chart-card__content">
        <div *ngIf="loading" class="chart-card__spinner">
          <mat-spinner diameter="40"></mat-spinner>
        </div>
        <div *ngIf="!loading && error" class="chart-card__error">
          <mat-icon>error_outline</mat-icon>
          <span>{{ error }}</span>
        </div>
        <ng-content *ngIf="!loading && !error"></ng-content>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .chart-card {
      border-radius: 12px;
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      box-shadow: 0 2px 8px rgba(0,0,0,.08);
      height: 100%;
      display: flex;
      flex-direction: column;
    }
    .chart-card--loading { pointer-events: none; }
    .chart-card__header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      width: 100%;
      padding: 16px 16px 0;
    }
    .chart-card__title-row {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .chart-card__icon { color: var(--color-primary); font-size: 20px; }
    .chart-card__title {
      margin: 0;
      font-size: 15px;
      font-weight: 600;
      color: var(--text-primary);
      line-height: 1.2;
    }
    .chart-card__subtitle {
      margin: 2px 0 0;
      font-size: 12px;
      color: var(--text-secondary);
    }
    .chart-card__actions { display: flex; align-items: center; gap: 6px; }
    .chart-card__badge {
      font-size: 11px;
      font-weight: 600;
      padding: 2px 8px;
      border-radius: 12px;
      background: var(--color-primary-bg);
      color: var(--color-primary);
    }
    .chart-card__badge--danger { background: rgba(239,68,68,.12); color: #ef4444; }
    .chart-card__badge--warning { background: rgba(245,158,11,.12); color: #f59e0b; }
    .chart-card__badge--success { background: rgba(16,185,129,.12); color: #10b981; }
    .chart-card__info { font-size: 16px; color: var(--text-secondary); cursor: help; }
    .chart-card__content {
      flex: 1;
      padding: 12px 16px 16px !important;
      display: flex;
      flex-direction: column;
    }
    .chart-card__spinner {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 160px;
    }
    .chart-card__error {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      min-height: 100px;
      justify-content: center;
      color: var(--text-secondary);
      font-size: 13px;
    }
    .chart-card__error mat-icon { color: var(--status-overdue); }
    mat-card-header { padding: 0; }
  `]
})
export class ChartCardComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() icon = '';
  @Input() badge = '';
  @Input() badgeType: 'default' | 'danger' | 'warning' | 'success' = 'default';
  @Input() tooltip = '';
  @Input() loading = false;
  @Input() error = '';
}
