import { Link } from "react-router-dom";

const VetPage = () => {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-500">
        <h1 className="text-3xl font-bold text-blue-800 mb-2">
          Vet Dashboard
        </h1>
        <p className="text-gray-600">
          Access your appointments, patients, and treatments 🩺🐾
        </p>
      </div>

      {/* Navigation cards */}
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <div className="bg-white rounded-xl shadow-md p-6 border-t-4 border-orange-500 hover:shadow-lg transition-shadow">
          <h2 className="text-lg font-semibold text-gray-800 mb-2">
            Today&apos;s Appointments
          </h2>
          <p className="text-sm text-gray-600">
            Review and manage your daily schedule
          </p>
        </div>
        <div className="bg-white rounded-xl shadow-md p-6 border-t-4 border-green-500 hover:shadow-lg transition-shadow">
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Search Pets</h2>
          <p className="text-sm text-gray-600">
            Find patient records quickly
          </p>
        </div>
        <Link
          to="/vet/medical-records"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-blue-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-blue-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">
            Medical Records Management
          </h2>
          <p className="text-sm text-gray-600">
            Add treatments, vaccinations, and manage pet health records
          </p>
        </Link>
      </div>
    </div>
  );
};

export default VetPage;
