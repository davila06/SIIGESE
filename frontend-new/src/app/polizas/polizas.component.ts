import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { Poliza, CreatePoliza } from '../interfaces/user.interface';
import { formatCurrencyByCode, formatDateCR, MONEDAS_SISTEMA, CURRENCY_CONSTANTS, ASEGURADORAS_SISTEMA } from '../shared/constants/currency.constants';

@Component({
  selector: 'app-polizas',
  templateUrl: './polizas.component.html',
  styleUrls: ['./polizas.component.scss'],
  standalone: false
})
export class PolizasComponent implements OnInit, AfterViewInit {
  @ViewChild('formSection') formSection!: ElementRef;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  polizas: Poliza[] = [];
  polizasDataSource = new MatTableDataSource<Poliza>([]);
  polizaForm: FormGroup;
  isLoading = false;
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
  displayedColumns: string[] = ['numeroPoliza', 'nombreAsegurado', 'aseguradora', 'fechaVigencia', 'frecuencia', 'observaciones', 'actions'];

  // Datos para selectores
  monedasSistema = MONEDAS_SISTEMA;
  aseguradorasSistema = ASEGURADORAS_SISTEMA;

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    public authService: AuthService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {
    this.polizaForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadPolizas();
    // Asegurar que el formulario esté siempre limpio al iniciar
    this.resetForm();
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

    // Debug del formulario en tiempo real
    if (this.polizaForm) {
      this.polizaForm.valueChanges.subscribe(value => {
        console.log('📝 Formulario cambió:', {
          valid: this.polizaForm.valid,
          invalid: this.polizaForm.invalid,
          validForSubmission: this.isFormValidForSubmission(),
          value: value
        });
      });

      this.polizaForm.statusChanges.subscribe(status => {
        console.log('🔄 Estado del formulario cambió:', status);
        console.log('📊 Validación para envío:', this.isFormValidForSubmission());
      });
    }
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
      nombreAsegurado: ['', [Validators.required, Validators.maxLength(200)]],
      prima: [0, [Validators.required, Validators.min(0)]],
      moneda: [CURRENCY_CONSTANTS.DEFAULT_CURRENCY, [Validators.required, Validators.maxLength(10)]],
      fechaVigencia: ['', Validators.required],
      frecuencia: ['', [Validators.required, Validators.maxLength(50)]],
      aseguradora: ['', [Validators.required, Validators.maxLength(100)]],
      placa: ['', Validators.maxLength(20)],
      marca: ['', Validators.maxLength(50)],
      modelo: ['', Validators.maxLength(50)],
      observaciones: ['', Validators.maxLength(500)]
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
            
            // Actualizar todas las listas filtradas y vistas
            this.performSearch();
            
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
            
            // Actualizar todas las listas filtradas y vistas
            this.performSearch();
            
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

  formatCurrency(amount: number, currency: string = CURRENCY_CONSTANTS.DEFAULT_CURRENCY): string {
    return formatCurrencyByCode(amount, currency);
  }

  formatDate(date: Date | string): string {
    if (!date) return '-';
    
    try {
      const dateObj = typeof date === 'string' ? new Date(date) : date;
      
      // Verificar si la fecha es válida
      if (isNaN(dateObj.getTime())) {
        return '-';
      }
      
      return formatDateCR(dateObj);
    } catch (error) {
      console.error('Error formateando fecha:', error, date);
      return '-';
    }
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
      const searchLower = this.normalizeText(this.searchTerm.toLowerCase().trim());
      this.filteredPolizas = this.polizas.filter(poliza => {
        // Buscar en número de póliza
        const numeroMatch = this.normalizeText(poliza.numeroPoliza?.toLowerCase() || '').includes(searchLower);
        
        // Búsqueda mejorada en nombre del asegurado
        const nombreMatch = this.searchInName(poliza.nombreAsegurado || '', searchLower);
        
        // Buscar en placa (si existe)
        const placaMatch = this.normalizeText(poliza.placa?.toLowerCase() || '').includes(searchLower);
        
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

  /**
   * Normaliza texto removiendo acentos y caracteres especiales
   */
  private normalizeText(text: string): string {
    return text
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '') // Remover acentos
      .replace(/[^\w\s]/g, '') // Remover caracteres especiales excepto espacios
      .trim();
  }

  /**
   * Búsqueda inteligente en nombres - permite buscar palabras en cualquier orden
   */
  private searchInName(nombre: string, searchTerm: string): boolean {
    const normalizedName = this.normalizeText(nombre.toLowerCase());
    const normalizedSearch = searchTerm;
    
    // Búsqueda directa (búsqueda completa)
    if (normalizedName.includes(normalizedSearch)) {
      return true;
    }
    
    // Búsqueda por palabras separadas (permite buscar "perez juan" para encontrar "Juan Pérez")
    const searchWords = normalizedSearch.split(/\s+/).filter(word => word.length > 0);
    const nameWords = normalizedName.split(/\s+/).filter(word => word.length > 0);
    
    // Verificar que todas las palabras de búsqueda estén en el nombre
    return searchWords.every(searchWord => 
      nameWords.some(nameWord => 
        nameWord.includes(searchWord) || searchWord.includes(nameWord)
      )
    );
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
    console.log('🔄 Iniciando edición de póliza:', poliza.numeroPoliza);
    
    // Configurar datos primero
    this.selectedPoliza = poliza;
    this.isEditMode = true;
    this.loadPolizaToForm(poliza);
    
    console.log('📝 Datos cargados en formulario:', {
      numeroPoliza: poliza.numeroPoliza,
      nombreAsegurado: poliza.nombreAsegurado,
      prima: poliza.prima,
      isEditMode: this.isEditMode
    });
    
    // Forzar detección de cambios para que Angular actualice el DOM completamente
    this.cdr.detectChanges();
    
    // Scroll suave al formulario con múltiples métodos para compatibilidad
    setTimeout(() => {
      console.log('🎯 Ejecutando scroll al formulario...');
      
      // Método 1: Scroll nativo del browser
      window.scrollTo({
        top: 0,
        left: 0,
        behavior: 'smooth'
      });
      
      // Método 2: Backup para navegadores que no soportan behavior smooth
      if (window.scrollY > 0) {
        document.documentElement.scrollTop = 0;
        document.body.scrollTop = 0;
      }
      
      // Método 3: Scroll a elemento específico si existe
      if (this.formSection?.nativeElement) {
        this.formSection.nativeElement.scrollIntoView({
          behavior: 'smooth',
          block: 'start'
        });
      }
      
      console.log('✅ Scroll completado, posición final:', window.scrollY);
      
    }, 100);
    
    // Efecto visual de confirmación
    setTimeout(() => {
      if (this.formSection?.nativeElement) {
        console.log('✨ Aplicando efecto visual de edición...');
        this.formSection.nativeElement.classList.add('editing-highlight');
        
        // Mostrar mensaje de confirmación
        this.showMessage(`📝 Editando póliza: ${poliza.numeroPoliza}`);
        
        setTimeout(() => {
          this.formSection.nativeElement.classList.remove('editing-highlight');
        }, 2000);
      }
    }, 500);
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
    console.log('📋 Cargando datos de póliza al formulario:', poliza);
    
    // Formatear la fecha para el input tipo date
    const fechaVigencia = new Date(poliza.fechaVigencia).toISOString().split('T')[0];
    
    // Preparar los valores para el formulario
    const formValues = {
      perfilId: poliza.perfilId || 1,
      numeroPoliza: poliza.numeroPoliza,
      nombreAsegurado: poliza.nombreAsegurado,
      prima: poliza.prima,
      moneda: poliza.moneda,
      fechaVigencia: fechaVigencia,
      frecuencia: poliza.frecuencia,
      aseguradora: poliza.aseguradora,
      placa: poliza.placa || '',
      marca: poliza.marca || '',
      modelo: poliza.modelo || '',
      observaciones: poliza.observaciones || ''
    };
    
    console.log('💾 Valores a cargar en formulario:', formValues);
    
    // Cargar los valores al formulario
    this.polizaForm.patchValue(formValues);
    
    // Marcar el formulario como pristine y untouched después de cargar los datos
    this.polizaForm.markAsPristine();
    this.polizaForm.markAsUntouched();
    
    // Verificar que los valores se cargaron correctamente
    setTimeout(() => {
      console.log('✅ Verificación del formulario cargado:', {
        formValid: this.polizaForm.valid,
        formValue: this.polizaForm.value,
        isEditMode: this.isEditMode,
        selectedPoliza: this.selectedPoliza?.numeroPoliza
      });
    }, 50);
  }

  resetForm(): void {
    // Resetear completamente el formulario
    this.polizaForm.reset();
    this.selectedPoliza = null;
    this.isEditMode = false;
    
    // Restablecer valores por defecto
    this.polizaForm.patchValue({
      perfilId: 1,
      moneda: CURRENCY_CONSTANTS.DEFAULT_CURRENCY,
      prima: 0,
      fechaVigencia: new Date().toISOString().split('T')[0] // Fecha actual por defecto
    });
    
    // Limpiar completamente los estados de validación
    Object.keys(this.polizaForm.controls).forEach(key => {
      const control = this.polizaForm.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    });
    
    // Forzar actualización de la vista
    this.cdr.detectChanges();
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

  // Método para validar si el formulario es válido para envío
  isFormValidForSubmission(): boolean {
    const requiredFields = ['numeroPoliza', 'nombreAsegurado', 
                           'prima', 'fechaVigencia', 'frecuencia', 'aseguradora'];
    
    const isValid = requiredFields.every(field => {
      const control = this.polizaForm.get(field);
      return control && control.valid && control.value !== '' && control.value !== null;
    });

    // Debug logging
    if (!isValid) {
      console.log('🔍 Formulario inválido. Campos con problemas:');
      requiredFields.forEach(field => {
        const control = this.polizaForm.get(field);
        if (!control || !control.valid || control.value === '' || control.value === null) {
          console.log(`❌ ${field}:`, {
            exists: !!control,
            valid: control?.valid,
            value: control?.value,
            errors: control?.errors
          });
        }
      });
    }

    return isValid;
  }

}