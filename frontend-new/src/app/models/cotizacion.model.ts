import { MONEDAS_SISTEMA } from '../shared/constants/currency.constants';

export interface CreateCotizacion {
  nombreSolicitante: string;
  nombreAsegurado?: string;
  numeroCedula?: string;
  email: string;
  telefono?: string;
  correo?: string;
  tipoSeguro: string; // AUTO, VIDA, HOGAR, EMPRESARIAL
  modalidad?: string;
  frecuencia?: string;
  aseguradora: string;
  montoAsegurado: number;
  primaCotizada?: number;
  moneda: string;
  fechaVencimiento?: Date;
  observaciones?: string;
  perfilId?: number;

  // Datos específicos para seguros de auto
  placa?: string;
  marca?: string;
  modelo?: string;
  año?: number;
  cilindraje?: string;

  // Datos específicos para seguros de vida
  fechaNacimiento?: Date;
  genero?: string;
  ocupacion?: string;

  // Datos específicos para seguros de hogar
  direccionInmueble?: string;
  tipoInmueble?: string;
  valorInmueble?: number;
}

export interface UpdateCotizacion extends CreateCotizacion {
  estado?: string;
}

export interface Cotizacion {
  id: number;
  numeroCotizacion: string;
  nombreSolicitante: string;
  nombreAsegurado?: string;
  numeroCedula?: string;
  email: string;
  telefono?: string;
  correo?: string;
  tipoSeguro: string;
  modalidad?: string;
  frecuencia?: string;
  aseguradora: string;
  montoAsegurado: number;
  primaCotizada?: number;
  moneda: string;
  fechaCotizacion: Date;
  fechaVencimiento?: Date;
  estado: string; // PENDIENTE, APROBADA, RECHAZADA, CONVERTIDA
  observaciones?: string;
  perfilId?: number;

  // Datos específicos para seguros de auto
  placa?: string;
  marca?: string;
  modelo?: string;
  año?: number;
  cilindraje?: string;

  // Datos específicos para seguros de vida
  fechaNacimiento?: Date;
  genero?: string;
  ocupacion?: string;

  // Datos específicos para seguros de hogar
  direccionInmueble?: string;
  tipoInmueble?: string;
  valorInmueble?: number;

  usuarioId: number;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
}

export interface CotizacionSearch {
  numeroCotizacion?: string;
  nombreSolicitante?: string;
  email?: string;
  tipoSeguro?: string;
  estado?: string;
  fechaDesde?: Date;
  fechaHasta?: Date;
  pageNumber?: number;
  pageSize?: number;
}

export const TIPOS_SEGURO = [
  { value: 'AUTO', label: 'Seguro de Automóvil' },
  { value: 'VIDA', label: 'Seguro de Vida' },
  { value: 'HOGAR', label: 'Seguro de Hogar' },
  { value: 'EMPRESARIAL', label: 'Seguro Empresarial' }
];

export const ESTADOS_COTIZACION = [
  { value: 'PENDIENTE', label: 'Pendiente', color: 'warn' },
  { value: 'APROBADA', label: 'Aprobada', color: 'primary' },
  { value: 'RECHAZADA', label: 'Rechazada', color: 'accent' },
  { value: 'CONVERTIDA', label: 'Convertida a Póliza', color: 'primary' }
];

export const MONEDAS = MONEDAS_SISTEMA;

export const GENEROS = [
  { value: 'MASCULINO', label: 'Masculino' },
  { value: 'FEMENINO', label: 'Femenino' },
  { value: 'OTRO', label: 'Otro' }
];

export const TIPOS_INMUEBLE = [
  { value: 'CASA', label: 'Casa' },
  { value: 'APARTAMENTO', label: 'Apartamento' },
  { value: 'OFICINA', label: 'Oficina' },
  { value: 'LOCAL', label: 'Local Comercial' },
  { value: 'BODEGA', label: 'Bodega' },
  { value: 'OTRO', label: 'Otro' }
];

export const MODALIDADES = [
  { value: 'BASICO', label: 'Básico' },
  { value: 'PLUS', label: 'Plus' },
  { value: 'PREMIUM', label: 'Premium' },
  { value: 'TOTAL', label: 'Total' }
];

export const FRECUENCIAS = [
  { value: 'MENSUAL', label: 'Mensual' },
  { value: 'TRIMESTRAL', label: 'Trimestral' },
  { value: 'SEMESTRAL', label: 'Semestral' },
  { value: 'ANUAL', label: 'Anual' }
];
