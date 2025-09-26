import { Link, useNavigate } from "react-router-dom";
import { PawPrint, ArrowLeft } from "lucide-react";

const NotFound = () => {
  const navigate = useNavigate();

  return (
    <main className="relative min-h-screen bg-gradient-to-b from-white via-slate-50 to-white text-slate-800 flex items-center">
      {/* decorative blobs */}
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute -right-24 -top-24 h-72 w-72 rounded-full bg-indigo-200/40 blur-3xl" />
        <div className="absolute left-1/4 top-1/3 h-72 w-72 rounded-full bg-blue-200/40 blur-3xl" />
      </div>

      <div className="mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-xl rounded-3xl bg-white p-8 sm:p-10 ring-1 ring-slate-200 shadow-xl text-center">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-700">
            <PawPrint className="h-7 w-7" aria-hidden />
          </div>

          <p className="text-xs font-semibold uppercase tracking-wider text-indigo-700">
            Error 404
          </p>
          <h1 className="mt-2 text-2xl sm:text-3xl font-extrabold tracking-tight">
            Page not found
          </h1>
          <p className="mt-2 text-slate-600">
            The page you’re looking for doesn’t exist or may have moved.
          </p>

          <div className="mt-6 flex flex-col sm:flex-row gap-3 justify-center">
            <button
              onClick={() => navigate(-1)}
              className="inline-flex items-center justify-center gap-2 rounded-2xl border border-slate-300 px-5 py-3 text-sm font-medium hover:bg-white"
            >
              <ArrowLeft className="h-4 w-4" />
              Go back
            </button>
            <Link
              to="/"
              className="inline-flex items-center justify-center gap-2 rounded-2xl bg-indigo-600 px-5 py-3 text-sm font-semibold text-white hover:bg-indigo-700"
            >
              Home
            </Link>
          </div>

          <div className="mt-6 text-xs text-slate-500">
            Tip: check the URL or use the navigation to find what you need.
          </div>
        </div>
      </div>
    </main>
  );
};

export default NotFound;
