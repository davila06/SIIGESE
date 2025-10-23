import { Injectable } from '@angular/core';
import { formatCurrencyByCode } from '../constants/currency.constants';

export interface ExportOptions {
  filename?: string;
  format: 'csv' | 'excel' | 'pdf';
  includeHeaders?: boolean;
  dateFormat?: string;
}

export interface ExportColumn {
  key: string;
  header: string;
  type?: 'text' | 'number' | 'currency' | 'date' | 'boolean';
  currencyCode?: string;
  dateFormat?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExportService {

  constructor() { }

  /**
   * Exporta datos a CSV
   * @param data Array de datos a exportar
   * @param columns Configuración de columnas
   * @param options Opciones de exportación
   */
  exportToCSV<T>(data: T[], columns: ExportColumn[], options: ExportOptions = { format: 'csv' }): void {
    if (!data || data.length === 0) {
      console.warn('No hay datos para exportar');
      return;
    }

    const csvContent = this.generateCSVContent(data, columns, options);
    const filename = options.filename || `export_${this.getCurrentDateString()}.csv`;
    
    this.downloadFile(csvContent, filename, 'text/csv;charset=utf-8;');
  }

  /**
   * Exporta datos a Excel (formato CSV compatible con Excel)
   * @param data Array de datos a exportar
   * @param columns Configuración de columnas
   * @param options Opciones de exportación
   */
  exportToExcel<T>(data: T[], columns: ExportColumn[], options: ExportOptions = { format: 'excel' }): void {
    if (!data || data.length === 0) {
      console.warn('No hay datos para exportar');
      return;
    }

    const csvContent = this.generateCSVContent(data, columns, { ...options, includeHeaders: true });
    const filename = options.filename || `export_${this.getCurrentDateString()}.xlsx`;
    
    // Para Excel, usamos el BOM UTF-8 para mejor compatibilidad
    const bom = '\uFEFF';
    const csvWithBom = bom + csvContent;
    
    this.downloadFile(csvWithBom, filename, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;');
  }

  /**
   * Genera el reporte en PDF (funcionalidad básica)
   * @param data Array de datos a exportar
   * @param columns Configuración de columnas
   * @param options Opciones de exportación
   */
  exportToPDF<T>(data: T[], columns: ExportColumn[], options: ExportOptions = { format: 'pdf' }): void {
    // Para PDF necesitaríamos una librería como jsPDF
    // Por ahora, exportamos como texto plano que se puede convertir
    const textContent = this.generateTextContent(data, columns, options);
    const filename = options.filename || `export_${this.getCurrentDateString()}.txt`;
    
    this.downloadFile(textContent, filename, 'text/plain;charset=utf-8;');
  }

  /**
   * Genera contenido CSV
   */
  private generateCSVContent<T>(data: T[], columns: ExportColumn[], options: ExportOptions): string {
    const lines: string[] = [];
    
    // Headers
    if (options.includeHeaders !== false) {
      const headers = columns.map(col => this.escapeCSV(col.header));
      lines.push(headers.join(','));
    }

    // Data rows
    for (const item of data) {
      const row = columns.map(col => {
        const value = this.getNestedValue(item, col.key);
        const formattedValue = this.formatValue(value, col);
        return this.escapeCSV(formattedValue);
      });
      lines.push(row.join(','));
    }

    return lines.join('\n');
  }

  /**
   * Genera contenido de texto plano para PDF
   */
  private generateTextContent<T>(data: T[], columns: ExportColumn[], options: ExportOptions): string {
    const lines: string[] = [];
    
    // Título
    lines.push('REPORTE DE EXPORTACIÓN');
    lines.push('='.repeat(50));
    lines.push(`Fecha de generación: ${new Date().toLocaleString('es-CR')}`);
    lines.push(`Total de registros: ${data.length}`);
    lines.push('');

    // Headers
    const headers = columns.map(col => col.header.padEnd(20)).join(' | ');
    lines.push(headers);
    lines.push('-'.repeat(headers.length));

    // Data rows
    for (const item of data) {
      const row = columns.map(col => {
        const value = this.getNestedValue(item, col.key);
        const formattedValue = this.formatValue(value, col);
        return formattedValue.toString().padEnd(20);
      }).join(' | ');
      lines.push(row);
    }

    return lines.join('\n');
  }

  /**
   * Obtiene valor anidado de un objeto usando notación de punto
   */
  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((value, key) => value?.[key], obj);
  }

  /**
   * Formatea un valor según su tipo
   */
  private formatValue(value: any, column: ExportColumn): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (column.type) {
      case 'currency':
        const currencyCode = column.currencyCode || 'CRC';
        return formatCurrencyByCode(Number(value), currencyCode);
      
      case 'date':
        if (value instanceof Date) {
          const format = column.dateFormat || 'dd/MM/yyyy';
          return this.formatDate(value, format);
        }
        return value.toString();
      
      case 'number':
        return Number(value).toLocaleString('es-CR');
      
      case 'boolean':
        return value ? 'Sí' : 'No';
      
      default:
        return value.toString();
    }
  }

  /**
   * Formatea una fecha
   */
  private formatDate(date: Date, format: string): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear().toString();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');

    return format
      .replace('dd', day)
      .replace('MM', month)
      .replace('yyyy', year)
      .replace('HH', hours)
      .replace('mm', minutes);
  }

  /**
   * Escapa caracteres especiales para CSV
   */
  private escapeCSV(value: string): string {
    if (value === null || value === undefined) {
      return '';
    }
    
    const stringValue = value.toString();
    
    // Si contiene comas, comillas o saltos de línea, envolver en comillas
    if (stringValue.includes(',') || stringValue.includes('"') || stringValue.includes('\n')) {
      // Escapar comillas duplicándolas
      const escaped = stringValue.replace(/"/g, '""');
      return `"${escaped}"`;
    }
    
    return stringValue;
  }

  /**
   * Obtiene la fecha actual como string para nombres de archivo
   */
  private getCurrentDateString(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = (now.getMonth() + 1).toString().padStart(2, '0');
    const day = now.getDate().toString().padStart(2, '0');
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    
    return `${year}${month}${day}_${hours}${minutes}`;
  }

  /**
   * Descarga un archivo
   */
  private downloadFile(content: string, filename: string, mimeType: string): void {
    const blob = new Blob([content], { type: mimeType });
    const url = window.URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.style.display = 'none';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    window.URL.revokeObjectURL(url);
  }
}