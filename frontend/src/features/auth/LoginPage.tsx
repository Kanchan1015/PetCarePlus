import React, { useState } from "react";
import { motion } from "framer-motion";
import { PawPrint } from "lucide-react";

// ---- tiny helpers (you can move these to a shared auth/api module later)
const BASE = (import.meta as any).env?.VITE_API_BASE_URL as string | undefined;
const TOKEN_KEY = "APP_AT";
const setToken = (t: string) => localStorage.setItem(TOKEN_KEY, t);

// API result types
type LoginPayload = {
  accessToken: string;
  expiresAt?: string;
  user?: { id: string; role?: string; fullName?: string; email?: string };
};
type LoginOk = { ok: true; data: LoginPayload };
type LoginErr = {
  ok: false;
  code: "invalid" | "inactive" | "failed" | "network";
};

// call POST /api/auth/login
async function loginApi(body: {
  email: string;
  password: string;
}): Promise<LoginOk | LoginErr> {
  if (!BASE) return { ok: false, code: "failed" };
  try {
    const res = await fetch(`${BASE}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });

    if (res.status === 200) {
      const data = (await res.json()) as LoginPayload; // { accessToken, user: { role } }
      if (!data?.accessToken) return { ok: false, code: "failed" };
      return { ok: true, data };
    }
    if (res.status === 401) return { ok: false, code: "invalid" };
    if (res.status === 403) return { ok: false, code: "inactive" };
    return { ok: false, code: "failed" };
  } catch (e) {
    console.error("login error", e);
    return { ok: false, code: "network" };
  }
}

// ---- component
function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [remember, setRemember] = useState(false);

  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [emailError, setEmailError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setEmailError(null);

    // basic client checks
    const mail = email.trim().toLowerCase();
    const emailRe = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRe.test(mail)) {
      setEmailError("Enter a valid email address.");
      return;
    }
    if (password.length < 1) {
      setFormError("Enter your password.");
      return;
    }

    setSubmitting(true);
    try {
      const res = await loginApi({ email: mail, password });
      if (!res.ok) {
        if (res.code === "invalid") {
          setFormError("Email or password is incorrect.");
          return;
        }
        if (res.code === "inactive") {
          setFormError("This account is inactive. Please contact support.");
          return;
        }
        if (res.code === "network") {
          setFormError(
            "Cannot reach the server. Check your connection and try again."
          );
          return;
        }
        setFormError("Login failed. Please try again.");
        return;
      }

      // store token
      setToken(res.data.accessToken);
      localStorage.setItem(
        "APP_ROLE",
        (res.data.user?.role ?? "").toUpperCase()
      );

      if (remember) {
        // (optional) persist a refresh token later
      }

      // route by role directly from login payload
      const rawRole = res.data.user?.role ?? "";
      const role = String(rawRole).toUpperCase();

      // one-time log to verify
      console.log("login user.role =", rawRole);

      if (role === "ADMIN") {
        window.location.assign("/admin");
      } else if (role === "VET" || role === "VETERINARIAN") {
        window.location.assign("/vet");
      } else {
        window.location.assign("/owner"); // default for Owner/unknown
      }
    } catch (err) {
      console.error(err);
      setFormError("Unexpected error. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="relative min-h-screen bg-gradient-to-b from-white via-slate-50 to-white text-slate-800">
      {/* decorative blobs */}
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute -right-24 -top-24 h-72 w-72 rounded-full bg-indigo-200/40 blur-3xl" />
        <div className="absolute left-1/4 top-1/3 h-72 w-72 rounded-full bg-blue-200/40 blur-3xl" />
      </div>

      <header className="sticky top-0 z-40 border-b border-slate-200/60 backdrop-blur supports-[backdrop-filter]:bg-white/70">
        <div className="mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <a href="/" className="flex items-center gap-2 group">
            <PawPrint
              className="h-6 w-6 text-indigo-600 group-hover:scale-110 transition"
              aria-hidden
            />
            <span className="font-semibold tracking-tight">
              VetCare+ Hospital
            </span>
          </a>
          <div className="hidden sm:flex items-center gap-3">
            <a
              href="/register"
              className="px-4 py-2 text-sm font-semibold rounded-xl bg-indigo-600 text-white hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500"
            >
              Sign up
            </a>
            <a
              href="/"
              className="px-4 py-2 text-sm font-medium rounded-xl hover:bg-slate-100 focus-visible:ring focus-visible:ring-indigo-500"
            >
              Home
            </a>
          </div>
        </div>
      </header>

      <section className="relative">
        <div className="mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8 grid lg:grid-cols-2 gap-10 lg:gap-16 py-10 lg:py-16 items-center">
          {/* Left visual */}
          <motion.div
            initial={{ opacity: 0, scale: 0.98 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.6, delay: 0.1 }}
            className="hidden lg:block"
          >
            <div className="aspect-[4/3] w-full overflow-hidden rounded-3xl shadow-xl ring-1 ring-slate-200">
              <img
                src="./Petcare_cover_image.jpg"
                alt="Veterinary care illustration"
                className="h-full w-full object-cover"
                loading="eager"
              />
            </div>
          </motion.div>

          {/* Right form card */}
          <motion.div
            initial={{ opacity: 0, y: 14 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="w-full"
          >
            <div className="rounded-2xl bg-white p-6 sm:p-8 ring-1 ring-slate-200 shadow-xl">
              {/* brand */}
              <a href="/" className="flex items-center gap-2">
                <img
                  className="w-10 h-10 rounded-lg ring-1 ring-slate-200"
                  src="./logo.jpg"
                  alt="logo"
                />
                <span className="text-xl font-bold">PetCare+</span>
              </a>

              <h1 className="mt-6 text-2xl sm:text-3xl font-extrabold tracking-tight">
                Welcome back
              </h1>
              <p className="mt-2 text-slate-600">
                Log in to manage pets, book visits, and access records 24/7.
              </p>

              <form className="mt-6 space-y-5" onSubmit={onSubmit} noValidate>
                {formError && (
                  <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
                    {formError}
                  </div>
                )}

                <div>
                  <label
                    htmlFor="email"
                    className="block text-sm font-medium text-slate-700"
                  >
                    Email
                  </label>
                  <input
                    type="email"
                    id="email"
                    placeholder="you@example.com"
                    className="mt-2 block w-full rounded-xl border-slate-300 bg-white px-3 py-3 text-sm text-slate-900 ring-1 ring-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                  />
                  {emailError && (
                    <p className="mt-1 text-xs text-red-600">{emailError}</p>
                  )}
                </div>

                <div>
                  <label
                    htmlFor="password"
                    className="block text-sm font-medium text-slate-700"
                  >
                    Password
                  </label>
                  <input
                    type="password"
                    id="password"
                    placeholder="••••••••"
                    className="mt-2 block w-full rounded-xl border-slate-300 bg-white px-3 py-3 text-sm text-slate-900 ring-1 ring-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <label className="flex items-center gap-2">
                    <input
                      id="remember"
                      type="checkbox"
                      className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                      checked={remember}
                      onChange={(e) => setRemember(e.target.checked)}
                    />
                    <span className="text-sm text-slate-600">Remember me</span>
                  </label>

                  <a
                    href="#"
                    className="text-sm font-medium text-indigo-700 hover:underline"
                  >
                    Forgot password?
                  </a>
                </div>

                {/* Primary + Secondary CTAs */}
                <div className="flex flex-col sm:flex-row gap-3">
                  <button
                    type="submit"
                    disabled={submitting}
                    className="flex-1 rounded-2xl bg-indigo-600 px-5 py-3 text-white text-sm font-semibold hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-60"
                  >
                    {submitting ? "Logging in..." : "Login"}
                  </button>
                </div>

                <p className="text-sm text-slate-600 text-center">
                  New here?{" "}
                  <a
                    href="/register"
                    className="font-medium text-indigo-700 hover:underline"
                  >
                    Sign up
                  </a>
                </p>
              </form>
            </div>
          </motion.div>
        </div>
      </section>
    </main>
  );
}

export default LoginPage;
