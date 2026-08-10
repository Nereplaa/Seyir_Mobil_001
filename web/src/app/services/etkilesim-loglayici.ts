import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

const API_BASE = 'http://localhost:5080/api';

// Madde 5 (Eren bey 2. geri bildirimi, 2026-08-06): web'deki HER tiklama Graylog'a loglanacak.
// Mimari kural geregi istemci Graylog'a DOGRUDAN baglanmiyor (sadece backend'i cagirir, backend
// kendi Serilog->Graylog hattina aktarir) - bkz. backend'deki POST /api/frontend-log.
@Injectable({ providedIn: 'root' })
export class EtkilesimLoglayici {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  // Fire-and-forget: loglama asla kullanici deneyimini bloklamamali/bozmamali - istek
  // basarisiz olsa bile (backend kapali, ag sorunu vb.) sessizce yutuluyor.
  logla(eylem: string, detay?: string): void {
    this.http
      .post(`${API_BASE}/frontend-log`, {
        eylem,
        detay: detay ?? null,
        sayfa: this.router.url,
      })
      .subscribe({ next: () => {}, error: () => {} });
  }
}
