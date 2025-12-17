import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Poliza {
  id: number;
  numeroPoliza: string;
  nombreAsegurado: string;
  numeroCedula: string;
  prima: number;
  moneda: string;
  fechaVigencia: Date;
  frecuencia: string;
  aseguradora: string;
  placa?: string;
  marca?: string;
  modelo?: string;
  año?: string;
  correo?: string;
  numeroTelefono?: string;
  esActivo: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PolizasService {
  private apiUrl = `${environment.apiUrl}/polizas`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<Poliza[]> {
    return this.http.get<Poliza[]>(this.apiUrl);
  }

  getById(id: number): Observable<Poliza> {
    return this.http.get<Poliza>(`${this.apiUrl}/${id}`);
  }

  getByNumeroPoliza(numeroPoliza: string): Observable<Poliza> {
    return this.http.get<Poliza>(`${this.apiUrl}/numero/${numeroPoliza}`);
  }

  buscarPolizas(termino: string): Observable<Poliza[]> {
    return this.http.get<Poliza[]>(`${this.apiUrl}/buscar?termino=${encodeURIComponent(termino)}`);
  }
}
