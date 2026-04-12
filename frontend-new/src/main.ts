import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';

import { environment } from './environments/environment';

// Development-only diagnostics — suppressed in production to avoid leaking
// internal URLs and build metadata to end-users via the browser console.
if (!environment.production) {
  console.log('🚀 SIINADSEG Application Starting - Build:', new Date().toISOString());
  console.log('📅 Version:', environment.version || 'unknown');
  console.log('🌐 API URL:', environment.apiUrl);
  console.log('🔧 Environment: DEVELOPMENT');
}

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error('❌ Bootstrap Error:', err));
