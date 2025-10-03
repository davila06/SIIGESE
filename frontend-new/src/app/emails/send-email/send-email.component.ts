import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { EmailService, EmailRequest } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-send-email',
    templateUrl: './send-email.component.html',
    styleUrls: ['./send-email.component.scss'],
    standalone: false
})
export class SendEmailComponent implements OnInit {
  emailForm: FormGroup;
  loading = false;
  emailTypes = [
    { value: 'Manual', label: 'Email Manual' },
    { value: 'CobroVencido', label: 'Cobro Vencido' },
    { value: 'ReclamoRecibido', label: 'Reclamo Recibido' },
    { value: 'Bienvenida', label: 'Bienvenida' },
    { value: 'PolizaPorVencer', label: 'Póliza por Vencer' }
  ];

  emailTemplates = {
    'Manual': {
      subject: '',
      body: ''
    },
    'CobroVencido': {
      subject: 'Recordatorio de Pago - Póliza #{numeroPoliza}',
      body: `<p>Estimado/a cliente,</p>
<p>Le recordamos que su póliza #{numeroPoliza} tiene un pago pendiente:</p>
<ul>
  <li><strong>Monto:</strong> {montoVencido}</li>
  <li><strong>Fecha de vencimiento:</strong> {fechaVencimiento}</li>
  <li><strong>Días vencidos:</strong> {diasVencido}</li>
</ul>
<p>Por favor, proceda con el pago a la brevedad posible.</p>
<p>Gracias por su preferencia.</p>`
    },
    'ReclamoRecibido': {
      subject: 'Confirmación de Reclamo #{numeroReclamo}',
      body: `<p>Estimado/a cliente,</p>
<p>Hemos recibido su reclamo con los siguientes detalles:</p>
<ul>
  <li><strong>Número de reclamo:</strong> {numeroReclamo}</li>
  <li><strong>Póliza:</strong> {numeroPoliza}</li>
  <li><strong>Fecha:</strong> {fechaReclamo}</li>
</ul>
<p>Nuestro equipo revisará su caso y le contactaremos pronto.</p>
<p>Gracias por confiar en nosotros.</p>`
    },
    'Bienvenida': {
      subject: 'Bienvenido al Sistema SIINADSEG',
      body: `<p>¡Bienvenido/a al Sistema SIINADSEG!</p>
<p>Sus credenciales de acceso son:</p>
<ul>
  <li><strong>Usuario:</strong> {userName}</li>
  <li><strong>Contraseña temporal:</strong> {temporalPassword}</li>
</ul>
<p>Por favor, cambie su contraseña en el primer acceso.</p>
<p>¡Gracias por unirse a nuestro equipo!</p>`
    }
  };

  constructor(
    private fb: FormBuilder,
    private emailService: EmailService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.emailForm = this.fb.group({
      toEmail: ['', [Validators.required, Validators.email]],
      toName: [''],
      subject: ['', Validators.required],
      body: ['', Validators.required],
      emailType: ['Manual', Validators.required]
    });
  }

  ngOnInit(): void {
    this.onEmailTypeChange();
  }

  onEmailTypeChange(): void {
    const emailType = this.emailForm.get('emailType')?.value;
    if (emailType && this.emailTemplates[emailType as keyof typeof this.emailTemplates]) {
      const template = this.emailTemplates[emailType as keyof typeof this.emailTemplates];
      this.emailForm.patchValue({
        subject: template.subject,
        body: template.body
      });
    }
  }

  onSubmit(): void {
    if (this.emailForm.valid) {
      this.loading = true;
      
      const emailRequest: EmailRequest = {
        toEmail: this.emailForm.value.toEmail,
        toName: this.emailForm.value.toName,
        subject: this.emailForm.value.subject,
        body: this.emailForm.value.body,
        emailType: this.emailForm.value.emailType
      };

      this.emailService.sendEmail(emailRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.snackBar.open('Email enviado exitosamente', 'Cerrar', { duration: 3000 });
            this.router.navigate(['/emails']);
          } else {
            this.snackBar.open(`Error: ${response.message}`, 'Cerrar', { duration: 5000 });
          }
          this.loading = false;
        },
        error: (error) => {
          this.snackBar.open('Error enviando email', 'Cerrar', { duration: 3000 });
          console.error('Error:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/emails']);
  }

  previewEmail(): void {
    // Implementar preview del email
    console.log('Preview:', this.emailForm.value);
  }
}
