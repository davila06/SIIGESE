import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { PolizasComponent } from './polizas/polizas.component';
import { UsuariosComponent } from './usuarios/usuarios.component';
import { AuthGuard } from './auth/auth.guard';
import { AdminGuard } from './auth/admin.guard';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'polizas', component: PolizasComponent, canActivate: [AuthGuard] },
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
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
