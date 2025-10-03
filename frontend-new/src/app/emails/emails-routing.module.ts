import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmailDashboardComponent } from './email-dashboard/email-dashboard.component';
import { SendEmailComponent } from './send-email/send-email.component';
import { EmailHistoryComponent } from './email-history/email-history.component';
import { AutomaticNotificationsComponent } from './components/automatic-notifications/automatic-notifications.component';
import { EmailTemplateManagerComponent } from './components/email-template-manager/email-template-manager.component';

const routes: Routes = [
  {
    path: '',
    component: EmailDashboardComponent
  },
  {
    path: 'send',
    component: SendEmailComponent
  },
  {
    path: 'history',
    component: EmailHistoryComponent
  },
  {
    path: 'notifications',
    component: AutomaticNotificationsComponent
  },
  {
    path: 'templates',
    component: EmailTemplateManagerComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmailsRoutingModule { }
