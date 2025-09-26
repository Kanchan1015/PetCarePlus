import { Link, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import AdminLayout from "./AdminLayout";
import Breadcrumbs from "../../components/Breadcrumbs";
import { fetchVetById, type VetDetails } from "./vetDetailsApi";

export default function AdminVetDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const [data, setData] = useState<VetDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;
    (async () => {
      if (!id) return;
      setLoading(true);
      setError(null);
      const res = await fetchVetById(id);
      if (!alive) return;

      if (res.ok) setData(res.data);
      else {
        if (res.code === "not_found") setError("Vet not found.");
        else if (res.code === "unauthorized")
          setError("Session expired. Please log in again.");
        else if (res.code === "forbidden")
          setError("You don’t have permission to view this page.");
        else if (res.code === "timeout")
          setError("The request timed out. Try again.");
        else if (res.code === "network")
          setError("Cannot reach the server. Check your connection.");
        else setError(res.detail || "Failed to load vet. Please try again.");
      }
      setLoading(false);
    })();
    return () => {
      alive = false;
    };
  }, [id]);

  return (
    <AdminLayout>
      <Breadcrumbs
        items={[
          { label: "Admin", to: "/admin" },
          { label: "Vets", to: "/admin/vets" },
          { label: "Details" },
        ]}
      />

      <div className="flex items-center justify-between mb-4 gap-3">
        <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-800">
          Vet Details
        </h1>
        <Link
          to="/admin/vets"
          className="inline-flex items-center gap-2 rounded-2xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-white focus-visible:ring focus-visible:ring-indigo-500"
        >
          ← Back to list
        </Link>
      </div>

      {loading && (
        <div className="rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm">
          <p className="text-slate-600">Loading vet…</p>
        </div>
      )}

      {!loading && error && (
        <div className="rounded-2xl ring-1 ring-red-200 bg-red-50 px-4 py-3 text-red-800 shadow-sm">
          {error}
        </div>
      )}

      {!loading && !error && data && (
        <div className="rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm">
          <div className="grid sm:grid-cols-2 gap-6">
            <div>
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Full name
              </div>
              <div className="text-lg text-slate-800">
                {data.fullName || "—"}
              </div>
            </div>
            <div>
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Email
              </div>
              <div className="text-slate-800">{data.email || "—"}</div>
            </div>
          </div>

          {/* future: actions (reset password, deactivate, etc.) */}
        </div>
      )}
    </AdminLayout>
  );
}
