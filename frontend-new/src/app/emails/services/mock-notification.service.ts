import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import {
  NotificationResult,
  CobroVencido,
  PolizaVencimiento,
  NotificationStatistics
} from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class MockNotificationService {
  private mockOverduePayments: CobroVencido[] = [
    {
      cobroId: 1,
      numeroPoliza: 'POL-001-2024',
      clienteEmail: 'juan.perez@email.com',
      clienteNombre: 'Juan Pérez García',
      montoVencido: 1250.00,
      fechaVencimiento: new Date('2024-09-15'),
      diasMora: 43,
      concepto: 'Prima mensual - Seguro de Vida'
    },
    {
      cobroId: 2,
      numeroPoliza: 'POL-025-2024',
      clienteEmail: 'maria.rodriguez@email.com',
      clienteNombre: 'María Rodríguez López',
      montoVencido: 850.50,
      fechaVencimiento: new Date('2024-10-01'),
      diasMora: 27,
      concepto: 'Prima mensual - Seguro Vehicular'
    },
    {
      cobroId: 3,
      numeroPoliza: 'POL-042-2024',
      clienteEmail: 'carlos.martinez@email.com',
      clienteNombre: 'Carlos Martínez Sánchez',
      montoVencido: 2100.75,
      fechaVencimiento: new Date('2024-09-30'),
      diasMora: 28,
      concepto: 'Prima anual - Seguro de Hogar'
    },
    {
      cobroId: 4,
      numeroPoliza: 'POL-058-2024',
      clienteEmail: 'ana.gonzalez@email.com',
      clienteNombre: 'Ana González Torres',
      montoVencido: 675.25,
      fechaVencimiento: new Date('2024-10-10'),
      diasMora: 18,
      concepto: 'Prima mensual - Seguro Médico'
    },
    {
      cobroId: 5,
      numeroPoliza: 'POL-071-2024',
      clienteEmail: 'pedro.jimenez@email.com',
      clienteNombre: 'Pedro Jiménez Ruiz',
      montoVencido: 1890.00,
      fechaVencimiento: new Date('2024-08-25'),
      diasMora: 64,
      concepto: 'Prima semestral - Seguro Empresarial'
    }
  ];

  private mockExpiringPolicies: PolizaVencimiento[] = [
    {
      polizaId: 1,
      numeroPoliza: 'POL-103-2024',
      clienteEmail: 'lucia.fernandez@email.com',
      clienteNombre: 'Lucía Fernández Castro',
      fechaVencimiento: new Date('2024-11-15'),
      diasHastaVencimiento: 18,
      tipoPoliza: 'Seguro de Vida',
      montoAsegurado: 250000.00,
      prima: 1450.00
    },
    {
      polizaId: 2,
      numeroPoliza: 'POL-089-2024',
      clienteEmail: 'diego.morales@email.com',
      clienteNombre: 'Diego Morales Vargas',
      fechaVencimiento: new Date('2024-11-28'),
      diasHastaVencimiento: 31,
      tipoPoliza: 'Seguro Vehicular',
      montoAsegurado: 75000.00,
      prima: 890.50
    },
    {
      polizaId: 3,
      numeroPoliza: 'POL-156-2024',
      clienteEmail: 'sofia.herrera@email.com',
      clienteNombre: 'Sofía Herrera Mendoza',
      fechaVencimiento: new Date('2024-12-05'),
      diasHastaVencimiento: 38,
      tipoPoliza: 'Seguro de Hogar',
      montoAsegurado: 180000.00,
      prima: 2250.75
    },
    {
      polizaId: 4,
      numeroPoliza: 'POL-134-2024',
      clienteEmail: 'roberto.silva@email.com',
      clienteNombre: 'Roberto Silva Medina',
      fechaVencimiento: new Date('2024-11-08'),
      diasHastaVencimiento: 11,
      tipoPoliza: 'Seguro Médico',
      montoAsegurado: 50000.00,
      prima: 725.00
    },
    {
      polizaId: 5,
      numeroPoliza: 'POL-178-2024',
      clienteEmail: 'valeria.castro@email.com',
      clienteNombre: 'Valeria Castro Ramos',
      fechaVencimiento: new Date('2024-12-12'),
      diasHastaVencimiento: 45,
      tipoPoliza: 'Seguro Empresarial',
      montoAsegurado: 500000.00,
      prima: 3200.50
    },
    {
      polizaId: 6,
      numeroPoliza: 'POL-192-2024',
      clienteEmail: 'fernando.ortega@email.com',
      clienteNombre: 'Fernando Ortega López',
      fechaVencimiento: new Date('2024-11-20'),
      diasHastaVencimiento: 23,
      tipoPoliza: 'Seguro de Vida',
      montoAsegurado: 300000.00,
      prima: 1675.25
    }
  ];

  processOverduePayments(): Observable<NotificationResult> {
    console.log('🧪 MockNotificationService: Procesando cobros vencidos...');
    return of({
      success: true,
      message: 'Notificaciones de cobros vencidos enviadas exitosamente',
      overduePaymentsSent: this.mockOverduePayments.length,
      overduePaymentsFailed: 0,
      expiringPoliciesSent: 0,
      expiringPoliciesFailed: 0
    }).pipe(delay(1500));
  }

  processExpiringPolicies(daysBeforeExpiration: number = 30): Observable<NotificationResult> {
    console.log('🧪 MockNotificationService: Procesando pólizas por vencer...');
    const validPolicies = this.mockExpiringPolicies.filter(p => p.diasHastaVencimiento <= daysBeforeExpiration);
    return of({
      success: true,
      message: 'Notificaciones de pólizas por vencer enviadas exitosamente',
      overduePaymentsSent: 0,
      overduePaymentsFailed: 0,
      expiringPoliciesSent: validPolicies.length,
      expiringPoliciesFailed: 0
    }).pipe(delay(1500));
  }

  processAllNotifications(daysBeforeExpiration: number = 30): Observable<NotificationResult> {
    console.log('🧪 MockNotificationService: Procesando todas las notificaciones...');
    const validPolicies = this.mockExpiringPolicies.filter(p => p.diasHastaVencimiento <= daysBeforeExpiration);
    return of({
      success: true,
      message: 'Todas las notificaciones automáticas han sido enviadas exitosamente',
      overduePaymentsSent: this.mockOverduePayments.length,
      overduePaymentsFailed: 0,
      expiringPoliciesSent: validPolicies.length,
      expiringPoliciesFailed: 0
    }).pipe(delay(2000));
  }

  getOverduePayments(): Observable<CobroVencido[]> {
    console.log('🧪 MockNotificationService: Obteniendo cobros vencidos...');
    return of([...this.mockOverduePayments]).pipe(delay(1000));
  }

  getExpiringPolicies(daysBeforeExpiration: number = 30): Observable<PolizaVencimiento[]> {
    console.log('🧪 MockNotificationService: Obteniendo pólizas por vencer...');
    const validPolicies = this.mockExpiringPolicies.filter(p => p.diasHastaVencimiento <= daysBeforeExpiration);
    return of([...validPolicies]).pipe(delay(1000));
  }

  getNotificationStatistics(daysBeforeExpiration: number = 30): Observable<NotificationStatistics> {
    console.log('🧪 MockNotificationService: Obteniendo estadísticas...');
    const validPolicies = this.mockExpiringPolicies.filter(p => p.diasHastaVencimiento <= daysBeforeExpiration);
    const totalOverdueAmount = this.mockOverduePayments.reduce((sum, payment) => sum + payment.montoVencido, 0);
    const totalInsuredAmount = validPolicies.reduce((sum, policy) => sum + policy.montoAsegurado, 0);

    return of({
      overduePaymentsCount: this.mockOverduePayments.length,
      expiringPoliciesCount: validPolicies.length,
      totalOverdueAmount: totalOverdueAmount,
      totalInsuredAmount: totalInsuredAmount,
      daysBeforeExpiration: daysBeforeExpiration,
      lastUpdated: new Date()
    }).pipe(delay(800));
  }
}