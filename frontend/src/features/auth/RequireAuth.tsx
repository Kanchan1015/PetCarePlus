import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";

const TOKEN_KEY = "APP_AT";

export default function RequireAuth({ children }: { children: ReactNode }) {
  const location = useLocation();
  const token = localStorage.getItem(TOKEN_KEY);

  if (!token) {
    // no token → bounce to login and remember where they were going
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  return <>{children}</>;
}
