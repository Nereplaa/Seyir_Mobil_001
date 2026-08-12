import { Routes } from '@angular/router';
import { Liste } from './pages/liste/liste';
import { Rapor } from './pages/rapor/rapor';
import { Login } from './pages/login/login';
import { SifremiUnuttum } from './pages/sifremi-unuttum/sifremi-unuttum';
import { SifreSifirla } from './pages/sifre-sifirla/sifre-sifirla';
import { AdminPaneli } from './pages/admin-paneli/admin-paneli';
import { Import } from './pages/import/import';
import { authGuard } from './guards/auth-guard';
import { roleGuard } from './guards/role-guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'sifremi-unuttum', component: SifremiUnuttum },
  { path: 'sifre-sifirla', component: SifreSifirla },
  { path: 'admin', component: AdminPaneli, canActivate: [roleGuard(['Admin'])] },
  { path: '', component: Liste, canActivate: [authGuard] },
  { path: 'rapor', component: Rapor, canActivate: [authGuard] },
  { path: 'import', component: Import, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];
