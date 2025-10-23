// src/features/admin/inventoryApi.ts
import { getToken } from "../auth/token";

const BASE = (import.meta.env?.VITE_API_BASE_URL as string) || undefined;

export type InventoryItem = {
  id: string;
  name: string;
  quantity: number;
  category: string;
  supplier: string;
  expiryDate?: string | null;
  description?: string | null;
  photoUrl?: string | null;
};

export type CreateInventoryDto = {
  name: string;
  quantity: number;
  category: string;
  supplier: string;
  expiryDate?: string | null;
  description?: string | null;
  photoUrl?: string | null;
};

export type UpdateInventoryDto = {
  name: string;
  quantity: number;
  category: string;
  supplier: string;
  expiryDate?: string | null;
  description?: string | null;
  photoUrl?: string | null;
};

export async function fetchInventory(): Promise<{ ok: true; data: InventoryItem[] } | { ok: false; detail?: string }> {
  if (!BASE) return { ok: false, detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, detail: "Unauthorized" };
  try {
    const res = await fetch(`${BASE}/Inventory`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) return { ok: false, detail: await res.text() };
    const data = await res.json();
    return { ok: true, data };
  } catch (e) {
    return { ok: false, detail: String(e) };
  }
}

export async function createInventoryItem(body: CreateInventoryDto) {
  if (!BASE) return { ok: false, detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, detail: "Unauthorized" };
  try {
    const res = await fetch(`${BASE}/Inventory`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: JSON.stringify(body),
    });
    if (res.status === 201) return { ok: true };
    return { ok: false, detail: await res.text() };
  } catch (e) {
    return { ok: false, detail: String(e) };
  }
}

export async function updateInventoryItem(id: string, body: UpdateInventoryDto) {
  if (!BASE) return { ok: false, detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, detail: "Unauthorized" };
  try {
    const res = await fetch(`${BASE}/Inventory/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: JSON.stringify(body),
    });
    if (res.ok) return { ok: true };
    return { ok: false, detail: await res.text() };
  } catch (e) {
    return { ok: false, detail: String(e) };
  }
}

export async function deleteInventoryItem(id: string) {
  if (!BASE) return { ok: false, detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, detail: "Unauthorized" };
  try {
    const res = await fetch(`${BASE}/Inventory/${id}`, {
      method: "DELETE",
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.ok) return { ok: true };
    return { ok: false, detail: await res.text() };
  } catch (e) {
    return { ok: false, detail: String(e) };
  }
}

export async function uploadInventoryPhoto(file: File): Promise<{ ok: true; url: string } | { ok: false; detail: string }> {
  const BASE = (import.meta.env?.VITE_API_BASE_URL as string) || undefined;
  if (!BASE) return { ok: false, detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, detail: "Unauthorized" };
  const formData = new FormData();
  formData.append("file", file);
  try {
    const res = await fetch(`${BASE}/Inventory/upload-photo`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: formData,
    });
    if (!res.ok) return { ok: false, detail: await res.text() };
    const data = await res.json();
    return { ok: true, url: data.url };
  } catch (e) {
    return { ok: false, detail: String(e) };
  }
}

export async function searchInventory(query: string): Promise<{ ok: true; data: InventoryItem[] } | { ok: false; detail?: string }> {
  if (!BASE) return { ok: false, detail: "VITE_API_BASE_URL not set" };
  const token = getToken();
  if (!token) return { ok: false, detail: "Unauthorized" };
  try {
    const res = await fetch(`${BASE}/Inventory/search?q=${encodeURIComponent(query)}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) return { ok: false, detail: await res.text() };
    const data = await res.json();
    return { ok: true, data };
  } catch (e) {
    return { ok: false, detail: String(e) };
  }
}
