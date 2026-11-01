import { useState, type FormEvent } from "react";
import { Activity, ArrowRight, Command, Eye, EyeOff, LockKeyhole, Mail, ShieldCheck, UserRound } from "lucide-react";
import Dashboard from "./Dashboard";
import { AuthProvider, useAuth } from "./context/AuthContext";
import "./App.css";

function AuthScreen() {
  const { session, loading, signIn, signUp, signOut } = useAuth();
  const [mode, setMode] = useState<"login" | "register">("login");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setBusy(true);
    try {
      if (mode === "login") await signIn(email.trim(), password);
      else await signUp(fullName.trim(), email.trim(), password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong. Please try again.");
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <div className="auth-loading"><div className="auth-spinner" />Checking your session...</div>;
  if (session) return <Dashboard user={session} onLogout={() => void signOut()} />;

  return (
    <main className="auth-page">
      <div className="auth-decoration auth-decoration-one" />
      <div className="auth-decoration auth-decoration-two" />
      <section className="auth-card">
        <div className="auth-brand"><span className="auth-brand-icon"><Command size={22} /></span><span>Nexus<span className="brand-accent">Ops</span></span></div>
        <div className="auth-heading">
          <div className="auth-eyebrow"><Activity size={14} /> ENTERPRISE WORKSPACE</div>
          <h1>{mode === "login" ? "Welcome back" : "Create your account"}</h1>
          <p>{mode === "login" ? "Sign in to continue to your NexusOps workspace." : "Register to get started with your workspace."}</p>
        </div>
        <div className="auth-tabs">
          <button className={mode === "login" ? "selected" : ""} onClick={() => { setMode("login"); setError(""); }}>Sign in</button>
          <button className={mode === "register" ? "selected" : ""} onClick={() => { setMode("register"); setError(""); }}>Create account</button>
        </div>
        <form className="auth-form" onSubmit={handleSubmit}>
          {mode === "register" && <label>Full name<div className="auth-input"><UserRound size={17}/><input value={fullName} onChange={e => setFullName(e.target.value)} placeholder="Your full name" minLength={2} maxLength={100} required /></div></label>}
          <label>Email address<div className="auth-input"><Mail size={17}/><input type="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="you@company.com" maxLength={256} required /></div></label>
          <label>Password<div className="auth-input"><LockKeyhole size={17}/><input type={showPassword ? "text" : "password"} value={password} onChange={e => setPassword(e.target.value)} placeholder={mode === "register" ? "At least 12 characters" : "Enter your password"} minLength={mode === "register" ? 12 : 1} maxLength={128} required/><button type="button" className="password-toggle" onClick={() => setShowPassword(v => !v)} aria-label={showPassword ? "Hide password" : "Show password"}>{showPassword ? <EyeOff size={17}/> : <Eye size={17}/>}</button></div></label>
          {error && <div className="auth-error" role="alert">{error}</div>}
          <button className="auth-submit" type="submit" disabled={busy}>{busy ? "Please wait..." : mode === "login" ? "Sign in to workspace" : "Create account"} <ArrowRight size={17}/></button>
        </form>
        <div className="auth-security"><ShieldCheck size={16}/><span>Protected by NexusOps Identity</span></div>
        <p className="auth-switch">{mode === "login" ? "New to NexusOps?" : "Already have an account?"} <button onClick={() => { setMode(mode === "login" ? "register" : "login"); setError(""); }}>{mode === "login" ? "Create an account" : "Sign in"}</button></p>
      </section>
      <footer className="auth-footer">© 2026 NexusOps · Enterprise Workspace Platform</footer>
    </main>
  );
}

export default function App() {
  return <AuthProvider><AuthScreen /></AuthProvider>;
}
