import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { appointmentsApi } from "../shared/api/appointmentsApi";
import type { Appointment } from "../shared/types/appointment";

const VetPage = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log('Loading vet dashboard data...');
      
      // Load approved appointments (tries all appointments, falls back to assigned)
      const approvedAppointments = await appointmentsApi.getAllApprovedForVet();
      console.log('Approved appointments for vet:', approvedAppointments);
      console.log('Approved appointments count:', approvedAppointments.length);
      
      // Show latest 5 approved appointments
      setAppointments(approvedAppointments.slice(0, 5));
      
    } catch (err) {
      console.error('Failed to load dashboard data:', err);
      
      // Check if it's an authorization error
      if (err instanceof Error && err.message.includes('403')) {
        setError('Access denied: You may not have vet permissions or no appointments are assigned to you.');
      } else if (err instanceof Error && err.message.includes('401')) {
        setError('Authentication required: Please log in again.');
      } else {
        setError(`Failed to load appointments: ${err instanceof Error ? err.message : 'Unknown error'}`);
      }
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-500">
        <h1 className="text-3xl font-bold text-blue-800 mb-2">
          Vet Dashboard
        </h1>
        <p className="text-gray-600">
          Manage your appointments, patients, and treatments 🩺🐾
        </p>
      </div>

      {/* Quick Actions */}
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
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
        
        <Link
          to="/vet/appointments"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-green-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-green-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">
            All Appointments
          </h2>
          <p className="text-sm text-gray-600">
            View all your assigned appointments
          </p>
        </Link>

        <Link
          to="/vet/profile"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-purple-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-purple-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">
            Profile
          </h2>
          <p className="text-sm text-gray-600">
            View and edit your veterinarian profile
          </p>
        </Link>
      </div>

      {/* Appointments Section */}
      <div className="bg-white rounded-xl shadow-md p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-semibold text-gray-800">Approved Appointments</h2>
          <div className="flex space-x-2">
            <button
              onClick={loadDashboardData}
              className="text-gray-600 hover:text-gray-700 text-sm font-medium px-3 py-1 border border-gray-300 rounded"
            >
              Refresh
            </button>
            <Link 
              to="/vet/appointments"
              className="text-blue-600 hover:text-blue-700 text-sm font-medium"
            >
              View All
            </Link>
          </div>
        </div>
        
        {loading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
        ) : error ? (
          <div className="text-center py-8 text-red-600">
            <p>{error}</p>
          </div>
        ) : appointments.length > 0 ? (
          <div className="space-y-3">
            {appointments.map((appointment) => (
              <div key={appointment.id} className="flex justify-between items-center p-4 bg-gray-50 rounded-lg">
                <div>
                  <h3 className="font-medium text-gray-800">{appointment.petName}</h3>
                  <p className="text-sm text-gray-600">Owner: {appointment.ownerName}</p>
                  <p className="text-sm text-gray-600">{appointment.reasonForVisit}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-medium text-blue-600">
                    {formatDate(appointment.requestedDateTime)}
                  </p>
                  <span className="inline-block px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                    {appointment.status}
                  </span>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            <div className="mb-2 text-2xl">📅</div>
            <p className="font-medium">No approved appointments found</p>
            <div className="text-sm mt-2 space-y-1">
              <p>Approved appointments will appear here when:</p>
              <ul className="list-disc list-inside text-left inline-block mt-2">
                <li>Pet owners request appointments</li>
                <li>An admin approves the appointment requests</li>
                <li>The appointments become available for veterinary care</li>
              </ul>
            </div>
          </div>
        )}
      </div>

      {/* Treatments Section */}
      <div className="bg-white rounded-xl shadow-md p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-semibold text-gray-800">Recent Treatment History</h2>
          <Link 
            to="/vet/treatments"
            className="text-blue-600 hover:text-blue-700 text-sm font-medium"
          >
            View All
          </Link>
        </div>
        
        {/* Placeholder for treatments - will be implemented when vet treatment API is available */}
        <div className="text-center py-8 text-gray-500">
          <p>Treatment history will be displayed here</p>
          <p className="text-sm mt-2">Coming soon - vet-specific treatment API integration</p>
        </div>
      </div>
    </div>
  );
};

export default VetPage;
