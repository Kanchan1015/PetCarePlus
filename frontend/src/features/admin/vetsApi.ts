import { getToken } from "../auth/token";

const BASE = (import.meta.env?.VITE_API_BASE_URL as string) || undefined;

export type VetItem = {
  id: string;
  fullName: string | null;
  email: string | null;
};
export type VetList = {
  items: VetItem[];
  total: number;
  page: number;
  pageSize: number;
};

export type FetchVetsOk = { ok: true; data: VetList };
export type FetchVetsErr = {
  ok: false;
  code: "unauthorized" | "forbidden" | "failed" | "network" | "timeout";
  detail?: string;
};

export async function fetchVets(
  params: { search?: string; page?: number; pageSize?: number } = {},
  opts?: { timeoutMs?: number }
): Promise<FetchVetsOk | FetchVetsErr> {
  if (!BASE)
    return { ok: false, code: "failed", detail: "VITE_API_BASE_URL not set" };

  const token = getToken();
  if (!token) return { ok: false, code: "unauthorized" };

  const q = new URLSearchParams();
  if (params.search) q.set("search", params.search);
  if (params.page) q.set("page", String(params.page));
  if (params.pageSize) q.set("pageSize", String(params.pageSize));

  const url = `${BASE}/admin/users/vets${q.toString() ? `?${q}` : ""}`;

  const timeoutMs = opts?.timeoutMs ?? 10_000;
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(url, {
      headers: { Authorization: `Bearer ${token}` },
      signal: controller.signal,
    });
    clearTimeout(timer);

    if (res.status === 200) {
      const data = (await res.json()) as VetList;
      return { ok: true, data };
    }
    if (res.status === 401) return { ok: false, code: "unauthorized" };
    if (res.status === 403) return { ok: false, code: "forbidden" };

    let text = "";
    try {
      text = await res.text();
    } catch {
      // JSON parsing failed, ignore
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
