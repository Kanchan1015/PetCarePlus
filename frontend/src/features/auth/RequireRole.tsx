import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";

const TOKEN_KEY = "APP_AT";
const ROLE_KEY = "APP_ROLE";

type RequireRoleProps = {
  children: ReactNode;
  roles: string[]; // acceptable roles, e.g., ["ADMIN"]
  redirectTo?: string; // optional redirect path if unauthorized
};

export default function RequireRole({ children, roles, redirectTo }: RequireRoleProps) {
  const location = useLocation();
  const token = localStorage.getItem(TOKEN_KEY);

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  const cachedRole = (localStorage.getItem(ROLE_KEY) || "").toUpperCase();
  const allowedRoles = roles.map(r => r.toUpperCase());

  if (!allowedRoles.includes(cachedRole)) {
    return <Navigate to={redirectTo || "/unauthorized"} replace />;
  }

  return <>{children}</>;
}


