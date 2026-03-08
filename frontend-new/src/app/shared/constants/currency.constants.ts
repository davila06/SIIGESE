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

export function formatDateCR(date: Date | string): string {
  return new Date(date).toLocaleDateString('es-CR');
}