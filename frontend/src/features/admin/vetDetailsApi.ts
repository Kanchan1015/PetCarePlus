import { getToken } from "../auth/token";
const BASE = (import.meta.env?.VITE_API_BASE_URL as string) || undefined;

export type VetDetails = {
  id: string;
  fullName: string | null;
  email: string | null;
};

export type FetchVetOk = { ok: true; data: VetDetails };
export type FetchVetErr = {
  ok: false;
  code:
    | "unauthorized"
    | "forbidden"
    | "not_found"
    | "failed"
    | "network"
    | "timeout";
  detail?: string;
};

export async function fetchVetById(
  id: string,
  opts?: { timeoutMs?: number }
): Promise<FetchVetOk | FetchVetErr> {
  if (!BASE)
    return { ok: false, code: "failed", detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, code: "unauthorized" };

  const url = `${BASE}/admin/users/vets/${encodeURIComponent(id)}`;
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), opts?.timeoutMs ?? 10000);

  try {
    const res = await fetch(url, {
      headers: { Authorization: `Bearer ${token}` },
      signal: controller.signal,
    });
    clearTimeout(timer);

    if (res.status === 200)
      return { ok: true, data: (await res.json()) as VetDetails };
    if (res.status === 401) return { ok: false, code: "unauthorized" };
    if (res.status === 403) return { ok: false, code: "forbidden" };
    if (res.status === 404) return { ok: false, code: "not_found" };

    let txt = "";
    try {
      txt = await res.text();
    } catch {
      // JSON parsing failed, ignore
    }
    return { ok: false, code: "failed", detail: txt || `HTTP ${res.status}` };
  } catch (e: unknown) {
    if (e && typeof e === 'object' && 'name' in e && e.name === "AbortError") 
      return { ok: false, code: "timeout" };
    return { ok: false, code: "network", detail: String(e) };
  } finally {
    clearTimeout(timer);
  }
}
