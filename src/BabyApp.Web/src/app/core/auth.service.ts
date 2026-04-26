import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

const TOKEN_KEY = 'babyapp_access_token';

export type LoginResponse = {
  accessToken: string;
  refreshToken?: string;
  expiresIn?: number;
  tokenType?: string;
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  constructor(private readonly http: HttpClient) {}

  isLoggedIn(): boolean {
    return !!this.token();
  }

  getToken(): string | null {
    return this.token();
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.token.set(null);
  }

  register(email: string, password: string) {
    const url = `${environment.apiBaseUrl}/register`;
    return this.http.post(url, { email, password });
  }

  login(email: string, password: string) {
    const url = `${environment.apiBaseUrl}/login`;
    return this.http.post<LoginResponse>(url, { email, password }).pipe(
      tap((res) => {
        localStorage.setItem(TOKEN_KEY, res.accessToken);
        this.token.set(res.accessToken);
      }),
    );
  }
}
