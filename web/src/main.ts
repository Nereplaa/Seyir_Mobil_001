import { bootstrapApplication } from '@angular/platform-browser';
import config from 'devextreme/core/config';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { licenseKey } from './devextreme-license';

// devextreme-license.ts .gitignore'da (kisisel hesaba bagli anahtar, resmi "devextreme-license"
// CLI araciyla uretildi - bkz. AI_NOTES/decisions.md). Anahtar henuz uretilmemisse (dosya yoksa)
// "npm run devextreme:license" (package.json'daki script) calistirilmali.
if (licenseKey) {
  config({ licenseKey });
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
