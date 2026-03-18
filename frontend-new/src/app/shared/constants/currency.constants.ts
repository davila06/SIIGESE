// Constantes de monedas para todo el sistema
export const CURRENCY_CONSTANTS = {
  DEFAULT_CURRENCY: 'CRC',
  SUPPORTED_CURRENCIES: ['CRC', 'USD', 'EUR'],
  DEFAULT_LOCALE: 'es-CR'
};

export const MONEDAS_SISTEMA = [
  { value: 'CRC', label: 'Colones Costarricenses (CRC)', symbol: '₡', locale: 'es-CR' },
  { value: 'USD', label: 'Dólares Americanos (USD)', symbol: '$', locale: 'en-US' },
  { value: 'EUR', label: 'Euros (EUR)', symbol: '€', locale: 'es-ES' }
];

export const ASEGURADORAS_SISTEMA = [
  { value: 'INS', label: 'Instituto Nacional de Seguros (INS)' },
  { value: 'SAGICOR', label: 'Sagicor Seguros' },
  { value: 'ASSA', label: 'ASSA Compañía de Seguros' },
  { value: 'BCR_SEGUROS', label: 'BCR Seguros' },
  { value: 'MAPFRE', label: 'MAPFRE Seguros Costa Rica' },
  { value: 'OTROS', label: 'Otras Aseguradoras' }
];

export function formatCurrencyByCode(amount: number, currencyCode: string = 'CRC'): string {
  const currency = MONEDAS_SISTEMA.find(m => m.value === currencyCode);
  if (!currency) {
    return formatCurrencyByCode(amount, CURRENCY_CONSTANTS.DEFAULT_CURRENCY);
  }

  return new Intl.NumberFormat(currency.locale, {
    style: 'currency',
    currency: currency.value
  }).format(amount);
}

/**
 * Parsea una fecha del backend que puede venir en formato DD-MM-YYYY,
 * DD-MM-YYYY HH:mm:ss, o ISO (YYYY-MM-DD...).
 * Retorna null si el valor es falsy o no se puede parsear.
 */
export function parseBackendDate(value: any): Date | null {
  if (!value) return null;
  if (value instanceof Date) {
    return Number.isNaN(value.getTime()) ? null : value;
  }
  return parseBackendDateString(value);
}

function validDate(d: Date): Date | null {
  return Number.isNaN(d.getTime()) ? null : d;
}

function parseBackendDateString(value: string): Date | null {
  // DD-MM-YYYY
  if (/^\d{2}-\d{2}-\d{4}$/.test(value)) {
    const [day, month, year] = value.split('-');
    return validDate(new Date(`${year}-${month}-${day}T00:00:00`));
  }
  // DD-MM-YYYY HH:mm:ss
  if (/^\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}$/.test(value)) {
    const [datePart, timePart] = value.split(' ');
    const [day, month, year] = datePart.split('-');
    return validDate(new Date(`${year}-${month}-${day}T${timePart}`));
  }
  return validDate(new Date(value));
}

/**
 * Formatea una fecha JS a DD-MM-YYYY para enviar al backend.
 */
export function formatToBackendDate(date: Date | string | null | undefined): string | null {
  if (!date) return null;
  const d = parseBackendDate(date);
  if (!d) return null;
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  return `${day}-${month}-${year}`;
}

export function formatDateCR(date: Date | string): string {
  const d = parseBackendDate(date);
  if (!d) return '-';
  return d.toLocaleDateString('es-CR');
}