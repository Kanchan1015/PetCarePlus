import { Link } from "react-router-dom";

const OwnerPage = () => {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-green-500">
        <h1 className="text-3xl font-bold text-green-800 mb-2">
          Pet Owner Dashboard
        </h1>
        <p className="text-gray-600">
          Manage your pets, appointments, and profile 🐕🐈❤️
        </p>
      </div>

      {/* Navigation cards */}
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <Link
          to="/owner/pets"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-green-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-green-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">My Pets</h2>
          <p className="text-sm text-gray-600">
            View and manage your pets
          </p>
        </Link>
        <Link
          to="/owner/pets"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-blue-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-blue-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Medical History</h2>
          <p className="text-sm text-gray-600">
            View your pets' medical records and vaccination history
          </p>
        </Link>
        <div className="bg-white rounded-xl shadow-md p-6 border-t-4 border-purple-500 hover:shadow-lg transition-shadow">
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Request Appointment</h2>
          <p className="text-sm text-gray-600">
            Book your next visit easily
          </p>
        </div>
        <div className="bg-white rounded-xl shadow-md p-6 border-t-4 border-orange-500 hover:shadow-lg transition-shadow">
          <h2 className="text-lg font-semibold text-gray-800 mb-2">My Appointments</h2>
          <p className="text-sm text-gray-600">
            Track upcoming and past bookings
          </p>
        </div>
      </div>

      {/* Footer link */}
      <div className="bg-white rounded-xl shadow-md p-6">
        <Link
          to="/owner/profile"
          className="inline-flex items-center rounded-2xl bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700 focus-visible:ring focus-visible:ring-green-500 transition-colors"
        >
          Edit my profile
        </Link>
      </div>
    </div>
  );
};

export default OwnerPage;
