import { useEffect, useState } from "react";
import AdminLayout from "./AdminLayout";
import Breadcrumbs from "../../components/Breadcrumbs";
import { fetchAdminUsers, setUserActive, type UserListItem } from "./usersApi";
import { Link, useNavigate } from "react-router-dom";

export default function AdminUsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    let active = true;
    const ctrl = new AbortController();

    async function load() {
      setLoading(true);
      setError(null);
      const res = await fetchAdminUsers({
        page: 1,
        pageSize: 10,
        signal: ctrl.signal,
      });
      if (!active) return;

      if (!res.ok) {
        setError(`Failed to load users: ${res.code}`);
        setUsers([]);
      } else {
        setUsers(res.items);
      }
      setLoading(false);
    }
    load();

    return () => {
      active = false;
      ctrl.abort();
    };
  }, []);

  async function onToggleActive(u: UserListItem) {
    const nextActive = !u.isActive;
    const verb = nextActive ? "activate" : "deactivate";
    if (
      !confirm(`Are you sure you want to ${verb} "${u.fullName ?? u.email}"?`)
    )
      return;

    try {
      setBusyId(u.id);

      // optimistic update
      setUsers((prev) =>
        prev.map((x) =>
          x.id === u.id
            ? {
                ...x,
                isActive: nextActive,
                accountStatus: nextActive ? "Active" : "Inactive",
              }
            : x
        )
      );

      const res = await setUserActive(u.id, nextActive);
      if (!res.ok) {
        // revert on failure
        setUsers((prev) =>
          prev.map((x) =>
            x.id === u.id
              ? {
                  ...x,
                  isActive: !nextActive,
                  accountStatus: !nextActive ? "Active" : "Inactive",
                }
              : x
          )
        );
        alert(
          res.code === "forbidden"
            ? "You are not allowed to do that."
            : res.code === "unauthorized"
            ? "Session expired. Please log in again."
            : res.code === "conflict"
            ? "Update conflict."
            : "Failed to update status."
        );
      }
    } finally {
      setBusyId(null);
    }
  }

  return (
    <AdminLayout>
      <Breadcrumbs
        items={[{ label: "Admin", to: "/admin" }, { label: "Users" }]}
      />

      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-4 gap-3">
        <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-800">
          Users
        </h1>
        <div className="flex gap-3">
          <button
            onClick={() => navigate(-1)}
            className="inline-flex items-center gap-2 rounded-2xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-white focus-visible:ring focus-visible:ring-indigo-500"
          >
            ← Back
          </button>
        </div>
      </div>

      {/* Loading */}
      {loading && (
        <div className="rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm mb-4">
          <p className="text-slate-600">Loading users…</p>
        </div>
      )}

      {/* Error */}
      {!loading && error && (
        <div className="rounded-2xl ring-1 ring-red-200 bg-red-50 p-4 text-red-800 shadow-sm">
          {error}
        </div>
      )}

      {/* Empty */}
      {!loading && !error && users.length === 0 && (
        <div className="rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm">
          <p className="text-slate-700">No users found.</p>
        </div>
      )}

      {/* Table */}
      {!loading && !error && users.length > 0 && (
        <div className="rounded-2xl bg-white ring-1 ring-slate-200 shadow-sm overflow-hidden">
          <table className="min-w-full text-left text-slate-700">
            <thead className="bg-slate-50 text-slate-600">
              <tr>
                <th className="px-6 py-3 font-medium">Full Name</th>
                <th className="px-6 py-3 font-medium">Email</th>
                <th className="px-6 py-3 font-medium">Roles</th>
                <th className="px-6 py-3 font-medium">Status</th>
                <th className="px-6 py-3 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr
                  key={u.id}
                  className="border-t border-slate-200/70 hover:bg-slate-50 transition"
                >
                  <td className="px-6 py-3">{u.fullName || "—"}</td>
                  <td className="px-6 py-3">{u.email || "—"}</td>
                  <td className="px-6 py-3">{u.roles.join(", ")}</td>
                  <td className="px-6 py-3">
                    <span
                      className={`inline-flex items-center gap-1 rounded-full px-2 py-1 text-xs font-medium ring-1 ${
                        u.isActive
                          ? "bg-green-50 text-green-700 ring-green-200"
                          : "bg-red-50 text-red-700 ring-red-200"
                      }`}
                    >
                      <span
                        className={`h-1.5 w-1.5 rounded-full ${
                          u.isActive ? "bg-green-600" : "bg-red-600"
                        }`}
                      />
                      {u.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className="px-6 py-3">
                    <div className="flex flex-wrap gap-2">
                      <Link
                        to={`/admin/users/${u.id}`}
                        className="inline-flex items-center rounded-2xl border border-slate-300 bg-white px-3 py-1 text-sm font-medium text-slate-700 hover:bg-white focus-visible:ring focus-visible:ring-indigo-500"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => onToggleActive(u)}
                        disabled={busyId === u.id}
                        className="inline-flex items-center rounded-2xl bg-indigo-600 px-3 py-1 text-sm font-semibold text-white hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-60"
                      >
                        {busyId === u.id
                          ? "Saving..."
                          : u.isActive
                          ? "Deactivate"
                          : "Activate"}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </AdminLayout>
  );
}
