import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { ChangePasswordComponent } from './auth/change-password/change-password.component';
import { PolizasComponent } from './polizas/polizas.component';
import { UploadPolizasComponent } from './polizas/upload-polizas.component';
import { CotizacionesComponent } from './cotizaciones/cotizaciones.component';
import { UsuariosComponent } from './usuarios/usuarios.component';
import { AuthGuard } from './auth/auth.guard';
import { AdminGuard } from './auth/admin.guard';
import { DataLoaderGuard } from './auth/data-loader.guard';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'change-password', component: ChangePasswordComponent },
  { path: 'polizas', component: PolizasComponent, canActivate: [AuthGuard] },
  { path: 'polizas/upload', component: UploadPolizasComponent, canActivate: [AuthGuard, DataLoaderGuard] },
  { path: 'cotizaciones', component: CotizacionesComponent, canActivate: [AuthGuard] },
  { path: 'usuarios', component: UsuariosComponent, canActivate: [AuthGuard, AdminGuard] },
  { 
    path: 'cobros', 
    loadChildren: () => import('./cobros/cobros.module').then(m => m.CobrosModule),
    canActivate: [AuthGuard]
  },
  { 
    path: 'reclamos', 
    loadChildren: () => import('./reclamos/reclamos.module').then(m => m.ReclamosModule),
    canActivate: [AuthGuard]
  },
  { 
    path: 'emails', 
    loadChildren: () => import('./emails/emails.module').then(m => m.EmailsModule),
    canActivate: [AuthGuard]
  },
  { 
    path: 'configuracion', 
    loadChildren: () => import('./configuracion/configuracion.module').then(m => m.ConfiguracionModule),
    canActivate: [AuthGuard, AdminGuard]
  },
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { 
    enableTracing: false, // Cambiar a true para debugging
    useHash: false // Usar HTML5 routing
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
