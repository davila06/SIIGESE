import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';

console.log('🚀 SIINADSEG Application Starting - Build:', new Date().toISOString());

platformBrowserDynamic().bootstrapModule(AppModule)
  .then(() => {
    // Importar environment después del bootstrap para asegurar el reemplazo correcto
    import('./environments/environment').then(env => {
      console.log('📅 Version:', env.environment.version || 'unknown');
      console.log('🌐 API URL:', env.environment.apiUrl);
      console.log('🔧 Environment:', env.environment.production ? 'PRODUCTION' : 'DEVELOPMENT');
    });
  })
  .catch(err => console.error('❌ Bootstrap Error:', err));
