import React, { useState } from "react";
import { motion } from "framer-motion";

// tiny local helper — you can move this to src/features/auth/api.ts later
const BASE = (import.meta as any).env?.VITE_API_BASE_URL as string | undefined;

type RegisterOwnerResult =
  | { ok: true }
  | { ok: false; code: "email_in_use" | "failed" };

async function registerOwner(body: {
  fullName: string;
  email: string;
  password: string;
}): Promise<RegisterOwnerResult> {
  if (!BASE) {
    console.error("VITE_API_BASE_URL is not set");
    return { ok: false, code: "failed" };
  }
  const res = await fetch(`${BASE}/api/auth/register-owner`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (res.status === 201) return { ok: true };
  if (res.status === 409) return { ok: false, code: "email_in_use" };
  return { ok: false, code: "failed" };
}

function RegistrationPage() {
  // form state
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [agree, setAgree] = useState(false);

  // ui state
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setEmailError(null);
    setPasswordError(null);

    // --- client-side validation (mirror server rules) ---
    const name = fullName.trim();
    const mail = email.trim().toLowerCase();

    if (name.length < 2) {
      setFormError("Please enter your full name.");
      return;
    }
    const emailRe = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRe.test(mail)) {
      setEmailError("Enter a valid email address.");
      return;
    }
    // min 8 chars, include at least one letter and one number
    const policy = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;
    if (!policy.test(password)) {
      setPasswordError(
        "Password must be at least 8 characters and include a letter and a number."
      );
      return;
    }
    if (password !== confirm) {
      setPasswordError("Passwords do not match.");
      return;
    }
    if (!agree) {
      setFormError("You must accept the Terms and Conditions.");
      return;
    }

    // --- call API ---
    setSubmitting(true);
    const result = await registerOwner({
      fullName: name,
      email: mail,
      password,
    });
    setSubmitting(false);

    if (result.ok) {
      // simple success UX; you can replace with a toast
      alert("Account created. Please log in.");
      window.location.assign("/login");
      return;
    }

    if (result.code === "email_in_use") {
      setEmailError("An account with this email already exists.");
      return;
    }

    setFormError(
      "Registration failed. Please check your details and try again."
    );
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
            {/* optional: replace with <PawPrint className="h-6 w-6 text-indigo-600" /> if you're importing icons */}
            <img
              src="./logo.jpg"
              alt="logo"
              className="h-8 w-8 rounded-lg ring-1 ring-slate-200"
            />
            <span className="font-semibold tracking-tight">
              VetCare+ Hospital
            </span>
          </a>
          <div className="hidden sm:flex items-center gap-3">
            <a
              href="/login"
              className="px-4 py-2 text-sm font-medium rounded-xl hover:bg-slate-100 focus-visible:ring focus-visible:ring-indigo-500"
            >
              Log in
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
                Create an account
              </h1>
              <p className="mt-2 text-slate-600">
                Join VetCare+ to book visits, manage pets, and view records
                anytime.
              </p>

              <form className="mt-6 space-y-5" onSubmit={onSubmit} noValidate>
                {formError && (
                  <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
                    {formError}
                  </div>
                )}

                <div>
                  <label
                    htmlFor="full-name"
                    className="block text-sm font-medium text-slate-700"
                  >
                    Full name
                  </label>
                  <input
                    type="text"
                    id="full-name"
                    placeholder="Full Name"
                    className="mt-2 block w-full rounded-xl bg-white px-3 py-3 text-sm text-slate-900 ring-1 ring-slate-200 border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    required
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                  />
                </div>

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
                    className="mt-2 block w-full rounded-xl bg-white px-3 py-3 text-sm text-slate-900 ring-1 ring-slate-200 border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
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
                    className="mt-2 block w-full rounded-xl bg-white px-3 py-3 text-sm text-slate-900 ring-1 ring-slate-200 border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                </div>

                <div>
                  <label
                    htmlFor="confirm-password"
                    className="block text-sm font-medium text-slate-700"
                  >
                    Confirm password
                  </label>
                  <input
                    type="password"
                    id="confirm-password"
                    placeholder="••••••••"
                    className="mt-2 block w-full rounded-xl bg-white px-3 py-3 text-sm text-slate-900 ring-1 ring-slate-200 border-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    required
                    value={confirm}
                    onChange={(e) => setConfirm(e.target.value)}
                  />
                  {passwordError && (
                    <p className="mt-1 text-xs text-red-600">{passwordError}</p>
                  )}
                </div>

                <div className="flex items-start">
                  <input
                    id="terms"
                    type="checkbox"
                    className="mt-1 h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                    checked={agree}
                    onChange={(e) => setAgree(e.target.checked)}
                    required
                  />
                  <label
                    htmlFor="terms"
                    className="ml-2 text-sm text-slate-600"
                  >
                    I accept the{" "}
                    <a
                      href="#"
                      className="font-medium text-indigo-700 hover:underline"
                    >
                      Terms and Conditions
                    </a>
                  </label>
                </div>

                {/* Primary + Secondary CTAs */}
                <div className="flex flex-col sm:flex-row gap-3">
                  <button
                    type="submit"
                    disabled={submitting}
                    className="flex-1 rounded-2xl bg-indigo-600 px-5 py-3 text-white text-sm font-semibold hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500 disabled:opacity-60"
                  >
                    {submitting ? "Creating account..." : "Create account"}
                  </button>
                </div>

                <p className="text-sm text-slate-600 text-center">
                  Already have an account?{" "}
                  <a
                    href="/login"
                    className="font-medium text-indigo-700 hover:underline"
                  >
                    Log in
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

export default RegistrationPage;
