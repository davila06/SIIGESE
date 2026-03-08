import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EmailTemplate {
  id: number;
  templateType: string;
  name: string;
  subject: string;
  htmlContent: string;
  isActive: boolean;
  isDefault: boolean;
  createdDate: Date;
  modifiedDate?: Date;
  availableVariables: string[];
}

export interface EmailTemplateRequest {
  templateType: string;
  name: string;
  subject: string;
  htmlContent: string;
  isDefault: boolean;
}

export interface CustomEmailRequest {
  toEmail: string;
  toName?: string;
  subject: string;
  htmlContent: string;
  emailType?: string;
  templateId?: number;
  variables: { [key: string]: string };
  sendImmediately: boolean;
  scheduledDate?: Date;
}

export interface EmailPreviewRequest {
  templateType: string;
  subject: string;
  htmlContent: string;
  sampleData: { [key: string]: string };
}

export interface EmailPreviewResponse {
  subject: string;
  htmlContent: string;
  plainTextContent: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailTemplateService {
  private apiUrl = `${environment.apiUrl}/api/emailtemplates`;

  constructor(private http: HttpClient) {}

  getTemplatesByType(templateType: string): Observable<EmailTemplate[]> {
    return this.http.get<EmailTemplate[]>(`${this.apiUrl}/by-type/${templateType}`);
  }

  getTemplate(id: number): Observable<EmailTemplate> {
    return this.http.get<EmailTemplate>(`${this.apiUrl}/${id}`);
  }

  getDefaultTemplate(templateType: string): Observable<EmailTemplate> {
    return this.http.get<EmailTemplate>(`${this.apiUrl}/default/${templateType}`);
  }

  createTemplate(request: EmailTemplateRequest): Observable<EmailTemplate> {
    return this.http.post<EmailTemplate>(this.apiUrl, request);
  }

  updateTemplate(id: number, request: EmailTemplateRequest): Observable<EmailTemplate> {
    return this.http.put<EmailTemplate>(`${this.apiUrl}/${id}`, request);
  }

  deleteTemplate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  setAsDefault(id: number, templateType: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/set-default`, templateType);
  }

  previewTemplate(request: EmailPreviewRequest): Observable<EmailPreviewResponse> {
    return this.http.post<EmailPreviewResponse>(`${this.apiUrl}/preview`, request);
  }

  getAvailableVariables(templateType: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/variables/${templateType}`);
  }

  sendCustomEmail(request: CustomEmailRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/api/email/custom`, request);
  }
}