import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, ChangeDetectorRef, inject, HostListener } from '@angular/core';
import { MatTabGroup } from '@angular/material/tabs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { PolizaDetailDialogComponent } from './poliza-detail-dialog.component';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { Poliza, CreatePoliza } from '../interfaces/user.interface';
import { ConfirmDialogComponent, ConfirmDialogData } from '../shared/components/confirm-dialog.component';
import { formatCurrencyByCode, formatDateCR, parseBackendDate, MONEDAS_SISTEMA, CURRENCY_CONSTANTS, ASEGURADORAS_SISTEMA } from '../shared/constants/currency.constants';
import { LoggingService } from '../services/logging.service';

@Component({
  selector: 'app-polizas',
  templateUrl: './polizas.component.html',
  styleUrls: ['./polizas.component.scss'],
  standalone: false
})
export class PolizasComponent implements OnInit, AfterViewInit {
  @ViewChild('formSection') formSection!: ElementRef;
  @ViewChild(MatTabGroup) tabGroup!: MatTabGroup;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  polizas: Poliza[] = [];
  polizasDataSource = new MatTableDataSource<Poliza>([]);
  polizaForm: FormGroup;
  isLoading = false;
  selectedPoliza: Poliza | null = null;
  isEditMode = false;
  formLoadingState: 'idle' | 'loading' | 'loaded' = 'idle';

  // Vista activa (tarjetas / tabla), persiste en localStorage
  currentView: 'cards' | 'table' = 'cards';
  
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

  get availableAseguradoras(): Array<{ value: string; label: string }> {
    const currentValue = this.polizaForm.get('aseguradora')?.value;
    if (!currentValue || this.aseguradorasSistema.some(a => a.value === currentValue)) {
      return this.aseguradorasSistema;
    }

    return [
      ...this.aseguradorasSistema,
      { value: currentValue, label: currentValue }
    ];
  }

  private readonly logger = inject(LoggingService);

  constructor(
    private readonly fb: FormBuilder,
    private readonly apiService: ApiService,
    public readonly authService: AuthService,
    private readonly snackBar: MatSnackBar,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog
  ) {
    this.polizaForm = this.createForm();
    // Restaurar preferencia de vista almacenada
    const savedView = localStorage.getItem('polizasView');
    if (savedView === 'table' || savedView === 'cards') {
      this.currentView = savedView;
    }
  }

  ngOnInit(): void {
    this.loadPolizas();
    // Asegurar que el formulario esté siempre limpio al iniciar
    this.resetForm();
  }

