const VetPage = () => {
  return (
    <main className="mx-auto max-w-5xl px-4 py-10 space-y-8">
      {/* Header */}
      <header>
        <h1 className="text-3xl font-bold tracking-tight text-slate-800">
          Vet Dashboard
        </h1>
        <p className="mt-1 text-slate-600">
          Access your appointments, patients, and treatments
        </p>
      </header>

      {/* Navigation cards */}
      <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <li className="rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm hover:shadow-md hover:bg-slate-50 transition">
          <h2 className="font-semibold text-slate-800">
            Today&apos;s Appointments
          </h2>
          <p className="mt-1 text-sm text-slate-600">
            Review and manage your daily schedule
          </p>
        </li>
        <li className="rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm hover:shadow-md hover:bg-slate-50 transition">
          <h2 className="font-semibold text-slate-800">Search Pets</h2>
          <p className="mt-1 text-sm text-slate-600">
            Find patient records quickly
          </p>
        </li>
        <li className="rounded-2xl bg-white p-5 ring-1 ring-slate-200 shadow-sm hover:shadow-md hover:bg-slate-50 transition">
          <h2 className="font-semibold text-slate-800">
            Add Treatment / Vaccination
          </h2>
          <p className="mt-1 text-sm text-slate-600">
            Update pet health records with new care
          </p>
        </li>
      </ul>
    </main>
  );
};

export default VetPage;
