import React, { useEffect, useState } from "react";
import { getMe, updateMe } from "../admin/usersApi"; // or "../auth/meApi" if you split

function OwnerProfilePage() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");

  const [msg, setMsg] = useState<string | null>(null);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    (async () => {
      setLoading(true);
      setErr(null);
      try {
        const me = await getMe();
        if (!active) return;
        setFullName(me.fullName ?? "");
        setEmail(me.email ?? "");
      } catch {
        setErr("Failed to load your profile.");
      } finally {
        if (active) setLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, []);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setMsg(null);
    setErr(null);

    const body: { fullName?: string; email?: string } = {};
    if (fullName.trim()) body.fullName = fullName.trim();
    if (email.trim()) body.email = email.trim().toLowerCase();

    const res = await updateMe(body);
    if (!res.ok) {
      if (res.code === "unauthorized")
        setErr("Session expired. Please log in again.");
      else if (res.code === "conflict") setErr("That email is already in use.");
      else setErr("Could not save. Please try again.");
      setSaving(false);
      return;
    }
    setMsg("Profile updated.");
    setSaving(false);
  };

  if (loading)
    return (
      <section className="mx-auto w-full max-w-xl rounded-2xl bg-white p-6 ring-1 ring-slate-200 shadow-sm text-slate-600">
        Loading...
      </section>
    );

  if (err && !saving)
    return (
      <section className="mx-auto w-full max-w-xl rounded-2xl bg-red-50 p-6 ring-1 ring-red-200 text-red-800 shadow-sm">
        {err}
      </section>
    );

  return (
    <section className="mx-auto w-full max-w-xl rounded-2xl bg-white p-6 sm:p-8 ring-1 ring-slate-200 shadow-sm">
      <h1 className="mb-6 text-2xl sm:text-3xl font-bold tracking-tight text-slate-800">
        My Profile
      </h1>

      {msg && (
        <div className="mb-4 rounded-2xl bg-green-50 px-4 py-3 text-green-800 ring-1 ring-green-200">
          {msg}
        </div>
      )}
      {err && (
        <div className="mb-4 rounded-2xl bg-red-50 px-4 py-3 text-red-800 ring-1 ring-red-200">
          {err}
        </div>
      )}

      <form className="space-y-5" onSubmit={onSubmit} noValidate>
        <div>
          <label className="mb-2 block text-sm font-medium text-slate-700">
            Full name
          </label>
          <input
            className="w-full rounded-xl border-0 px-4 py-2 ring-1 ring-slate-200 text-slate-800 placeholder:text-slate-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Your full name"
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
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@example.com"
          />
        </div>

        <button
          disabled={saving}
          className="w-full inline-flex items-center justify-center rounded-2xl bg-indigo-600 px-6 py-3 font-semibold text-white hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-60"
        >
          {saving ? "Saving..." : "Save changes"}
        </button>
      </form>
    </section>
  );
}

export default OwnerProfilePage;
