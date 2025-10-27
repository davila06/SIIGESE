import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule, MatTabGroup } from '@angular/material/tabs';

import { CotizacionService } from '../services/cotizacion.service';
import { Cotizacion, CreateCotizacion, UpdateCotizacion, TIPOS_SEGURO, ESTADOS_COTIZACION, MONEDAS, GENEROS, TIPOS_INMUEBLE } from '../models/cotizacion.model';
import { formatCurrencyByCode, formatDateCR, CURRENCY_CONSTANTS } from '../shared/constants/currency.constants';

@Component({
  selector: 'app-cotizaciones',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatDialogModule,
    MatChipsModule,
    MatMenuModule,
    MatTooltipModule,
    MatTabsModule
  ],
  templateUrl: './cotizaciones.component.html',
  styleUrl: './cotizaciones.component.scss'
})
export class CotizacionesComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatTabGroup) tabGroup!: MatTabGroup;

  cotizaciones: Cotizacion[] = [];
  cotizacionesDataSource = new MatTableDataSource<Cotizacion>();
  
  cotizacionForm: FormGroup;
  isEditMode = false;
  selectedCotizacion: Cotizacion | null = null;
  isLoading = false;
  
  // Configuraci├│n de columnas de la tabla
  displayedColumns: string[] = [
    'numeroCotizacion',
    'nombreSolicitante',
    'email',
    'tipoSeguro',
    'aseguradora',
    'montoAsegurado',
    'estado',
    'fechaCotizacion',
    'acciones'
  ];

  // Datos para los selectores
  tiposSeguro = TIPOS_SEGURO;
  estadosCotizacion = ESTADOS_COTIZACION;
  monedas = MONEDAS;
  generos = GENEROS;
  tiposInmueble = TIPOS_INMUEBLE;

  // Filtros de b├║squeda
  searchTerm = '';
  selectedTipoSeguro = '';
  selectedEstado = '';

  constructor(
    private cotizacionService: CotizacionService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.cotizacionForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCotizaciones();
  }

  ngAfterViewInit(): void {
    this.cotizacionesDataSource.paginator = this.paginator;
    this.cotizacionesDataSource.sort = this.sort;
  }

  createForm(): FormGroup {
    return this.fb.group({
      nombreSolicitante: ['', [Validators.required, Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      telefono: ['', [Validators.maxLength(20)]],
      tipoSeguro: ['', [Validators.required]],
      aseguradora: ['', [Validators.required, Validators.maxLength(100)]],
      montoAsegurado: [0, [Validators.required, Validators.min(1)]],
      primaCotizada: [0, [Validators.min(0)]],
      moneda: [CURRENCY_CONSTANTS.DEFAULT_CURRENCY, [Validators.required]],
      fechaVencimiento: [''],
      observaciones: ['', [Validators.maxLength(500)]],
      
      // Campos espec├¡ficos para auto
      placa: ['', [Validators.maxLength(20)]],
      marca: ['', [Validators.maxLength(50)]],
      modelo: ['', [Validators.maxLength(50)]],
      año: [null, [Validators.min(1900), Validators.max(2100)]],
      cilindraje: ['', [Validators.maxLength(50)]],
      
      // Campos espec├¡ficos para vida
      fechaNacimiento: [''],
      genero: [''],
      ocupacion: ['', [Validators.maxLength(100)]],
      
      // Campos espec├¡ficos para hogar
      direccionInmueble: ['', [Validators.maxLength(200)]],
      tipoInmueble: [''],
      valorInmueble: [0, [Validators.min(0)]]
    });
  }

  loadCotizaciones(): void {
    this.isLoading = true;
    this.cotizacionService.getCotizaciones().subscribe({
      next: (cotizaciones: Cotizacion[]) => {
        this.cotizaciones = cotizaciones;
        this.cotizacionesDataSource.data = cotizaciones;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading cotizaciones:', error);
        this.showMessage('Error al cargar las cotizaciones', 'error');
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.cotizacionForm.valid) {
      this.isLoading = true;
      const cotizacionData: CreateCotizacion = this.cotizacionForm.value;

      if (this.isEditMode && this.selectedCotizacion) {
        // Actualizar cotizaci├│n existente
        this.cotizacionService.updateCotizacion(this.selectedCotizacion.id, cotizacionData).subscribe({
          next: (updatedCotizacion: Cotizacion) => {
            const index = this.cotizaciones.findIndex(c => c.id === updatedCotizacion.id);
            if (index !== -1) {
              this.cotizaciones[index] = updatedCotizacion;
              this.cotizacionesDataSource.data = [...this.cotizaciones];
            }
            this.resetForm();
            this.showMessage('Cotizaci├│n actualizada exitosamente');
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error updating cotizacion:', error);
            this.showMessage('Error al actualizar la cotizaci├│n', 'error');
            this.isLoading = false;
          }
        });
      } else {
        // Crear nueva cotizaci├│n
        this.cotizacionService.createCotizacion(cotizacionData).subscribe({
          next: (cotizacion: Cotizacion) => {
            this.cotizaciones.unshift(cotizacion);
            this.cotizacionesDataSource.data = [...this.cotizaciones];
            this.resetForm();
            this.showMessage('Cotizaci├│n creada exitosamente');
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error creating cotizacion:', error);
            this.showMessage('Error al crear la cotizaci├│n', 'error');
            this.isLoading = false;
          }
        });
      }
    }
  }

  editCotizacion(cotizacion: Cotizacion): void {
    console.log('🔄 Iniciando edición de cotización:', cotizacion.numeroCotizacion);
    
    this.selectedCotizacion = cotizacion;
    this.isEditMode = true;
    
    // Cargar datos en el formulario
    this.cotizacionForm.patchValue({
      nombreSolicitante: cotizacion.nombreSolicitante,
      email: cotizacion.email,
      telefono: cotizacion.telefono,
      tipoSeguro: cotizacion.tipoSeguro,
      aseguradora: cotizacion.aseguradora,
      montoAsegurado: cotizacion.montoAsegurado,
      primaCotizada: cotizacion.primaCotizada,
      moneda: cotizacion.moneda,
      fechaVencimiento: cotizacion.fechaVencimiento ? new Date(cotizacion.fechaVencimiento) : null,
      observaciones: cotizacion.observaciones,
      
      // Campos espec├¡ficos
      placa: cotizacion.placa,
      marca: cotizacion.marca,
      modelo: cotizacion.modelo,
      año: cotizacion.año,
      cilindraje: cotizacion.cilindraje,
      fechaNacimiento: cotizacion.fechaNacimiento ? new Date(cotizacion.fechaNacimiento) : null,
      genero: cotizacion.genero,
      ocupacion: cotizacion.ocupacion,
      direccionInmueble: cotizacion.direccionInmueble,
      tipoInmueble: cotizacion.tipoInmueble,
      valorInmueble: cotizacion.valorInmueble
    });
    
    // Marcar el formulario como pristine después de cargar los datos
    this.cotizacionForm.markAsPristine();
    this.cotizacionForm.markAsUntouched();
    
    console.log('📝 Datos cargados en formulario:', {
      numeroCotizacion: cotizacion.numeroCotizacion,
      nombreSolicitante: cotizacion.nombreSolicitante,
      tipoSeguro: cotizacion.tipoSeguro,
      isEditMode: this.isEditMode
    });
    
    // Cambiar automáticamente al tab del formulario
    setTimeout(() => {
      if (this.tabGroup) {
        this.tabGroup.selectedIndex = 1; // Índice 1 = tab del formulario
        console.log('✅ Cambiado al tab del formulario para edición');
      }
    }, 100);
  }

  deleteCotizacion(cotizacion: Cotizacion): void {
    if (confirm(`┬┐Est├í seguro de eliminar la cotizaci├│n ${cotizacion.numeroCotizacion}?`)) {
      this.cotizacionService.deleteCotizacion(cotizacion.id).subscribe({
        next: () => {
          this.cotizaciones = this.cotizaciones.filter(c => c.id !== cotizacion.id);
          this.cotizacionesDataSource.data = [...this.cotizaciones];
          this.showMessage('Cotizaci├│n eliminada exitosamente');
          
          if (this.selectedCotizacion?.id === cotizacion.id) {
            this.resetForm();
          }
        },
        error: (error: any) => {
          console.error('Error deleting cotizacion:', error);
          this.showMessage('Error al eliminar la cotizaci├│n', 'error');
        }
      });
    }
  }

  updateEstado(cotizacion: Cotizacion, nuevoEstado: string): void {
    this.cotizacionService.updateEstado(cotizacion.id, nuevoEstado).subscribe({
      next: (updatedCotizacion: Cotizacion) => {
        const index = this.cotizaciones.findIndex(c => c.id === updatedCotizacion.id);
        if (index !== -1) {
          this.cotizaciones[index] = updatedCotizacion;
          this.cotizacionesDataSource.data = [...this.cotizaciones];
        }
        this.showMessage(`Estado actualizado a ${nuevoEstado}`);
      },
      error: (error: any) => {
        console.error('Error updating estado:', error);
        this.showMessage('Error al actualizar el estado', 'error');
      }
    });
  }

  resetForm(): void {
    this.cotizacionForm.reset();
    this.selectedCotizacion = null;
    this.isEditMode = false;
    
    // Restablecer valores por defecto
    this.cotizacionForm.patchValue({
      moneda: CURRENCY_CONSTANTS.DEFAULT_CURRENCY,
      montoAsegurado: 0,
      primaCotizada: 0,
      valorInmueble: 0
    });
    
    // Limpiar estados de validación
    Object.keys(this.cotizacionForm.controls).forEach(key => {
      const control = this.cotizacionForm.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    });
    
    // Regresar al tab de lista cuando se cancela
    setTimeout(() => {
      if (this.tabGroup) {
        this.tabGroup.selectedIndex = 0; // Índice 0 = tab de lista
        console.log('↩️ Regresado al tab de lista');
      }
    }, 100);
  }

  // Filtros y b├║squeda
  applyFilter(): void {
    let filteredData = this.cotizaciones;

    // Filtro por t├®rmino de b├║squeda
    if (this.searchTerm) {
      const searchLower = this.searchTerm.toLowerCase();
      filteredData = filteredData.filter(c => 
        c.numeroCotizacion.toLowerCase().includes(searchLower) ||
        c.nombreSolicitante.toLowerCase().includes(searchLower) ||
        c.email.toLowerCase().includes(searchLower)
      );
    }

    // Filtro por tipo de seguro
    if (this.selectedTipoSeguro) {
      filteredData = filteredData.filter(c => c.tipoSeguro === this.selectedTipoSeguro);
    }

    // Filtro por estado
    if (this.selectedEstado) {
      filteredData = filteredData.filter(c => c.estado === this.selectedEstado);
    }

    this.cotizacionesDataSource.data = filteredData;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedTipoSeguro = '';
    this.selectedEstado = '';
    this.cotizacionesDataSource.data = this.cotizaciones;
  }

  // Utilidades
  formatCurrency(amount: number, currency: string = CURRENCY_CONSTANTS.DEFAULT_CURRENCY): string {
    return formatCurrencyByCode(amount, currency);
  }

  formatDate(date: Date): string {
    return formatDateCR(date);
  }

  getEstadoColor(estado: string): string {
    const estadoObj = this.estadosCotizacion.find(e => e.value === estado);
    return estadoObj?.color || 'primary';
  }

  getTipoSeguroLabel(tipo: string): string {
    const tipoObj = this.tiposSeguro.find(t => t.value === tipo);
    return tipoObj?.label || tipo;
  }

  get formTitle(): string {
    return this.isEditMode ? 'Editar Cotizaci├│n' : 'Nueva Cotizaci├│n';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Actualizar Cotizaci├│n' : 'Crear Cotizaci├│n';
  }

  // Campos espec├¡ficos por tipo de seguro
  get isAutoSeguro(): boolean {
    return this.cotizacionForm.get('tipoSeguro')?.value === 'AUTO';
  }

  get isVidaSeguro(): boolean {
    return this.cotizacionForm.get('tipoSeguro')?.value === 'VIDA';
  }

  get isHogarSeguro(): boolean {
    return this.cotizacionForm.get('tipoSeguro')?.value === 'HOGAR';
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
}
