export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000/api";

export interface AuthUser {
  userId: string;
  fullName: string;
  email: string;
  roles: string[];
}
export interface AuthResponse extends AuthUser {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
}
export interface AuthResult {
  succeeded: boolean;
  message: string;
  data: AuthResponse | null;
}
export interface Session extends AuthResponse {}

export async function authRequest<T>(
  path: string,
  method: "GET" | "POST",
  body?: unknown,
  accessToken?: string
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  });

  const raw = await response.text();
  let payload: unknown = null;
  try { payload = raw ? JSON.parse(raw) : null; } catch { payload = raw; }

  if (!response.ok) {
    const message =
      typeof payload === "object" && payload !== null && "message" in payload
        ? String((payload as { message: unknown }).message)
        : `Request failed (${response.status} ${response.statusText})`;
    throw new Error(message);
  }
  return payload as T;
}

export const register = (fullName: string, email: string, password: string) =>
  authRequest<AuthResult>("/auth/register", "POST", { fullName, email, password });

export const login = (email: string, password: string) =>
  authRequest<AuthResult>("/auth/login", "POST", { email, password });

export const refresh = (refreshToken: string) =>
  authRequest<AuthResult>("/auth/refresh", "POST", { refreshToken });

export const logout = (refreshToken: string, accessToken: string) =>
  authRequest<AuthResult>("/auth/logout", "POST", { refreshToken }, accessToken);

export const getCurrentUser = (accessToken: string) =>
  authRequest<AuthUser>("/auth/me", "GET", undefined, accessToken);
