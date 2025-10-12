import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";

const TOKEN_KEY = "APP_AT";
const ROLE_KEY = "APP_ROLE";
const BASE = (import.meta.env?.VITE_API_BASE_URL as string) || undefined;

export default function RequireAdmin({ children }: { children: ReactNode }) {
  const location = useLocation();
  const token = localStorage.getItem(TOKEN_KEY);

  // no token → bounce to login
  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // quick path: trust cached role from login
  const cached = (localStorage.getItem(ROLE_KEY) || "").toUpperCase();
  if (cached === "ADMIN") {
    return <>{children}</>;
  }

  // slow path: verify with backend once (handles refresh/direct links)
  return <AdminVerifier>{children}</AdminVerifier>;
}

function AdminVerifier({ children }: { children: ReactNode }) {
  const [allowed, setAllowed] = useState<null | boolean>(null);

  useEffect(() => {
    const controller = new AbortController();
    (async () => {
      try {
        const token = localStorage.getItem(TOKEN_KEY);
        if (!token || !BASE) return setAllowed(false);

        // try /users/me first; if 404, fallback to /auth/me
        const urls = [`${BASE}/users/me`, `${BASE}/auth/me`];
        let roles: string[] | null = null;

        for (const url of urls) {
          const res = await fetch(url, {
            headers: { Authorization: `Bearer ${token}` },
            signal: controller.signal,
          });
          if (res.status === 200) {
            const data = await res.json();
            // support both shapes: { roles: [...] } or { user: { role } } if you ever change it
            if (Array.isArray(data?.roles))
              roles = data.roles.map((r: string | { name?: string }) => 
                typeof r === 'string' ? r.toUpperCase() : String(r.name || r).toUpperCase()
              );
            else if (data?.user?.role)
              roles = [String(data.user.role).toUpperCase()];
            break;
          }
          if (res.status === 401) break; // not logged in
        }

        setAllowed(roles?.includes("ADMIN") ?? false);
      } catch {
        setAllowed(false);
      }
      return () => controller.abort();
    })();
  }, []);

  if (allowed === null) {
    // tiny loading placeholder; you can swap for a spinner later
    return (
      <div className="p-6 text-sm text-gray-500">Checking permission…</div>
    );
  }

  if (!allowed) {
    return <Navigate to="/owner" replace />; // or a dedicated "403" page
  }

  return <>{children}</>;
}
