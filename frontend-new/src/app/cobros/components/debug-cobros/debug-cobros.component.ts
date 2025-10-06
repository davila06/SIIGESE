import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-debug-cobros',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Debug Cobros API</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <div style="margin-bottom: 16px;">
          <button mat-raised-button color="primary" (click)="testCobrosAPI()">
            Test Cobros API
          </button>
          <button mat-raised-button color="accent" (click)="testAuth()" style="margin-left: 8px;">
            Test Auth Token
          </button>
        </div>
        
        <div *ngIf="authInfo">
          <h3>Información de Autenticación:</h3>
          <pre>{{ authInfo | json }}</pre>
        </div>
        
        <div *ngIf="apiResponse">
          <h3>Respuesta de API:</h3>
          <pre>{{ apiResponse | json }}</pre>
        </div>
        
        <div *ngIf="error" style="color: red;">
          <h3>Error:</h3>
          <pre>{{ error | json }}</pre>
        </div>
      </mat-card-content>
    </mat-card>
  `
})
export class DebugCobrosComponent implements OnInit {
  authInfo: any = null;
  apiResponse: any = null;
  error: any = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.testAuth();
  }

  testAuth() {
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('currentUser');
    
    this.authInfo = {
      hasToken: !!token,
      tokenLength: token ? token.length : 0,
      tokenPreview: token ? token.substring(0, 50) + '...' : null,
      hasUser: !!user,
      user: user ? JSON.parse(user) : null
    };
  }

  testCobrosAPI() {
    const token = localStorage.getItem('authToken');
    const options = token ? 
      { headers: { 'Authorization': `Bearer ${token}` } } : 
      {};
    
    this.http.get(`${environment.apiUrl}/api/cobros`, options)
      .subscribe({
        next: (response) => {
          this.apiResponse = response;
          this.error = null;
        },
        error: (error) => {
          this.error = error;
          this.apiResponse = null;
        }
      });
  }
}