import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserSummaryDto, CreateUserRequestDto } from '../models/user.models';

const API_BASE = 'http://localhost:5080/api/users';

// Backend'deki Admin-only /api/users endpoint'lerine (2026-08-04'ten beri var, o zamandan
// beri hicbir istemci arayuzu yoktu) karsilik gelir.
@Service()
export class UserApi {
  private readonly http = inject(HttpClient);

  getKullanicilar(): Observable<UserSummaryDto[]> {
    return this.http.get<UserSummaryDto[]>(API_BASE);
  }

  createKullanici(request: CreateUserRequestDto): Observable<UserSummaryDto> {
    return this.http.post<UserSummaryDto>(API_BASE, request);
  }

  deleteKullanici(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/${id}`);
  }
}
