import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class MockApiService {
  private users = [
    {
      id: 1,
      email: 'admin@sinseg.com',
      password: 'password123',
      role: 'admin',
      name: 'Administrador SINSEG'
    },
    {
      id: 2,
      email: 'user@sinseg.com',
      password: 'password123',
      role: 'user',
      name: 'Usuario SINSEG'
    }
  ];

  private polizas = [
    {
      id: 1,
      numeroPoliza: "POL-001-2024",
      cliente: "Juan Pérez",
      tipoSeguro: "Vida",
      prima: 1500.00,
      fechaInicio: "2024-01-15T00:00:00Z",
      fechaVencimiento: "2025-01-15T00:00:00Z",
      estado: "Activa",
      montoAsegurado: 100000.00
    },
    {
      id: 2,
      numeroPoliza: "POL-002-2024",
      cliente: "María García",
      tipoSeguro: "Auto",
      prima: 2500.00,
      fechaInicio: "2024-02-01T00:00:00Z",
      fechaVencimiento: "2025-02-01T00:00:00Z",
      estado: "Activa",
      montoAsegurado: 50000.00
    },
    {
      id: 3,
      numeroPoliza: "POL-003-2024",
      cliente: "Carlos López",
      tipoSeguro: "Hogar",
      prima: 800.00,
      fechaInicio: "2024-03-10T00:00:00Z",
      fechaVencimiento: "2025-03-10T00:00:00Z",
      estado: "Activa",
      montoAsegurado: 75000.00
    }
  ];

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<any> {
    // Simulate API call delay
    return of(null).pipe(
      delay(1000),
      map(() => {
        const user = this.users.find(u => 
          u.email.toLowerCase() === email.toLowerCase() && 
          u.password === password
        );

        if (user) {
          const token = this.generateToken(user);
          return {
            success: true,
            token: `Bearer ${token}`,
            user: {
              id: user.id,
              email: user.email,
              role: user.role,
              name: user.name
            }
          };
        } else {
          throw new Error('Credenciales inválidas');
        }
      })
    );
  }

  getPolizas(): Observable<any> {
    return of({
      success: true,
      data: this.polizas,
      total: this.polizas.length
    }).pipe(delay(500));
  }

  private generateToken(user: any): string {
    const payload = {
      userId: user.id,
      email: user.email,
      role: user.role,
      exp: Date.now() + (24 * 60 * 60 * 1000) // 24 hours
    };
    return btoa(JSON.stringify(payload));
  }
}