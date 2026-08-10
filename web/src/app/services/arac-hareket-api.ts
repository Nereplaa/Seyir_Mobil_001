import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AracHareketDto,
  AracPlakaLookupDto,
  AracHareketSinirlarDto,
  CreateAracHareketRequestDto,
  RaporTopluRequestDto,
  AracRaporSonucuDto,
  AracHareketDetayRaporSatiriDto,
  ImportHamSatirDto,
  ImportOnizlemeYanitiDto,
  ImportOnaylaSatiriDto,
  ImportOnaylaSonucDto,
} from '../models/arac-hareket.models';

const API_BASE = 'http://localhost:5080/api/arac-hareketleri';

@Service()
export class AracHareketApi {
  private readonly http = inject(HttpClient);

  getTumHareketler(): Observable<AracHareketDto[]> {
    return this.http.get<AracHareketDto[]>(API_BASE);
  }

  getPlakalar(): Observable<AracPlakaLookupDto[]> {
    return this.http.get<AracPlakaLookupDto[]>(`${API_BASE}/plakalar`);
  }

  getSinirlar(plaka: string, tarih: string): Observable<AracHareketSinirlarDto> {
    const params = { plaka, tarih };
    return this.http.get<AracHareketSinirlarDto>(`${API_BASE}/sinirlar`, { params });
  }

  createHareket(request: CreateAracHareketRequestDto): Observable<AracHareketDto> {
    return this.http.post<AracHareketDto>(API_BASE, request);
  }

  deleteHareket(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/${id}`);
  }

  getRaporToplu(request: RaporTopluRequestDto): Observable<AracRaporSonucuDto[]> {
    return this.http.post<AracRaporSonucuDto[]>(`${API_BASE}/rapor-toplu`, request);
  }

  getDetayRaporu(request: RaporTopluRequestDto): Observable<AracHareketDetayRaporSatiriDto[]> {
    return this.http.post<AracHareketDetayRaporSatiriDto[]>(`${API_BASE}/rapor-detay`, request);
  }

  importOnizle(dosya: File): Observable<ImportOnizlemeYanitiDto> {
    const formData = new FormData();
    formData.append('dosya', dosya);
    return this.http.post<ImportOnizlemeYanitiDto>(`${API_BASE}/import-onizleme`, formData);
  }

  importYenidenDogrula(satirlar: ImportHamSatirDto[]): Observable<ImportOnizlemeYanitiDto> {
    return this.http.post<ImportOnizlemeYanitiDto>(`${API_BASE}/import-yeniden-dogrula`, { satirlar });
  }

  importOnayla(satirlar: ImportOnaylaSatiriDto[]): Observable<ImportOnaylaSonucDto> {
    return this.http.post<ImportOnaylaSonucDto>(`${API_BASE}/import-onayla`, { satirlar });
  }

  importSablonIndir(): Observable<Blob> {
    return this.http.get(`${API_BASE}/import-sablon`, { responseType: 'blob' });
  }
}
