import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';

import { environment } from './environments/environment';

console.log('🚀 SIINADSEG Application Starting - Build:', new Date().toISOString());
console.log('📅 Version:', environment.version || 'unknown');
console.log('🌐 API URL:', environment.apiUrl);
console.log('🔧 Environment:', environment.production ? 'PRODUCTION' : 'DEVELOPMENT');

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error('❌ Bootstrap Error:', err));
