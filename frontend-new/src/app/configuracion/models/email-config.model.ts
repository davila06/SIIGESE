export interface EmailConfig {
  id: number;
  configName: string;
  smtpServer: string;
  smtpPort: number;
  fromEmail: string;
  fromName: string;
  username: string;
  useSSL: boolean;
  useTLS: boolean;
  isDefault: boolean;
  isActive: boolean;
  description?: string;
  companyName?: string;
  companyAddress?: string;
  companyPhone?: string;
  companyWebsite?: string;
  companyLogo?: string;
  timeoutSeconds: number;
  maxRetries: number;
  lastTested: Date;
  lastTestSuccessful: boolean;
  lastTestError?: string;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  updatedBy: string;
}

export interface EmailConfigCreate {
  configName: string;
  smtpServer: string;
  smtpPort: number;
  fromEmail: string;
  fromName: string;
  username: string;
  password: string;
  useSSL: boolean;
  useTLS: boolean;
  isDefault: boolean;
  isActive: boolean;
  description?: string;
  companyName?: string;
  companyAddress?: string;
  companyPhone?: string;
  companyWebsite?: string;
  companyLogo?: string;
  timeoutSeconds: number;
  maxRetries: number;
}

export interface EmailConfigUpdate {
  configName: string;
  smtpServer: string;
  smtpPort: number;
  fromEmail: string;
  fromName: string;
  username: string;
  password?: string; // Opcional en actualización
  useSSL: boolean;
  useTLS: boolean;
  isDefault: boolean;
  isActive: boolean;
  description?: string;
  companyName?: string;
  companyAddress?: string;
  companyPhone?: string;
  companyWebsite?: string;
  companyLogo?: string;
  timeoutSeconds: number;
  maxRetries: number;
}

export interface EmailTestRequest {
  configId: number;
  toEmail: string;
  subject: string;
  body: string;
}

export interface EmailTestResponse {
  success: boolean;
  message: string;
  testedAt: Date;
  errorDetails?: string;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data?: T;
  errors: string[];
}