  /** Tecla Escape: cancela edición */
  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isEditMode) {
      this.resetForm();
    }
  }

  /** Navega al tab del formulario para crear nueva póliza */
  goToNewForm(): void {
    this.resetFormValues();
    setTimeout(() => {
      if (this.tabGroup) this.tabGroup.selectedIndex = 1;
    }, 0);
  }

  /** Maneja cambio de tab — si vuelve al listado cancelando edición */
  onTabChange(index: number): void {
    if (index === 0 && this.isEditMode) {
      this.resetFormValues();
    }
  }

  /** Cambia vista tarjetas/tabla y persiste en localStorage */
  onViewChange(view: 'cards' | 'table'): void {
    this.currentView = view;
    localStorage.setItem('polizasView', view);
  }

  // Variables para sorting manual
  currentSortColumn: string = '';
  currentSortDirection: 'asc' | 'desc' | '' = '';

  ngAfterViewInit(): void {
    setTimeout(() => {
      if (this.paginator) {
        this.polizasDataSource.paginator = this.paginator;
      }
    }, 0);
  }

  // Método de sorting manual
  sortData(column: string): void {
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
        return (parseBackendDate(poliza.fechaVigencia) ?? new Date(0)).getTime();
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
  onSortChange(_event: any): void { }

  createForm(): FormGroup {
    return this.fb.group({
      perfilId: [1, [Validators.required, Validators.min(1)]],
      numeroPoliza: ['', [Validators.required, Validators.maxLength(50)]],
      modalidad: ['', Validators.maxLength(50)],
      nombreAsegurado: ['', [Validators.required, Validators.maxLength(200)]],
      numeroCedula: ['', Validators.maxLength(50)],
      prima: [0, [Validators.required, Validators.min(0)]],
      moneda: [CURRENCY_CONSTANTS.DEFAULT_CURRENCY, [Validators.required, Validators.maxLength(10)]],
      fechaVigencia: ['', Validators.required],
      frecuencia: ['', [Validators.required, Validators.maxLength(50)]],
      aseguradora: ['', [Validators.required, Validators.maxLength(100)]],
      placa: ['', Validators.maxLength(20)],
      marca: ['', Validators.maxLength(50)],
      modelo: ['', Validators.maxLength(50)],
      año: ['', Validators.maxLength(4)],
      correo: ['', [Validators.email, Validators.maxLength(100)]],
      numeroTelefono: ['', Validators.maxLength(20)],
      observaciones: ['', Validators.maxLength(500)]
    });
  }

  loadPolizas(): void {
    this.isLoading = true;
    this.apiService.getPolizas().subscribe({
      next: (polizas: Poliza[]) => {
        if (polizas.length > 0) {
          console.table(polizas.map(p => ({
            id: p.id,
            numeroPoliza: p.numeroPoliza,
            nombreAsegurado: p.nombreAsegurado,
            aseguradora: p.aseguradora,
            frecuencia: p.frecuencia,
            prima: p.prima,
            moneda: p.moneda,
            fechaVigencia: p.fechaVigencia
          })));
        }
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
          }
        }, 0);
        
        // Configurar paginación manual para tarjetas
        this.updatePaginatedPolizas();
        
        this.isLoading = false;
      },
      error: (error: any) => {
        this.logger.error('Error loading polizas:', error);
        this.showMessage('Error al cargar las pólizas', 'error');
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    const requiredFields = ['numeroPoliza', 'nombreAsegurado', 'prima', 'fechaVigencia', 'frecuencia', 'aseguradora'];
    const invalidRequired = requiredFields.filter(key => {
      const control = this.polizaForm.get(key);
      return !control || !control.value || control.value === '';
    });
    if (invalidRequired.length > 0) {
      this.polizaForm.markAllAsTouched();
      this.logger.warn('Campos requeridos vacíos:', invalidRequired.join(', '));
      this.showMessage('Por favor, complete todos los campos requeridos', 'warning');
      return;
    }
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

          this.showMessage('Póliza actualizada exitosamente');
          this.isLoading = false;
          this.resetForm();
        },
        error: (error: any) => {
          this.logger.error('Error updating poliza:', error);
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

          this.showMessage('Póliza creada exitosamente');
          this.isLoading = false;
          this.resetForm();
        },
        error: (error: any) => {
          this.logger.error('Error creating poliza:', error);
          this.showMessage('Error al crear la póliza', 'error');
          this.isLoading = false;
        }
      });
    }
  }

  deletePoliza(poliza: Poliza): void {
    const data: ConfirmDialogData = {
      title: 'Eliminar Póliza',
      message: `¿Está seguro de eliminar la póliza ${poliza.numeroPoliza}?`,
      detail: `Asegurado: ${poliza.nombreAsegurado}. Esta acción no se puede deshacer.`,
      confirmText: 'Eliminar',
      confirmColor: 'warn',
      icon: 'delete_sweep'
    };
    this.dialog.open(ConfirmDialogComponent, {
      data,
      width: '420px',
      maxWidth: '95vw',
      autoFocus: false
    }).afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.apiService.deletePoliza(poliza.id).subscribe({
        next: () => {
          this.polizas = this.polizas.filter(p => p.id !== poliza.id);
          this.performSearch();
          if (this.selectedPoliza?.id === poliza.id) {
            this.resetForm();
          }
          this.showMessage('Póliza eliminada exitosamente');
        },
        error: (error: any) => {
          this.logger.error('Error deleting poliza:', error);
          this.showMessage('Error al eliminar la póliza', 'error');
        }
      });
    });
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
      this.logger.error('Error formateando fecha:', error, date);
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

        // Buscar en aseguradora
        const aseguradoraMatch = this.normalizeText(poliza.aseguradora?.toLowerCase() || '').includes(searchLower);

        // Buscar en cédula
        const cedulaMatch = (poliza.numeroCedula || '').toLowerCase().includes(searchLower);
        
        return numeroMatch || nombreMatch || placaMatch || aseguradoraMatch || cedulaMatch;
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

  openPolizaDetail(poliza: Poliza): void {
    const ref = this.dialog.open(PolizaDetailDialogComponent, {
      data: { poliza },
      width: '520px',
      maxWidth: '95vw',
      panelClass: 'pd-dialog-panel',
      autoFocus: false,
      restoreFocus: false  // prevents browser from scrolling back to the trigger card
    });
    ref.afterClosed().subscribe(result => {
      if (result === 'edit') {
        this.selectPolizaAndScroll(poliza);
      } else if (result === 'delete') {
        this.deletePoliza(poliza);
      }
    });
  }

  selectPoliza(poliza: Poliza): void {
    this.selectPolizaAndScroll(poliza);
  }

  selectPolizaAndScroll(poliza: Poliza): void {
    // Phase 1: skeleton
    this.formLoadingState = 'loading';
    this.selectedPoliza = poliza;
    this.isEditMode = true;
    this.cdr.detectChanges();

    // Switch to form tab
    if (this.tabGroup) this.tabGroup.selectedIndex = 1;

    // Phase 2: fill form after shimmer window (200ms)
    setTimeout(() => {
      this.loadPolizaToForm(poliza);
      this.formLoadingState = 'loaded';
      this.cdr.detectChanges();

      // Phase 3: entrance animation on the card
      if (this.formSection?.nativeElement) {
        const el = this.formSection.nativeElement;
        el.classList.remove('editing-highlight');
        // eslint-disable-next-line @typescript-eslint/no-unused-expressions
        el.offsetWidth;
        el.classList.add('editing-highlight');
        setTimeout(() => el.classList.remove('editing-highlight'), 600);
      }

      // Phase 4: reset loaded state so next selection triggers skeleton again
      setTimeout(() => { this.formLoadingState = 'idle'; }, 700);
    }, 200);
  }


  loadPolizaToForm(poliza: Poliza): void {
    const parsed = parseBackendDate(poliza.fechaVigencia);
    const fechaVigencia = parsed ? parsed.toISOString().split('T')[0] : '';

    const formValues = {
      perfilId: poliza.perfilId || 1,
      numeroPoliza: poliza.numeroPoliza,
      modalidad: poliza.modalidad || '',
      nombreAsegurado: poliza.nombreAsegurado,
      numeroCedula: poliza.numeroCedula || '',
      prima: poliza.prima,
      moneda: poliza.moneda,
      fechaVigencia: fechaVigencia,
      frecuencia: this.normalizeFrecuencia(poliza.frecuencia),
      aseguradora: this.normalizeAseguradora(poliza.aseguradora),
      placa: poliza.placa || '',
      marca: poliza.marca || '',
      modelo: poliza.modelo || '',
      año: poliza.año || '',
      correo: poliza.correo || '',
      numeroTelefono: poliza.numeroTelefono || '',
      observaciones: poliza.observaciones || ''
    };
    
    this.polizaForm.patchValue(formValues);
    this.polizaForm.markAsPristine();
    this.polizaForm.markAsUntouched();
  }

  private normalizeFrecuencia(val: string): string {
    if (!val) return val;
    const map: Record<string, string> = {
      'MENSUAL': 'Mensual', 'TRIMESTRAL': 'Trimestral',
      'SEMESTRAL': 'Semestral', 'ANUAL': 'Anual',
      'BIMESTRAL': 'Bimestral', 'SEMANAL': 'Semanal', 'QUINCENAL': 'Quincenal'
    };
    return map[val.toUpperCase()] ?? val;
  }

  private normalizeAseguradora(val: string): string {
    if (!val) return val;

    const normalized = val.trim().toUpperCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[._-]+/g, ' ')
      .replace(/\s+/g, ' ');

    const map: Record<string, string> = {
      INS: 'INS',
      'INSTITUTO NACIONAL DE SEGUROS': 'INS',
      SAGICOR: 'SAGICOR',
      'SAGICOR SEGUROS': 'SAGICOR',
      ASSA: 'ASSA',
      'ASSA COMPANIA DE SEGUROS': 'ASSA',
      'ASSA COMPAÑIA DE SEGUROS': 'ASSA',
      BCR: 'BCR_SEGUROS',
      'BCR SEGUROS': 'BCR_SEGUROS',
      BCR_SEGUROS: 'BCR_SEGUROS',
      MAPFRE: 'MAPFRE',
      'MAPFRE SEGUROS COSTA RICA': 'MAPFRE',
      OTROS: 'OTROS',
      OTRAS: 'OTROS',
      'OTRAS ASEGURADORAS': 'OTROS'
    };

    return map[normalized] ?? val.trim();
  }

  /** Reinicia el estado del formulario y navega al listado */
  resetForm(): void {
    this.resetFormValues();
    if (this.tabGroup) this.tabGroup.selectedIndex = 0;
  }

  /** Solo reinicia los valores del formulario sin cambiar de tab */
  private resetFormValues(): void {
    this.polizaForm.reset();
    this.selectedPoliza = null;
    this.isEditMode = false;

    // Restablecer valores por defecto
    this.polizaForm.patchValue({
      perfilId: 1,
      moneda: CURRENCY_CONSTANTS.DEFAULT_CURRENCY,
      prima: 0,
      fechaVigencia: new Date().toISOString().split('T')[0]
    });

    // Limpiar estados de validación
    Object.keys(this.polizaForm.controls).forEach(key => {
      const control = this.polizaForm.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    });

    this.cdr.detectChanges();
  }

  cancelEdit(): void {
    this.resetForm();
  }

  get formTitle(): string {
    return this.isEditMode ? 'Editar Póliza' : 'Agregar Nueva Póliza';
  }

  get formTabLabel(): string {
    return this.isEditMode && this.selectedPoliza
      ? `Editar: ${this.selectedPoliza.numeroPoliza}`
      : 'Nueva Póliza';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Actualizar Póliza' : 'Guardar Póliza';
  }

  // Método para validar si el formulario es válido para envío
  isFormValidForSubmission(): boolean {
    const requiredFields = ['numeroPoliza', 'nombreAsegurado', 
                           'prima', 'fechaVigencia', 'frecuencia', 'aseguradora'];
    return requiredFields.every(field => {
      const control = this.polizaForm.get(field);
      return control && control.valid && control.value !== '' && control.value !== null;
    });
  }

  /** Retorna 'expired' | 'warning' | 'active' según fechaVigencia */
  getPolizaStatus(poliza: Poliza): 'expired' | 'warning' | 'active' {
    const vigencia = parseBackendDate(poliza.fechaVigencia);
    if (!vigencia) return 'active';
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    if (vigencia < hoy) return 'expired';
    const diffDays = (vigencia.getTime() - hoy.getTime()) / (1000 * 60 * 60 * 24);
    if (diffDays <= 30) return 'warning';
    return 'active';
  }

  getPolizaStatusLabel(poliza: Poliza): string {
    const s = this.getPolizaStatus(poliza);
    return s === 'expired' ? 'Vencida' : s === 'warning' ? 'Por vencer' : 'Vigente';
  }

  /** Símbolo de moneda dinámico para el campo Prima */
  get currencyPrefix(): string {
    const moneda = this.polizaForm.get('moneda')?.value || CURRENCY_CONSTANTS.DEFAULT_CURRENCY;
    const prefixMap: Record<string, string> = { CRC: '₡', USD: '$', EUR: '€' };
    return prefixMap[moneda] ?? moneda;
  }

}