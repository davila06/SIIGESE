import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CobrosService } from '../../services/cobros.service';
import { getEstadoCobroLabel, EstadoCobro } from '../../interfaces/cobro.interface';

@Component({
  selector: 'app-debug-test',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="padding: 20px; background: #f5f5f5; margin: 10px;">
      <h3>DEBUG TEST COMPONENT</h3>
      <div *ngIf="loading">Cargando...</div>
      <div *ngIf="!loading">
        <h4>Datos recibidos del API:</h4>
        <div *ngFor="let cobro of cobros; let i = index" style="border: 1px solid #ccc; margin: 10px 0; padding: 10px;">
          <strong>Cobro {{i+1}}:</strong><br>
          - Número: {{cobro.numeroRecibo}}<br>
          - Cliente: {{cobro.clienteNombre}} {{cobro.clienteApellido}}<br>
          - Estado (raw): {{cobro.estado}} ({{getType(cobro.estado)}})<br>
          - Estado (label): {{getEstadoLabel(cobro.estado)}}<br>
          - Estado comparación con 0: {{cobro.estado === 0}}<br>
          - Estado comparación con EstadoCobro.Pendiente: {{cobro.estado === EstadoCobro.Pendiente}}<br>
        </div>
        
        <h4>Enum Values:</h4>
        <div>
          - EstadoCobro.Pendiente = {{EstadoCobro.Pendiente}}<br>
          - EstadoCobro.Cobrado = {{EstadoCobro.Cobrado}}<br>
          - EstadoCobro.Vencido = {{EstadoCobro.Vencido}}<br>
          - EstadoCobro.Cancelado = {{EstadoCobro.Cancelado}}<br>
        </div>
      </div>
    </div>
  `
})
export class DebugTestComponent implements OnInit {
  cobros: any[] = [];
  loading = true;
  EstadoCobro = EstadoCobro;

  constructor(private cobrosService: CobrosService) {}

  ngOnInit() {
    this.cobrosService.getCobros().subscribe({
      next: (data) => {
        console.log('Raw data from API:', data);
        this.cobros = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error:', err);
        this.loading = false;
      }
    });
  }

  getType(value: any): string {
    return typeof value;
  }

  getEstadoLabel(estado: any): string {
    return getEstadoCobroLabel(estado);
  }
}