import { useEffect, useState } from "react";
import AdminLayout from "./AdminLayout";
import Breadcrumbs from "../../components/Breadcrumbs";
import { fetchVets, type VetItem } from "./vetsApi";
import { Link } from "react-router-dom";

export default function AdminVetListPage() {
  const [items, setItems] = useState<VetItem[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const canPrev = page > 1;
  const canNext = page < totalPages;

  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search.trim()), 300);
    return () => clearTimeout(t);
  }, [search]);

  useEffect(() => {
    let alive = true;
    (async () => {
      setLoading(true);
      setError(null);
      const res = await fetchVets({ page, pageSize, search: debouncedSearch });
      if (!alive) return;

      if (res.ok) {
        setItems(res.data.items);
        setTotal(res.data.total);
      } else {
        setError(res.detail || "Failed to load vets. Please try again.");
      }
      setLoading(false);
    })();
    return () => {
      alive = false;
    };
  }, [page, pageSize, debouncedSearch]);

  useEffect(() => {
    setPage(1);
  }, [debouncedSearch]);

  return (
    <AdminLayout>
      <Breadcrumbs
        items={[{ label: "Admin", to: "/admin" }, { label: "Vets" }]}
      />

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-4 gap-3">
        <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-800">
          Veterinary Doctors
        </h1>
        <Link
          to="/admin/vets/new"
          className="inline-flex items-center gap-2 text-sm rounded-2xl bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500"
        >
          + Create Vet
        </Link>
      </div>

      {/* Controls */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div className="relative w-full sm:w-64">
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by name or email…"
            className="w-full rounded-xl border-0 ring-1 ring-slate-200 px-3 py-2 text-slate-800 placeholder:text-slate-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
          />
          {search && (
            <button
              type="button"
              onClick={() => setSearch("")}
              className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md px-2 py-1 text-slate-500 hover:bg-slate-100 hover:text-slate-700 focus-visible:ring focus-visible:ring-indigo-500"
              aria-label="Clear search"
            >
              ✕
            </button>
          )}
        </div>

        <div className="flex items-center gap-2 text-sm text-slate-700">
          <div>
            Showing <span className="font-medium">{items.length}</span> of{" "}
            <span className="font-medium">{total}</span>
          </div>
          <label htmlFor="ps" className="ml-3">
            Rows per page:
          </label>
          <select
            id="ps"
            className="rounded-lg border-0 ring-1 ring-slate-200 px-2 py-1 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
            value={pageSize}
            onChange={(e) => {
              const next = Number(e.target.value);
              setPageSize(next);
              setPage(1);
            }}
          >
            {[5, 10, 20, 50].map((n) => (
              <option key={n} value={n}>
                {n}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Loading */}
      {loading && <CardSkeleton />}

      {/* Error */}
      {!loading && error && (
        <div className="rounded-2xl ring-1 ring-red-200 bg-red-50 p-4 text-red-800 shadow-sm">
          {error}
        </div>
      )}

      {/* Empty */}
      {!loading && !error && items.length === 0 && (
        <div className="rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm">
          <p className="text-slate-700">No vets found.</p>
          {debouncedSearch && (
            <p className="text-slate-500 text-sm mt-1">
              Try clearing the search.
            </p>
          )}
        </div>
      )}

      {/* Table */}
      {!loading && !error && items.length > 0 && (
        <div className="rounded-2xl bg-white ring-1 ring-slate-200 shadow-sm overflow-hidden">
          <table className="min-w-full text-left text-slate-700">
            <thead className="bg-slate-50 text-slate-600">
              <tr>
                <th className="px-6 py-3 font-medium">Full Name</th>
                <th className="px-6 py-3 font-medium">Email</th>
              </tr>
            </thead>
            <tbody>
              {items.map((v) => (
                <tr
                  key={v.id}
                  className="border-t border-slate-200/70 hover:bg-slate-50 transition"
                >
                  <td className="px-6 py-3">
                    <Link
                      to={`/admin/vets/${v.id}`}
                      className="text-indigo-700 hover:underline"
                    >
                      {v.fullName || "—"}
                    </Link>
                  </td>
                  <td className="px-6 py-3">{v.email || "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          <div className="flex items-center justify-between px-6 py-3 border-t border-slate-200/70 text-slate-700 text-sm">
            <div>
              Page <span className="font-medium">{page}</span> of{" "}
              <span className="font-medium">{totalPages}</span>
              {debouncedSearch && (
                <span className="ml-2 text-slate-500">(filtered)</span>
              )}
            </div>
            <div className="flex items-center gap-2">
              <button
                className="rounded-2xl border border-slate-300 bg-white px-3 py-1 hover:bg-white focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-50"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={!canPrev}
              >
                Prev
              </button>
              <button
                className="rounded-2xl border border-slate-300 bg-white px-3 py-1 hover:bg-white focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-50"
                onClick={() => setPage((p) => (canNext ? p + 1 : p))}
                disabled={!canNext}
              >
                Next
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  );
}

function CardSkeleton() {
  return (
    <div className="rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm mb-4">
      <p className="text-slate-600 mb-3">Loading vets…</p>
      <div className="space-y-2">
        <div className="animate-pulse h-4 bg-slate-200 rounded w-1/3" />
        <div className="animate-pulse h-4 bg-slate-200 rounded w-1/2" />
        <div className="animate-pulse h-4 bg-slate-200 rounded w-1/4" />
      </div>
    </div>
  );
}
