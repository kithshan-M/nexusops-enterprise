import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { getCurrentUser, login as loginRequest, logout as logoutRequest, refresh as refreshRequest, register as registerRequest, type AuthResponse, type Session } from "../api/authApi";

interface AuthContextValue {
  session: Session | null;
  loading: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (fullName: string, email: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
}
const AuthContext = createContext<AuthContextValue | null>(null);
const STORAGE_KEY = "nexusops.auth.session";

function saveSession(data: AuthResponse): Session {
  const session: Session = data;
  // Prototype storage only. For production, move refresh tokens to Secure, HttpOnly cookies.
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  return session;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null);
  const [loading, setLoading] = useState(true);

  const acceptResult = (result: { succeeded: boolean; message: string; data: AuthResponse | null }) => {
    if (!result.succeeded || !result.data) throw new Error(result.message || "Authentication failed.");
    const next = saveSession(result.data);
    setSession(next);
  };

  useEffect(() => {
    let active = true;
    const restore = async () => {
      const stored = sessionStorage.getItem(STORAGE_KEY);
      if (!stored) { if (active) setLoading(false); return; }
      try {
        const previous = JSON.parse(stored) as Session;
        let current = previous;
        if (new Date(previous.accessTokenExpiresAtUtc).getTime() <= Date.now() + 30000) {
          const refreshed = await refreshRequest(previous.refreshToken);
          if (!refreshed.succeeded || !refreshed.data) throw new Error("Session expired. Please sign in again.");
          current = refreshed.data;
          sessionStorage.setItem(STORAGE_KEY, JSON.stringify(current));
        }
        await getCurrentUser(current.accessToken);
        if (active) setSession(current);
      } catch {
        sessionStorage.removeItem(STORAGE_KEY);
        if (active) setSession(null);
      } finally {
        if (active) setLoading(false);
      }
    };
    void restore();
    return () => { active = false; };
  }, []);

  const signIn = useCallback(async (email: string, password: string) => {
    const result = await loginRequest(email, password);
    acceptResult(result);
  }, []);

  const signUp = useCallback(async (fullName: string, email: string, password: string) => {
    const result = await registerRequest(fullName, email, password);
    acceptResult(result);
  }, []);

  const signOut = useCallback(async () => {
    const current = session;
    sessionStorage.removeItem(STORAGE_KEY);
    setSession(null);
    if (current) {
      try { await logoutRequest(current.refreshToken, current.accessToken); }
      catch (error) { console.error("Server logout failed:", error); }
    }
  }, [session]);

  const value = useMemo(() => ({ session, loading, signIn, signUp, signOut }), [session, loading, signIn, signUp, signOut]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used inside AuthProvider.");
  return context;
}
