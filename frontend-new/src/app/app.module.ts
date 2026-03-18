import { NgModule, LOCALE_ID } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { registerLocaleData } from '@angular/common';
import localeEsCR from '@angular/common/locales/es-CR';

// Angular Material Date
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';

// Angular Material Modules
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { PolizasComponent } from './polizas/polizas.component';
import { UploadPolizasComponent } from './polizas/upload-polizas.component';
import { LoginComponent } from './auth/login.component';
import { CotizacionesComponent } from './cotizaciones/cotizaciones.component';
import { ChangePasswordComponent } from './auth/change-password/change-password.component';
import { AuthInterceptor } from './auth/auth.interceptor';
import { UsuariosComponent } from './usuarios/usuarios.component';

// Configurar locale español Costa Rica
registerLocaleData(localeEsCR);

// Configurar formatos de fecha personalizados DD-MM-YYYY
export const CUSTOM_DATE_FORMATS = {
  parse: {
    dateInput: ['DD-MM-YYYY', 'DD/MM/YYYY'],
  },
  display: {
    dateInput: 'DD-MM-YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'DD-MM-YYYY',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@NgModule({
  declarations: [
    AppComponent,
    PolizasComponent,
    UploadPolizasComponent,
    LoginComponent,
    ChangePasswordComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    
    // Angular Material
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatToolbarModule,
    MatMenuModule,
    MatButtonToggleModule,
    MatTooltipModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatCheckboxModule,
    MatDialogModule,
    MatSidenavModule,
    MatListModule,
    MatProgressBarModule,
    MatExpansionModule,
    MatDatepickerModule,
    MatNativeDateModule,
    
    // Standalone Components
    UsuariosComponent
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: LOCALE_ID,
      useValue: 'es-CR'
    },
    {
      provide: MAT_DATE_FORMATS,
      useValue: CUSTOM_DATE_FORMATS
    },
    {
      provide: MAT_DATE_LOCALE,
      useValue: 'es-CR'
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
