import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { Poliza, CreatePoliza, DataUploadResult } from '../interfaces/user.interface';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-polizas',
  templateUrl: './polizas.component.html',
  styleUrls: ['./polizas.component.scss']
})
export class PolizasComponent implements OnInit, AfterViewInit {
  @ViewChild('fileInput') fileInput!: ElementRef;
  @ViewChild('formSection') formSection!: ElementRef;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  polizas: Poliza[] = [];
  polizasDataSource = new MatTableDataSource<Poliza>([]);
  polizaForm: FormGroup;
  isLoading = false;
  selectedFile: File | null = null;
  selectedPoliza: Poliza | null = null;
  isEditMode = false;
  
  // Variables de paginación
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50];
  
  // Propiedades para paginación manual de tarjetas
  currentPage = 0;
  paginatedPolizas: Poliza[] = [];
  
  // Variables de búsqueda
  searchTerm: string = '';
  filteredPolizas: Poliza[] = [];
  filteredPolizasCount = 0;
  displayedColumns: string[] = ['numeroPoliza', 'nombreAsegurado', 'prima', 'aseguradora', 'fechaVigencia', 'vehiculo', 'actions'];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {
    this.polizaForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadPolizas();
  }

  // Variables para sorting manual
  currentSortColumn: string = '';
  currentSortDirection: 'asc' | 'desc' | '' = '';

  ngAfterViewInit(): void {
    // Configurar solo paginación (el sorting será manual)
    setTimeout(() => {
      if (this.paginator) {
        this.polizasDataSource.paginator = this.paginator;
      }
      console.log('MatPaginator configurado:', !!this.paginator);
      console.log('DataSource data length:', this.polizasDataSource.data.length);
    }, 0);
  }

  // Método de sorting manual
  sortData(column: string): void {
    console.log('Sorting manual activado:', column);
    
    // Determinar dirección de sorting
    if (this.currentSortColumn === column) {
      // Cambiar dirección: asc -> desc -> none -> asc
      if (this.currentSortDirection === 'asc') {
        this.currentSortDirection = 'desc';
      } else if (this.currentSortDirection === 'desc') {
        this.currentSortDirection = '';
        this.currentSortColumn = '';
      } else {
        this.currentSortDirection = 'asc';
      }
    } else {
      // Nueva columna
      this.currentSortColumn = column;
      this.currentSortDirection = 'asc';
    }

    console.log('Sort column:', this.currentSortColumn);
    console.log('Sort direction:', this.currentSortDirection);

    // Aplicar sorting
    if (this.currentSortDirection === '') {
      // Sin sorting - orden original
      this.filteredPolizas = [...this.polizas];
    } else {
      this.filteredPolizas.sort((a, b) => {
        const valueA = this.getSortValue(a, column);
        const valueB = this.getSortValue(b, column);
        
        let result = 0;
        if (valueA < valueB) {
          result = -1;
        } else if (valueA > valueB) {
          result = 1;
        }
        
        return this.currentSortDirection === 'desc' ? -result : result;
      });
    }

    // Actualizar dataSource
    this.polizasDataSource.data = this.filteredPolizas;
    
    // Actualizar paginación para tarjetas
    this.updatePaginatedPolizas();
    
    console.log('Sorting aplicado, datos ordenados');
  }

  // Obtener valor para sorting
  private getSortValue(poliza: Poliza, column: string): any {
    switch (column) {
      case 'numeroPoliza':
        return (poliza.numeroPoliza || '').toLowerCase();
      case 'nombreAsegurado':
        return (poliza.nombreAsegurado || '').toLowerCase();
      case 'prima':
        return Number(poliza.prima) || 0;
      case 'aseguradora':
        return (poliza.aseguradora || '').toLowerCase();
      case 'fechaVigencia':
        return new Date(poliza.fechaVigencia).getTime();
      case 'vehiculo':
        return (poliza.placa || '').toLowerCase();
      default:
        return '';
    }
  }

  // Obtener clase CSS para la flecha de sorting
  getSortClass(column: string): string {
    if (this.currentSortColumn !== column) return '';
    return this.currentSortDirection === 'asc' ? 'sort-asc' : 'sort-desc';
  }

  // Método específico para manejar sorting
  onSortChange(event: any): void {
    // Ya no necesario con sorting manual, pero mantengo para compatibilidad
    console.log('MatSort event (ignorado):', event);
  }

  createForm(): FormGroup {
    return this.fb.group({
      perfilId: [1, [Validators.required, Validators.min(1)]],
      numeroPoliza: ['', [Validators.required, Validators.maxLength(50)]],
      modalidad: ['', [Validators.required, Validators.maxLength(100)]],
      nombreAsegurado: ['', [Validators.required, Validators.maxLength(200)]],
      prima: [0, [Validators.required, Validators.min(0)]],
      moneda: ['', [Validators.required, Validators.maxLength(10)]],
      fechaVigencia: ['', Validators.required],
      frecuencia: ['', [Validators.required, Validators.maxLength(50)]],
      aseguradora: ['', [Validators.required, Validators.maxLength(100)]],
      placa: ['', Validators.maxLength(20)],
      marca: ['', Validators.maxLength(50)],
      modelo: ['', Validators.maxLength(50)]
    });
  }

  loadPolizas(): void {
    this.isLoading = true;
    this.apiService.getPolizas().subscribe({
      next: (polizas: Poliza[]) => {
        this.polizas = polizas;
        
        // Inicializar datos filtrados
        this.filteredPolizas = [...this.polizas];
        this.filteredPolizasCount = this.filteredPolizas.length;
        
        // Configurar data source para la tabla
        this.polizasDataSource.data = this.filteredPolizas;
        
        // Reconfigurar sorting después de cargar datos
        setTimeout(() => {
          if (this.sort) {
            this.polizasDataSource.sort = this.sort;
            console.log('Datos cargados, sorting reconfigurado');
            console.log('Total pólizas:', this.polizas.length);
            console.log('DataSource data:', this.polizasDataSource.data.length);
          }
        }, 0);
        
        // Configurar paginación manual para tarjetas
        this.updatePaginatedPolizas();
        
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading polizas:', error);
        this.showMessage('Error al cargar las pólizas', 'error');
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.polizaForm.valid) {
      this.isLoading = true;
      const polizaData: CreatePoliza = this.polizaForm.value;

      if (this.isEditMode && this.selectedPoliza) {
        // Actualizar póliza existente
        this.apiService.updatePoliza(this.selectedPoliza.id, polizaData).subscribe({
          next: (updatedPoliza: Poliza) => {
            const index = this.polizas.findIndex(p => p.id === updatedPoliza.id);
            if (index !== -1) {
              this.polizas[index] = updatedPoliza;
            }
            this.resetForm();
            this.showMessage('Póliza actualizada exitosamente');
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error updating poliza:', error);
            this.showMessage('Error al actualizar la póliza', 'error');
            this.isLoading = false;
          }
        });
      } else {
        // Crear nueva póliza
        this.apiService.createPoliza(polizaData).subscribe({
          next: (poliza: Poliza) => {
            this.polizas.push(poliza);
            this.resetForm();
            this.showMessage('Póliza creada exitosamente');
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error creating poliza:', error);
            this.showMessage('Error al crear la póliza', 'error');
            this.isLoading = false;
          }
        });
      }
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') {
      this.selectedFile = file;
    } else {
      this.showMessage('Por favor seleccione un archivo Excel válido (.xlsx)', 'error');
    }
  }

  uploadExcel(): void {
    if (!this.selectedFile) {
      this.showMessage('Por favor seleccione un archivo Excel', 'error');
      return;
    }

    this.isLoading = true;
    const perfilId = this.polizaForm.get('perfilId')?.value || 1;

    this.apiService.uploadExcelPolizas(perfilId, this.selectedFile).subscribe({
      next: (result: DataUploadResult) => {
        this.showMessage(result.message);
        
        // Generar Excel con registros fallidos si hay errores
        if (result.errorRecords > 0 && result.failedRecords && result.failedRecords.length > 0) {
          console.warn('Errores en el procesamiento:', result.errors);
          this.generateFailedRecordsExcel(result.failedRecords, this.selectedFile!.name);
          
          this.showMessage(
            `Se procesaron ${result.processedRecords} registros exitosos y ${result.errorRecords} con errores. ` +
            `Se ha generado un archivo Excel con los registros fallidos.`,
            'warning'
          );
        }
        
        if (result.success && result.processedRecords > 0) {
          this.loadPolizas();
        }
        
        // Limpiar selección de archivo
        this.selectedFile = null;
        if (this.fileInput) {
          this.fileInput.nativeElement.value = '';
        }
        
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error uploading Excel:', error);
        this.showMessage('Error al procesar el archivo Excel', 'error');
        this.isLoading = false;
      }
    });
  }

  deletePoliza(poliza: Poliza): void {
    if (confirm(`¿Está seguro de eliminar la póliza ${poliza.numeroPoliza}?`)) {
      this.apiService.deletePoliza(poliza.id).subscribe({
        next: () => {
          // Actualizar todas las listas
          this.polizas = this.polizas.filter(p => p.id !== poliza.id);
          
          // Actualizar búsqueda y paginación
          this.performSearch();
          
          // Si estaba editando esta póliza, cancelar edición
          if (this.selectedPoliza?.id === poliza.id) {
            this.resetForm();
          }
          
          this.showMessage('Póliza eliminada exitosamente');
        },
        error: (error: any) => {
          console.error('Error deleting poliza:', error);
          this.showMessage('Error al eliminar la póliza', 'error');
        }
      });
    }
  }

  private showMessage(message: string, type: 'success' | 'error' | 'warning' = 'success'): void {
    const panelClass = type === 'error' ? ['error-snackbar'] : 
                      type === 'warning' ? ['warning-snackbar'] : 
                      ['success-snackbar'];
                      
    this.snackBar.open(message, 'Cerrar', {
      duration: type === 'warning' ? 6000 : 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: panelClass
    });
  }

  private generateFailedRecordsExcel(failedRecords: any[], originalFileName: string): void {
    try {
      // Crear datos para el Excel
      const excelData = failedRecords.map(record => {
        const row: any = {
          'Fila': record.rowNumber,
          'Error': record.error
        };
        
        // Agregar datos originales
        if (record.originalData) {
          Object.keys(record.originalData).forEach(key => {
            row[key] = record.originalData[key];
          });
        }
        
        return row;
      });

      // Crear workbook
      const worksheet = XLSX.utils.json_to_sheet(excelData);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Registros Fallidos');

      // Ajustar anchos de columnas
      const columnWidths = Object.keys(excelData[0] || {}).map(key => {
        const maxLength = Math.max(
          key.length,
          ...excelData.map(row => String(row[key] || '').length)
        );
        return { wch: Math.min(Math.max(maxLength + 2, 10), 50) };
      });
      worksheet['!cols'] = columnWidths;

      // Generar archivo y descargar
      const fileName = `Errores_${originalFileName.replace('.xlsx', '')}_${new Date().toISOString().split('T')[0]}.xlsx`;
      XLSX.writeFile(workbook, fileName);
      
      console.log(`Excel de errores generado: ${fileName}`);
    } catch (error) {
      console.error('Error generando Excel de errores:', error);
      this.showMessage('Error al generar el archivo Excel con los errores', 'error');
    }
  }

  testFailedRecordsExcel(): void {
    // Crear datos de prueba para simular registros fallidos
    const mockFailedRecords = [
      {
        rowNumber: 2,
        error: "Campos obligatorios vacíos (POLIZA, NOMBRE, ASEGURADORA)",
        originalData: {
          "POLIZA": "",
          "MOD": "AUTO",
          "NOMBRE": "Juan Pérez",
          "PRIMA": "150000",
          "MONEDA": "COP",
          "FECHA": "2024-12-31",
          "FRECUENCIA": "Mensual",
          "ASEGURADORA": "",
          "PLACA": "ABC123",
          "MARCA": "Toyota",
          "MODELO": "Corolla"
        }
      },
      {
        rowNumber: 5,
        error: "Póliza ya existe (Número: POL001)",
        originalData: {
          "POLIZA": "POL001",
          "MOD": "VIDA",
          "NOMBRE": "María García",
          "PRIMA": "200000",
          "MONEDA": "USD",
          "FECHA": "2024-11-30",
          "FRECUENCIA": "Anual",
          "ASEGURADORA": "Seguros XYZ",
          "PLACA": "",
          "MARCA": "",
          "MODELO": ""
        }
      },
      {
        rowNumber: 8,
        error: "Faltan columnas. Se requieren al menos 8 columnas",
        originalData: {
          "POLIZA": "POL123",
          "MOD": "HOGAR",
          "NOMBRE": "Carlos López",
          "PRIMA": "75000",
          "MONEDA": "COP"
        }
      }
    ];

    this.generateFailedRecordsExcel(mockFailedRecords, "archivo_prueba.xlsx");
    this.showMessage("Excel de prueba con errores generado exitosamente", "success");
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(amount);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('es-CO');
  }

  // Métodos para paginación de tarjetas
  updatePaginatedPolizas(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedPolizas = this.filteredPolizas.slice(startIndex, endIndex);
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.updatePaginatedPolizas();
  }

  get totalPolizas(): number {
    return this.filteredPolizas.length;
  }

  // Métodos de búsqueda
  onSearch(event: any): void {
    const searchValue = event.target.value;
    this.searchTerm = searchValue;
    this.performSearch();
  }

  performSearch(): void {
    if (!this.searchTerm || this.searchTerm.trim() === '') {
      // Si no hay término de búsqueda, mostrar todas las pólizas
      this.filteredPolizas = [...this.polizas];
    } else {
      const searchLower = this.searchTerm.toLowerCase().trim();
      this.filteredPolizas = this.polizas.filter(poliza => {
        // Buscar en número de póliza
        const numeroMatch = poliza.numeroPoliza?.toLowerCase().includes(searchLower);
        
        // Buscar en nombre del asegurado
        const nombreMatch = poliza.nombreAsegurado?.toLowerCase().includes(searchLower);
        
        // Buscar en placa (si existe)
        const placaMatch = poliza.placa?.toLowerCase().includes(searchLower);
        
        return numeroMatch || nombreMatch || placaMatch;
      });
    }
    
    // Aplicar sorting si hay uno activo
    if (this.currentSortColumn && this.currentSortDirection) {
      this.sortData(this.currentSortColumn);
      return; // sortData ya actualiza todo lo necesario
    }
    
    // Actualizar contador
    this.filteredPolizasCount = this.filteredPolizas.length;
    
    // Reiniciar paginación
    this.currentPage = 0;
    if (this.paginator) {
      this.paginator.firstPage();
    }
    
    // Actualizar data source de la tabla
    this.polizasDataSource.data = this.filteredPolizas;
    
    // Actualizar datos paginados para tarjetas
    this.updatePaginatedPolizas();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.performSearch();
  }

  trackByPoliza(index: number, poliza: Poliza): number {
    return poliza.id;
  }

  selectPoliza(poliza: Poliza): void {
    this.selectedPoliza = poliza;
    this.isEditMode = true;
    this.loadPolizaToForm(poliza);
    
    // Scroll inmediato al top de la página
    window.scrollTo({
      top: 0,
      left: 0,
      behavior: 'smooth'
    });
    
    // Efecto visual opcional con delay
    setTimeout(() => {
      if (this.formSection?.nativeElement) {
        this.formSection.nativeElement.classList.add('editing-highlight');
        setTimeout(() => {
          this.formSection.nativeElement.classList.remove('editing-highlight');
        }, 2000);
      }
    }, 500);
  }

  selectPolizaAndScroll(poliza: Poliza): void {
    // Configurar datos primero
    this.selectedPoliza = poliza;
    this.isEditMode = true;
    this.loadPolizaToForm(poliza);
    
    console.log('Seleccionando póliza y preparando scroll:', poliza.numeroPoliza);
    
    // Forzar detección de cambios para que Angular actualice el DOM completamente
    this.cdr.detectChanges();
    
    // Usar la misma lógica exitosa del forceScrollToTop con delay
    setTimeout(() => {
      console.log('Ejecutando scroll con lógica exitosa...');
      
      const scrollElements = [document.documentElement, document.body, window];
      
      scrollElements.forEach(element => {
        if (element === window) {
          element.scrollTo(0, 0);
        } else {
          (element as HTMLElement).scrollTop = 0;
        }
      });
      
      // También intentar con todos los elementos scrollables
      const scrollableElements = document.querySelectorAll('*');
      scrollableElements.forEach(el => {
        if (el.scrollTop > 0) {
          el.scrollTop = 0;
        }
      });
      
      console.log('Scroll completado, posición:', window.scrollY);
      
    }, 150); // Delay un poco más largo para Edge
    
    // Efecto visual después del scroll
    setTimeout(() => {
      if (this.formSection?.nativeElement) {
        this.formSection.nativeElement.classList.add('editing-highlight');
        setTimeout(() => {
          this.formSection.nativeElement.classList.remove('editing-highlight');
        }, 1500);
      }
    }, 400);
  }

  // Método de prueba para forzar scroll
  forceScrollToTop(): void {
    console.log('Forzando scroll al top...');
    const scrollElements = [document.documentElement, document.body, window];
    
    scrollElements.forEach(element => {
      if (element === window) {
        element.scrollTo(0, 0);
      } else {
        (element as HTMLElement).scrollTop = 0;
      }
    });
    
    // También intentar con todos los elementos scrollables
    const scrollableElements = document.querySelectorAll('*');
    scrollableElements.forEach(el => {
      if (el.scrollTop > 0) {
        el.scrollTop = 0;
      }
    });
  }

  loadPolizaToForm(poliza: Poliza): void {
    // Formatear la fecha para el input tipo date
    const fechaVigencia = new Date(poliza.fechaVigencia).toISOString().split('T')[0];
    
    this.polizaForm.patchValue({
      perfilId: poliza.perfilId || 1,
      numeroPoliza: poliza.numeroPoliza,
      modalidad: poliza.modalidad,
      nombreAsegurado: poliza.nombreAsegurado,
      prima: poliza.prima,
      moneda: poliza.moneda,
      fechaVigencia: fechaVigencia,
      frecuencia: poliza.frecuencia,
      aseguradora: poliza.aseguradora,
      placa: poliza.placa || '',
      marca: poliza.marca || '',
      modelo: poliza.modelo || ''
    });
    
    // Marcar el formulario como pristine y untouched después de cargar los datos
    this.polizaForm.markAsPristine();
    this.polizaForm.markAsUntouched();
  }

  resetForm(): void {
    this.polizaForm.reset();
    this.selectedPoliza = null;
    this.isEditMode = false;
    
    // Restablecer valores por defecto
    this.polizaForm.patchValue({
      perfilId: 1
    });
    
    // Limpiar completamente los estados de validación
    Object.keys(this.polizaForm.controls).forEach(key => {
      const control = this.polizaForm.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        control.setErrors(null);
      }
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  get formTitle(): string {
    return this.isEditMode ? 'Editar Póliza' : 'Agregar Nueva Póliza';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Actualizar Póliza' : 'Guardar Póliza';
  }

  canUploadExcel(): boolean {
    return this.authService.hasAnyRole(['Admin', 'DataLoader']);
  }
}