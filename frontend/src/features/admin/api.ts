import { getToken } from "../auth/token";

const BASE = (import.meta.env?.VITE_API_BASE_URL as string) || undefined;

export type CreateVetBody = {
  fullName: string;
  email: string;
  password: string;
};

export type CreateVetOk = {
  ok: true;
  data: { vetUserId: string; email: string };
};

export type CreateVetErr = {
  ok: false;
  code:
    | "unauthorized"
    | "forbidden"
    | "email_in_use"
    | "validation_failed"
    | "failed"
    | "network"
    | "timeout";
  detail?: string;
};

export async function createVet(
  body: CreateVetBody,
  opts?: { timeoutMs?: number }
): Promise<CreateVetOk | CreateVetErr> {
  if (!BASE)
    return { ok: false, code: "failed", detail: "VITE_API_BASE_URL not set" };

  const token = getToken();
  if (!token) return { ok: false, code: "unauthorized" };

  const timeoutMs = opts?.timeoutMs ?? 10_000;
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(`${BASE}/admin/users/vets`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
      signal: controller.signal,
    });

    clearTimeout(timer);

    if (res.status === 201) {
      const data = await res.json();
      return { ok: true, data };
    }

    if (res.status === 401) return { ok: false, code: "unauthorized" };
    if (res.status === 403) return { ok: false, code: "forbidden" };
    if (res.status === 409) return { ok: false, code: "email_in_use" };

    if (res.status === 400) {
      // try to read problem details
      let pd: { detail?: string } | null = null;
      try {
        pd = await res.json();
      } catch {
        // JSON parsing failed, ignore
      }
      const detail = (pd && typeof pd === 'object' && 'detail' in pd) 
        ? pd.detail || "Validation failed" 
        : "Validation failed";
      return { ok: false, code: "validation_failed", detail };
    }

    // other statuses
    let text = "";
    try {
      text = await res.text();
    } catch {
      // Text parsing failed, ignore
    }
    return { ok: false, code: "failed", detail: text || `HTTP ${res.status}` };
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'name' in e && e.name === "AbortError") 
      return { ok: false, code: "timeout" };
    return { ok: false, code: "network", detail: String(e) };
  } finally {
    clearTimeout(timer);
  }
}
