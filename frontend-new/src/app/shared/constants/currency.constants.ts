// Constantes de monedas para todo el sistema
export const CURRENCY_CONSTANTS = {
  DEFAULT_CURRENCY: 'GTQ',
  SUPPORTED_CURRENCIES: ['CRC', 'USD', 'EUR', 'GTQ'],
  DEFAULT_LOCALE: 'es-GT'
};

export const MONEDAS_SISTEMA = [
  { value: 'GTQ', label: 'Quetzales Guatemaltecos (GTQ)', symbol: 'Q', locale: 'es-GT' },
  { value: 'CRC', label: 'Colones Costarricenses (CRC)', symbol: '₡', locale: 'es-CR' },
  { value: 'USD', label: 'Dólares Americanos (USD)', symbol: '$', locale: 'en-US' },
  { value: 'EUR', label: 'Euros (EUR)', symbol: '€', locale: 'es-ES' }
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