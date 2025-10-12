// OwnerPage.tsx - Updated for Sprint
// Purpose: Owner dashboard with functional navigation cards and notification badges
// Key Changes: Added Links (not divs), notification badge on "My Appointments", state for pending count
// Features: Visual notification badges, responsive grid, connected to new appointment routes

import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { appointmentsApi } from "../shared/api/appointmentsApi";

const OwnerPage = () => {
  // Real state for notification badges - integrated with appointments API
  const [pendingAppointments, setPendingAppointments] = useState(0);

  useEffect(() => {
    loadPendingAppointmentCount();
    
    // Optional: Refresh count when page becomes visible (user returns from other tabs)
    const handleVisibilityChange = () => {
      if (!document.hidden) {
        loadPendingAppointmentCount();
      }
    };
    
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, []);

  const loadPendingAppointmentCount = async () => {
    try {
      // Fetch user's appointments and count pending ones
      const appointments = await appointmentsApi.getMy();
      const pendingCount = appointments.filter(
        appointment => appointment.status.toLowerCase() === 'pending'
      ).length;
      setPendingAppointments(pendingCount);
    } catch (error) {
      console.error("Failed to load appointment count:", error);
      // On error, don't show badge (keep count at 0)
      setPendingAppointments(0);
    }
  };

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
          to="/owner/medical-records"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-blue-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-blue-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Medical Records</h2>
          <p className="text-sm text-gray-600">
            View your pets' medical records and vaccination history
          </p>
        </Link>

        {/* Sprint Addition: Changed from div to Link for functional navigation */}
        <Link
          to="/owner/appointments/request"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-purple-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-purple-500"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Request Appointment</h2>
          <p className="text-sm text-gray-600">
            Book your next visit easily
          </p>
        </Link>

        {/* Sprint Addition: Added notification badge and Link functionality */}
        <Link
          to="/owner/appointments"
          className="block bg-white rounded-xl shadow-md p-6 border-t-4 border-orange-500 hover:shadow-lg transition-shadow focus-visible:ring focus-visible:ring-orange-500 relative"
        >
          <h2 className="text-lg font-semibold text-gray-800 mb-2">My Appointments</h2>
          <p className="text-sm text-gray-600">
            Track upcoming and past bookings
          </p>
          {/* Sprint Feature: Notification badge shows pending appointment count */}
          {pendingAppointments > 0 && (
            <div className="absolute -top-2 -right-2 h-6 w-6 rounded-full bg-red-500 flex items-center justify-center">
              <span className="text-xs font-medium text-white">
                {pendingAppointments > 9 ? '9+' : pendingAppointments}
              </span>
            </div>
          )}
        </Link>
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
