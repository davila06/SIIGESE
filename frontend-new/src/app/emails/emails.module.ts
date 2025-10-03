import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { EmailsRoutingModule } from './emails-routing.module';
import { EmailDashboardComponent } from './email-dashboard/email-dashboard.component';
import { SendEmailComponent } from './send-email/send-email.component';
import { EmailHistoryComponent } from './email-history/email-history.component';
import { AutomaticNotificationsComponent } from './components/automatic-notifications/automatic-notifications.component';
import { EmailTemplateManagerComponent } from './components/email-template-manager/email-template-manager.component';

@NgModule({
  declarations: [
    EmailDashboardComponent,
    SendEmailComponent,
    EmailHistoryComponent,
    AutomaticNotificationsComponent,
    EmailTemplateManagerComponent
  ],
  imports: [
    CommonModule,
    EmailsRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTabsModule,
    MatTooltipModule,
    MatCheckboxModule
  ]
})
export class EmailsModule { }
