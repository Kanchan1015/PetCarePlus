import { Link } from "react-router-dom";

const VetTreatmentsPage = () => {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-800">
            Treatment History
          </h1>
          <p className="mt-2 text-slate-600">
            View and manage treatment records
          </p>
        </div>
        <Link
          to="/vet/medical-records"
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          Add Treatment
        </Link>
      </div>

      {/* Info Card */}
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
        <div className="flex items-start space-x-3">
          <div className="text-blue-600 text-xl">ℹ️</div>
          <div>
            <h3 className="text-lg font-semibold text-blue-800 mb-2">How to View Treatment History</h3>
            <p className="text-blue-700 mb-3">
              Treatment records are currently organized by pet. To view treatment history:
            </p>
            <ol className="list-decimal list-inside text-blue-700 space-y-1 mb-4">
              <li>Go to your <Link to="/vet/appointments" className="font-medium underline hover:text-blue-800">Appointments</Link> page</li>
              <li>Find the appointment for the pet you want to review</li>
              <li>Click on the pet's name or go to Medical Records</li>
              <li>Add or view treatments for that specific pet</li>
            </ol>
            <p className="text-sm text-blue-600">
              <strong>Note:</strong> A consolidated treatment history view by veterinarian is planned for a future update.
            </p>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="grid gap-6 sm:grid-cols-1 lg:grid-cols-3">
        <Link
          to="/vet/medical-records"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-green-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-green-500"
        >
          <div className="text-3xl mb-3">🏥</div>
          <h2 className="text-lg font-semibold text-gray-800 mb-2">
            Medical Records
          </h2>
          <p className="text-sm text-gray-600">
            Add new treatments, vaccinations, and manage pet health records
          </p>
        </Link>
        
        <Link
          to="/vet/appointments"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-blue-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-blue-500"
        >
          <div className="text-3xl mb-3">📅</div>
          <h2 className="text-lg font-semibold text-gray-800 mb-2">
            View Appointments
          </h2>
          <p className="text-sm text-gray-600">
            See your scheduled appointments and access patient records
          </p>
        </Link>

        <div className="bg-gray-50 rounded-xl shadow-sm p-6 border-t-4 border-gray-300">
          <div className="text-3xl mb-3 opacity-50">📊</div>
          <h2 className="text-lg font-semibold text-gray-500 mb-2">
            Treatment Analytics
          </h2>
          <p className="text-sm text-gray-500">
            Treatment statistics and reports coming soon
          </p>
        </div>
      </div>

      {/* Future Features */}
      <div className="bg-white rounded-xl shadow-md p-6">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">Planned Features</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="flex items-start space-x-3">
            <div className="text-gray-400 text-lg">📋</div>
            <div>
              <h4 className="font-medium text-gray-700">Consolidated Treatment List</h4>
              <p className="text-sm text-gray-600">View all treatments performed by you across all pets</p>
            </div>
          </div>
          <div className="flex items-start space-x-3">
            <div className="text-gray-400 text-lg">🔍</div>
            <div>
              <h4 className="font-medium text-gray-700">Advanced Filtering</h4>
              <p className="text-sm text-gray-600">Filter treatments by date, pet, treatment type, and outcome</p>
            </div>
          </div>
          <div className="flex items-start space-x-3">
            <div className="text-gray-400 text-lg">📈</div>
            <div>
              <h4 className="font-medium text-gray-700">Treatment Analytics</h4>
              <p className="text-sm text-gray-600">Statistics on treatment success rates and patient outcomes</p>
            </div>
          </div>
          <div className="flex items-start space-x-3">
            <div className="text-gray-400 text-lg">📄</div>
            <div>
              <h4 className="font-medium text-gray-700">Export Reports</h4>
              <p className="text-sm text-gray-600">Generate and export treatment history reports</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VetTreatmentsPage;