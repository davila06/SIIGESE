import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConfiguracionLayout } from './components/configuracion-layout/configuracion-layout';
import { EmailConfigList } from './components/email-config-list/email-config-list';
import { EmailConfigForm } from './components/email-config-form/email-config-form';
import { EmailTest } from './components/email-test/email-test';

const routes: Routes = [
  {
    path: '',
    component: ConfiguracionLayout,
    children: [
      { path: '', redirectTo: 'email', pathMatch: 'full' },
      { path: 'email', component: EmailConfigList },
      { path: 'email/new', component: EmailConfigForm },
      { path: 'email/edit/:id', component: EmailConfigForm },
      { path: 'email/test/:id', component: EmailTest }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ConfiguracionRoutingModule { }