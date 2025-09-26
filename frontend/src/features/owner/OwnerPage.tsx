import { Link } from "react-router-dom";

const OwnerPage = () => {
  return (
    <main className="max-w-5xl mx-auto px-4 py-10 space-y-8">
      {/* Header */}
      <header>
        <h1 className="text-3xl font-bold tracking-tight text-slate-800">
          Owner Dashboard
        </h1>
        <p className="mt-1 text-slate-600">
          Manage your pets, appointments, and profile
        </p>
      </header>

      {/* Navigation cards */}
      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <li>
          <Link
            to="/owner/pets"
            className="block rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm hover:shadow-md hover:bg-slate-50 focus-visible:ring focus-visible:ring-indigo-500 transition"
          >
            <h2 className="font-semibold text-slate-800">My Pets</h2>
            <p className="mt-1 text-sm text-slate-600">
              View and manage your pets
            </p>
          </Link>
        </li>
        <li className="rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm text-slate-700">
          <h2 className="font-semibold text-slate-800">Medical History</h2>
          <p className="mt-1 text-sm text-slate-600">Read-only access</p>
        </li>
        <li className="rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm text-slate-700">
          <h2 className="font-semibold text-slate-800">Request Appointment</h2>
          <p className="mt-1 text-sm text-slate-600">
            Book your next visit easily
          </p>
        </li>
        <li className="rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm text-slate-700">
          <h2 className="font-semibold text-slate-800">My Appointments</h2>
          <p className="mt-1 text-sm text-slate-600">
            Track upcoming and past bookings
          </p>
        </li>
      </ul>

      {/* Footer link */}
      <div>
        <a
          href="/owner/profile"
          className="inline-flex items-center rounded-2xl border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 focus-visible:ring focus-visible:ring-indigo-500 transition"
        >
          Edit my profile
        </a>
      </div>
    </main>
  );
};

export default OwnerPage;
