import { Service, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequestDto, LoginResponseDto } from '../models/auth.models';

const API_BASE = 'http://localhost:5080/api/auth';
const TOKEN_KEY = 'seyir_token';
const USERNAME_KEY = 'seyir_username';
const ROLE_KEY = 'seyir_role';

@Service()
export class Auth {
  private readonly http = inject(HttpClient);

  readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly username = signal<string | null>(localStorage.getItem(USERNAME_KEY));
  readonly role = signal<string | null>(localStorage.getItem(ROLE_KEY));
  readonly girisYapilmisMi = computed(() => !!this.token());

  login(username: string, password: string): Observable<LoginResponseDto> {
    const request: LoginRequestDto = { username, password };
    return this.http
      .post<LoginResponseDto>(`${API_BASE}/login`, request)
      .pipe(tap((yanit) => this.oturumuKaydet(yanit)));
  }

  logout(): void {
    // Sunucu tarafinda da oturumu sonlandirmayi dene (best-effort) - basarisiz olsa bile
    // yerel oturum zaten temizlenir, kullanici tekrar login ekranina doner.
    this.http.post(`${API_BASE}/logout`, {}).subscribe({ next: () => {}, error: () => {} });
    this.oturumuTemizle();
  }

  // 401 durumunda (interceptor tarafindan cagrilir) sunucuya tekrar istek atmadan sadece
  // yerel oturumu temizler - "logout()"tan farkli olarak zaten gecersiz olan token'la
  // /api/auth/logout'u cagirmanin bir anlami yok.
  oturumuTemizle(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USERNAME_KEY);
    localStorage.removeItem(ROLE_KEY);
    this.token.set(null);
    this.username.set(null);
    this.role.set(null);
  }

  private oturumuKaydet(yanit: LoginResponseDto): void {
    localStorage.setItem(TOKEN_KEY, yanit.token);
    localStorage.setItem(USERNAME_KEY, yanit.username);
    localStorage.setItem(ROLE_KEY, yanit.role);
    this.token.set(yanit.token);
    this.username.set(yanit.username);
    this.role.set(yanit.role);
  }
}
