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

// Constantes de aseguradoras para todo el sistema
export const ASEGURADORAS_SISTEMA = [
  { value: 'Instituto Nacional de Seguros (INS)', label: 'Instituto Nacional de Seguros (INS)' },
  { value: 'ASSA Compañía de Seguros S.A.', label: 'ASSA Compañía de Seguros S.A.' },
  { value: 'Pan-American Life Insurance de Costa Rica, S.A. (PALIG)', label: 'Pan-American Life Insurance de Costa Rica, S.A. (PALIG)' },
  { value: 'Davivienda Seguros (Costa Rica)', label: 'Davivienda Seguros (Costa Rica)' },
  { value: 'MNK Seguros Compañía Aseguradora', label: 'MNK Seguros Compañía Aseguradora' },
  { value: 'Aseguradora del Istmo (ADISA)', label: 'Aseguradora del Istmo (ADISA)' }
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
  const d = new Date(date);
  const day = d.getDate().toString().padStart(2, '0');
  const month = (d.getMonth() + 1).toString().padStart(2, '0');
  const year = d.getFullYear();
  return `${day}-${month}-${year}`;
}