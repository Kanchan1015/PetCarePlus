import React, { useEffect, useState } from "react";
import { fetchAdminUsers, updateUser } from "./usersApi";
import type { UserListItem, UpdateUserBody } from "./usersApi";
import { useParams, useNavigate } from "react-router-dom";

function AdminUserEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [user, setUser] = useState<UserListItem | null>(null);
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [accountStatus, setAccountStatus] = useState<"Active" | "Inactive">(
    "Active"
  );

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveMsg, setSaveMsg] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    const run = async () => {
      if (!id) return;
      setLoading(true);
      setError(null);

      let found: UserListItem | undefined;
      for (let p = 1; p <= 3 && !found; p++) {
        const res = await fetchAdminUsers({ page: p, pageSize: 20 });
        if (!res.ok) {
          setError(`Failed to load user: ${res.code}`);
          setLoading(false);
          return;
        }
        found = res.items.find((u) => u.id === id);
      }

      if (!active) return;

      if (!found) {
        setError("User not found.");
        setLoading(false);
        return;
      }

      setUser(found);
      setFullName(found.fullName ?? "");
      setEmail(found.email ?? "");
      setAccountStatus(found.isActive ? "Active" : "Inactive");
      setLoading(false);
    };
    run();
    return () => {
      active = false;
    };
  }, [id]);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    setSaving(true);
    setSaveMsg(null);
    setError(null);

    const body: UpdateUserBody = {};
    if (fullName.trim() && fullName !== (user?.fullName ?? ""))
      body.fullName = fullName.trim();
    if (email.trim() && email !== (user?.email ?? ""))
      body.email = email.trim().toLowerCase();
    if ((accountStatus === "Active") !== !!user?.isActive)
      body.accountStatus = accountStatus;

    if (Object.keys(body).length === 0) {
      setSaveMsg("Nothing to update.");
      setSaving(false);
      return;
    }

    const res = await updateUser(id, body);
    if (!res.ok) {
      if (res.code === "conflict") setError("That email is already in use.");
      else if (res.code === "not_found") setError("User not found.");
      else if (res.code === "forbidden")
        setError("You do not have permission.");
      else if (res.code === "unauthorized")
        setError("Session expired. Please log in again.");
      else setError("Update failed. Please try again.");
      setSaving(false);
      return;
    }

    setUser({
      id: res.user.id,
      fullName: res.user.fullName,
      email: res.user.email,
      roles: res.user.roles,
      isActive: res.user.isActive,
      accountStatus: res.user.accountStatus,
    });
    setSaveMsg("Saved!");
    setSaving(false);
  };

  if (loading)
    return (
      <section className="mx-auto w-full max-w-3xl rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm text-slate-600 text-lg font-medium">
        Loading...
      </section>
    );
  if (error)
    return (
      <section className="mx-auto w-full max-w-3xl rounded-2xl bg-red-50 p-6 ring-1 ring-red-200 shadow-sm text-red-800 text-lg font-medium">
        {error}
      </section>
    );
  if (!user) return <section className="p-6 text-slate-500">No user.</section>;

  return (
    <section className="mx-auto w-full max-w-3xl rounded-2xl bg-white p-6 sm:p-8 ring-1 ring-slate-200 shadow-sm">
      <button
        className="mb-6 inline-flex items-center gap-2 rounded-2xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-white focus-visible:ring focus-visible:ring-indigo-500"
        onClick={() => navigate(-1)}
      >
        ← Back
      </button>

      <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-slate-800 mb-6">
        Edit User
      </h1>

      <form className="space-y-6" onSubmit={onSubmit}>
        {saveMsg && (
          <div className="rounded-2xl bg-green-50 px-4 py-3 text-green-800 ring-1 ring-green-200">
            {saveMsg}
          </div>
        )}
        {error && (
          <div className="rounded-2xl bg-red-50 px-4 py-3 text-red-800 ring-1 ring-red-200">
            {error}
          </div>
        )}

        <div>
          <label className="mb-2 block text-sm font-medium text-slate-700">
            Full Name
          </label>
          <input
            className="w-full rounded-xl border-0 px-4 py-2 ring-1 ring-slate-200 text-slate-800 placeholder:text-slate-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Full name"
          />
        </div>

        <div>
          <label className="mb-2 block text-sm font-medium text-slate-700">
            Email
          </label>
          <input
            type="email"
            className="w-full rounded-xl border-0 px-4 py-2 ring-1 ring-slate-200 text-slate-800 placeholder:text-slate-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
            value={email}
            readOnly
          />
        </div>

        <div>
          <label className="mb-2 block text-sm font-medium text-slate-700">
            Account Status
          </label>
          <select
            className="w-full rounded-xl border-0 px-4 py-2 ring-1 ring-slate-200 text-slate-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
            value={accountStatus}
            onChange={(e) =>
              setAccountStatus(e.target.value as "Active" | "Inactive")
            }
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
          <p className="mt-1 text-xs text-slate-500">
            Note: you cannot deactivate your own admin account.
          </p>
        </div>

        <button
          disabled={saving}
          className="w-full inline-flex items-center justify-center rounded-2xl bg-indigo-600 px-6 py-3 font-semibold text-white hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-60"
        >
          {saving ? "Saving..." : "Save Changes"}
        </button>
      </form>
    </section>
  );
}

export default AdminUserEditPage;
