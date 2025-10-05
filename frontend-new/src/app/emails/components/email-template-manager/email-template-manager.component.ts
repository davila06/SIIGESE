import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { EmailTemplateService, EmailTemplate, EmailTemplateRequest } from '../../services/email-template.service';

@Component({
  selector: 'app-email-template-manager',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatTableModule,
    MatPaginatorModule,
    MatChipsModule
  ],
  templateUrl: './email-template-manager.component.html',
  styleUrls: ['./email-template-manager.component.scss']
})
export class EmailTemplateManagerComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  templates = new MatTableDataSource<EmailTemplate>();
  displayedColumns: string[] = ['name', 'templateType', 'isDefault', 'createdDate', 'actions'];
  
  templateForm: FormGroup;
  isEditing = false;
  currentTemplateId?: number;
  selectedTemplateType = 'CobroVencido';
  availableVariables: string[] = [];
  previewHtml = '';
  
  templateTypes = [
    { value: 'CobroVencido', label: 'Cobro Vencido' },
    { value: 'PolizaVencimiento', label: 'Póliza por Vencer' },
    { value: 'Bienvenida', label: 'Bienvenida' },
    { value: 'ReclamoRecibido', label: 'Reclamo Recibido' }
  ];

  sampleData: { [key: string]: { [key: string]: string } } = {
    'CobroVencido': {
      'ClienteNombre': 'Juan',
      'ClienteApellido': 'Pérez',
      'ClienteEmail': 'juan.perez@email.com',
      'NumeroRecibo': 'REC-001',
      'NumeroPoliza': 'POL-12345',
      'MontoTotal': '1500.00',
      'FechaVencimiento': '2025-09-15',
      'DiasVencido': '18'
    },
    'PolizaVencimiento': {
      'NombreCliente': 'María García',
      'NumeroPoliza': 'POL-67890',
      'TipoPoliza': 'Seguro de Auto',
      'FechaVencimiento': '2025-11-30',
      'DiasHastaVencimiento': '58',
      'MontoAsegurado': '50000.00',
      'Prima': '2500.00'
    },
    'Bienvenida': {
      'UsuarioNombre': 'Carlos',
      'UsuarioApellido': 'López',
      'UsuarioEmail': 'carlos.lopez@email.com',
      'Username': 'clopez',
      'PasswordTemporal': 'temp123',
      'RolNombre': 'Cliente'
    },
    'ReclamoRecibido': {
      'ClienteNombre': 'Ana',
      'ClienteApellido': 'Martínez',
      'ClienteEmail': 'ana.martinez@email.com',
      'NumeroReclamo': 'REC-789',
      'TipoReclamo': 'Siniestro',
      'Descripcion': 'Accidente de tránsito',
      'FechaCreacion': '2025-10-03'
    }
  };

  constructor(
    private fb: FormBuilder,
    private templateService: EmailTemplateService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.templateForm = this.fb.group({
      templateType: [this.selectedTemplateType, Validators.required],
      name: ['', Validators.required],
      subject: ['', Validators.required],
      htmlContent: ['', Validators.required],
      isDefault: [false]
    });
  }

  ngOnInit() {
    this.loadTemplates();
    this.loadAvailableVariables();
    this.setupFormSubscriptions();
  }

  loadTemplates() {
    this.templateService.getTemplatesByType(this.selectedTemplateType).subscribe({
      next: (templates) => {
        this.templates.data = templates;
        this.templates.paginator = this.paginator;
      },
      error: (error) => {
        console.error('Error cargando templates:', error);
        this.snackBar.open('Error cargando templates', 'Cerrar', { duration: 3000 });
      }
    });
  }

  loadAvailableVariables() {
    this.templateService.getAvailableVariables(this.selectedTemplateType).subscribe({
      next: (variables) => {
        this.availableVariables = variables;
      },
      error: (error) => {
        console.error('Error cargando variables:', error);
      }
    });
  }

  setupFormSubscriptions() {
    this.templateForm.get('templateType')?.valueChanges.subscribe(value => {
      this.selectedTemplateType = value;
      this.loadTemplates();
      this.loadAvailableVariables();
    });

    // Auto-preview en tiempo real
    this.templateForm.valueChanges.subscribe(() => {
      this.generatePreview();
    });
  }

  onTemplateTypeChange() {
    this.loadTemplates();
    this.loadAvailableVariables();
    this.resetForm();
  }

  editTemplate(template: EmailTemplate) {
    this.isEditing = true;
    this.currentTemplateId = template.id;
    this.templateForm.patchValue({
      templateType: template.templateType,
      name: template.name,
      subject: template.subject,
      htmlContent: template.htmlContent,
      isDefault: template.isDefault
    });
  }

  saveTemplate() {
    if (this.templateForm.valid) {
      const request: EmailTemplateRequest = this.templateForm.value;

      const operation = this.isEditing && this.currentTemplateId
        ? this.templateService.updateTemplate(this.currentTemplateId, request)
        : this.templateService.createTemplate(request);

      operation.subscribe({
        next: () => {
          this.snackBar.open(
            this.isEditing ? 'Template actualizado' : 'Template creado',
            'Cerrar',
            { duration: 3000 }
          );
          this.loadTemplates();
          this.resetForm();
        },
        error: (error) => {
          console.error('Error guardando template:', error);
          this.snackBar.open('Error guardando template', 'Cerrar', { duration: 3000 });
        }
      });
    }
  }

  deleteTemplate(id: number) {
    if (confirm('¿Está seguro de eliminar este template?')) {
      this.templateService.deleteTemplate(id).subscribe({
        next: () => {
          this.snackBar.open('Template eliminado', 'Cerrar', { duration: 3000 });
          this.loadTemplates();
        },
        error: (error) => {
          console.error('Error eliminando template:', error);
          this.snackBar.open('Error eliminando template', 'Cerrar', { duration: 3000 });
        }
      });
    }
  }

  setAsDefault(template: EmailTemplate) {
    this.templateService.setAsDefault(template.id, template.templateType).subscribe({
      next: () => {
        this.snackBar.open('Template establecido como por defecto', 'Cerrar', { duration: 3000 });
        this.loadTemplates();
      },
      error: (error) => {
        console.error('Error estableciendo por defecto:', error);
        this.snackBar.open('Error estableciendo por defecto', 'Cerrar', { duration: 3000 });
      }
    });
  }

  generatePreview() {
    const formValue = this.templateForm.value;
    if (formValue.subject && formValue.htmlContent) {
      const request = {
        templateType: formValue.templateType,
        subject: formValue.subject,
        htmlContent: formValue.htmlContent,
        sampleData: this.sampleData[formValue.templateType] || {}
      };

      this.templateService.previewTemplate(request).subscribe({
        next: (preview) => {
          this.previewHtml = preview.htmlContent;
        },
        error: (error) => {
          console.error('Error generando preview:', error);
        }
      });
    }
  }

  insertVariable(variable: string) {
    const currentContent = this.templateForm.get('htmlContent')?.value || '';
    const variableTag = `{{${variable}}}`;
    this.templateForm.patchValue({
      htmlContent: currentContent + variableTag
    });
  }

  resetForm() {
    this.isEditing = false;
    this.currentTemplateId = undefined;
    this.templateForm.reset({
      templateType: this.selectedTemplateType,
      name: '',
      subject: '',
      htmlContent: '',
      isDefault: false
    });
    this.previewHtml = '';
  }

  getDefaultTemplate() {
    this.templateService.getDefaultTemplate(this.selectedTemplateType).subscribe({
      next: (template) => {
        this.templateForm.patchValue({
          subject: template.subject,
          htmlContent: template.htmlContent
        });
      },
      error: (error) => {
        console.error('Error cargando template por defecto:', error);
      }
    });
  }
}