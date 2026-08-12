import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    // Coklu dil altyapisi (feedback_001) - ceviri dosyalari public/i18n/*.json'dan HttpClient
    // ile yukleniyor (public/ Angular'da site kokune eslenir, yani /i18n/tr.json). Baslangic
    // dili burada DEGIL, Dil servisinin baslat()'inda secilir (localStorage'daki kullanici
    // tercihini okumak icin) - app.ts constructor'inda cagriliyor.
    provideTranslateService({
      loader: provideTranslateHttpLoader({ prefix: '/i18n/', suffix: '.json' }),
      fallbackLang: 'tr',
      lang: 'tr',
    }),
  ]
};
