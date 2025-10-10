import { Component } from '@angular/core';

@Component({
  selector: 'app-configuracion-layout',
  standalone: false,
  templateUrl: './configuracion-layout.html',
  styleUrl: './configuracion-layout.scss'
})
export class ConfiguracionLayout {
  menuItems = [
    {
      label: 'Configuración de Email',
      route: '/configuracion/email',
      icon: 'email',
      description: 'Administrar configuraciones SMTP para el envío de correos electrónicos'
    }
  ];
}
