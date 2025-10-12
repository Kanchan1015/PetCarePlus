import { useState, useEffect } from "react";
import { appointmentsApi } from "../../shared/api/appointmentsApi";
import type { Appointment } from "../../shared/types/appointment";

const VetAppointmentsPage = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<"all" | "approved" | "pending" | "completed">("approved");

  useEffect(() => {
    loadAppointments();
  }, []);

  const loadAppointments = async () => {
    try {
      setLoading(true);
      setError(null);
      console.log('Loading vet appointments...');
      
      // Load all approved appointments (tries all, falls back to assigned)
      const allAppointments = await appointmentsApi.getAllApprovedForVet();
      console.log('Loaded approved appointments for vet:', allAppointments);
      
      // For the appointments page, we want to show all appointments the vet can access
      let data: Appointment[] = [];
      try {
        // First try to get all appointments (admin access)
        data = await appointmentsApi.getAll();
        console.log('Loaded all appointments (admin access):', data);
      } catch (adminError) {
        try {
          // Then try to get all approved appointments (vet access)
          data = await appointmentsApi.getApproved();
          console.log('Loaded approved appointments (vet access):', data);
        } catch (approvedError) {
          console.log('Approved access denied, loading assigned appointments...', approvedError);
          // Final fallback to assigned appointments only
          data = await appointmentsApi.getMyAssigned();
          console.log('Loaded assigned appointments:', data);
        }
      }
      
      setAppointments(data);
    } catch (error) {
      console.error("Failed to load appointments:", error);
      setError(`Failed to load appointments: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  const filteredAppointments = appointments.filter((appointment) => {
    if (filter === "all") return true;
    return appointment.status.toLowerCase() === filter;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "pending":
        return "bg-yellow-100 text-yellow-800 border-yellow-200";
      case "approved":
        return "bg-green-100 text-green-800 border-green-200";
      case "completed":
        return "bg-blue-100 text-blue-800 border-blue-200";
      case "cancelled":
        return "bg-red-100 text-red-800 border-red-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
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

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg text-slate-600 animate-pulse">
          Loading appointments...
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-800">
            My Appointments
          </h1>
          <p className="mt-2 text-slate-600">
            Manage your assigned appointments and patient visits
          </p>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-1 bg-gray-100 rounded-xl p-1">
        {[
          { key: "all", label: "All" },
          { key: "approved", label: "Approved" },
          { key: "pending", label: "Pending" },
          { key: "completed", label: "Completed" },
        ].map((tab) => (
          <button
            key={tab.key}
            onClick={() => setFilter(tab.key as any)}
            className={`px-4 py-2 rounded-xl text-sm font-medium transition-all ${
              filter === tab.key
                ? "bg-white text-gray-900 shadow-sm transform scale-105"
                : "text-gray-600 hover:text-gray-900 hover:bg-white hover:shadow-sm"
            }`}
          >
            {tab.label}
            {tab.key !== "all" && (
              <span className="ml-2 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-gray-600 bg-gray-200 rounded-full">
                {
                  appointments.filter((a) => a.status.toLowerCase() === tab.key)
                    .length
                }
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Error */}
      {error && (
        <div className="rounded-2xl bg-red-50 p-4 text-red-800 ring-1 ring-red-200 animate-fadeIn">
          <p>{error}</p>
          <button
            onClick={loadAppointments}
            className="mt-2 inline-flex items-center text-sm font-medium text-red-700 underline hover:text-red-800"
          >
            Try again
          </button>
        </div>
      )}

      {/* Appointments List */}
      {filteredAppointments.length > 0 ? (
        <div className="space-y-4">
          {filteredAppointments.map((appointment) => (
            <div
              key={appointment.id}
              className="bg-white rounded-3xl shadow-md p-6 border-l-4 border-blue-500 transition-shadow hover:shadow-lg"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <h3 className="text-lg font-semibold text-slate-800">
                      {appointment.petName}
                    </h3>
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(
                        appointment.status
                      )}`}
                    >
                      {appointment.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                    <div>
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Owner:</span>{" "}
                        {appointment.ownerName}
                      </p>
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Date:</span>{" "}
                        {formatDate(appointment.requestedDateTime)}
                      </p>
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Reason:</span>{" "}
                        {appointment.reasonForVisit}
                      </p>
                    </div>
                    <div>
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Requested:</span>{" "}
                        {new Date(appointment.createdAt).toLocaleDateString()}
                      </p>
                      {appointment.notes && (
                        <p className="text-sm text-gray-600">
                          <span className="font-medium">Notes:</span>{" "}
                          {appointment.notes}
                        </p>
                      )}
                    </div>
                  </div>

                  {appointment.adminNotes && (
                    <div className="mt-4 p-3 bg-gray-50 rounded-xl">
                      <p className="text-sm text-gray-700">
                        <span className="font-medium">Admin Notes:</span>{" "}
                        {appointment.adminNotes}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="rounded-2xl bg-white p-8 text-center ring-1 ring-slate-200 shadow-md">
          <div className="mb-3 text-6xl text-slate-400 animate-pulse">📅</div>
          <p className="text-lg text-slate-600">
            {filter === "all"
              ? "No appointments assigned yet."
              : `No ${filter} appointments found.`}
          </p>
        </div>
      )}
    </div>
  );
};

export default VetAppointmentsPage;