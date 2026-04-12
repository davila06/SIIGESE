import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { ChangePasswordComponent } from './auth/change-password/change-password.component';
import { PolizasComponent } from './polizas/polizas.component';
import { UploadPolizasComponent } from './polizas/upload-polizas.component';
import { CotizacionesComponent } from './cotizaciones/cotizaciones.component';
import { UsuariosComponent } from './usuarios/usuarios.component';
import { authGuard } from './auth/auth.guard';
import { adminGuard } from './auth/admin.guard';
import { dataLoaderGuard } from './auth/data-loader.guard';
import { loginGuard } from './auth/login.guard';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, canActivate: [loginGuard] },
  { path: 'change-password', component: ChangePasswordComponent },
  { path: 'dashboard', redirectTo: '/analytics', pathMatch: 'full' }, // Redirect to analytics home
  {
    path: 'analytics',
    loadChildren: () => import('./analytics/analytics.module').then(m => m.AnalyticsModule),
    canActivate: [authGuard]
  },
  { path: 'polizas', component: PolizasComponent, canActivate: [authGuard] },
  { path: 'polizas/upload', component: UploadPolizasComponent, canActivate: [authGuard, dataLoaderGuard] },
  { path: 'cotizaciones', component: CotizacionesComponent, canActivate: [authGuard] },
  { path: 'usuarios', component: UsuariosComponent, canActivate: [authGuard, adminGuard] },
  { 
    path: 'cobros', 
    loadChildren: () => import('./cobros/cobros.module').then(m => m.CobrosModule),
    canActivate: [authGuard]
  },
  { 
    path: 'reclamos', 
    loadChildren: () => import('./reclamos/reclamos.module').then(m => m.ReclamosModule),
    canActivate: [authGuard]
  },
  { 
    path: 'emails', 
    loadChildren: () => import('./emails/emails.module').then(m => m.EmailsModule),
    canActivate: [authGuard]
  },
  { 
    path: 'configuracion', 
    loadChildren: () => import('./configuracion/configuracion.module').then(m => m.ConfiguracionModule),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'chat',
    loadChildren: () => import('./chat/chat.module').then(m => m.ChatModule),
    canActivate: [authGuard]
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
