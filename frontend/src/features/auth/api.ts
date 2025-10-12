const BASE = import.meta.env.VITE_API_BASE_URL as string;

export type RegisterOwnerBody = {
  fullName: string;
  email: string;
  password: string;
};

export async function registerOwner(body: RegisterOwnerBody) {
  const res = await fetch(`${BASE}/auth/register-owner`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  // Map your controller’s responses
  if (res.status === 201) return { ok: true as const };

  if (res.status === 409) {
    return { ok: false as const, code: "email_in_use" };
  }

  // Generic client-facing error
  return { ok: false as const, code: "failed" };
}
