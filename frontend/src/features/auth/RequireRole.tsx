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

  // Debug logging
  console.log("🔐 RequireRole Debug:");
  console.log("- Required roles:", roles);
  console.log("- Allowed roles (uppercase):", allowedRoles);
  console.log("- User's cached role:", localStorage.getItem(ROLE_KEY));
  console.log("- User's cached role (uppercase):", cachedRole);
  console.log("- Access granted:", allowedRoles.includes(cachedRole));
  console.log("- Current path:", location.pathname);

  if (!allowedRoles.includes(cachedRole)) {
    console.log("❌ Access denied - redirecting to:", redirectTo || "/unauthorized");
    return <Navigate to={redirectTo || "/unauthorized"} replace />;
  }

  console.log("✅ Access granted");
  return <>{children}</>;
}


