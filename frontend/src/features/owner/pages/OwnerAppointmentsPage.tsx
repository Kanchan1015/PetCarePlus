// OwnerAppointmentsPage.tsx
// Purpose: Display all of owner's appointments with status filtering and visual indicators
// Features: Filter tabs (All/Pending/Approved/Cancelled), status badges, appointment cards with details
// Route: /owner/appointments - "As an owner, I want to track my appointment requests and their status"

import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { appointmentsApi } from "../../shared/api/appointmentsApi";
import type { Appointment } from "../../shared/types/appointment";

export const OwnerAppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<
    "all" | "pending" | "approved" | "cancelled"
  >("all");

  useEffect(() => {
    loadMyAppointments();
  }, []);

  const loadMyAppointments = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await appointmentsApi.getMy();
      setAppointments(data);
    } catch (error) {
      console.error("Failed to load appointments:", error);
      setError("Failed to load your appointments. Please try again.");
    } finally {
      setIsLoading(false);
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
      case "cancelled":
        return "bg-red-100 text-red-800 border-red-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case "pending":
        return "⏳";
      case "approved":
        return "✅";
      case "cancelled":
        return "❌";
      default:
        return "📅";
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg text-slate-600 animate-pulse">
          Loading your appointments...
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
            Track your upcoming and past appointments
          </p>
        </div>
        <div>
          <Link
            to="/owner/appointments/request"
            className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
          >
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            Request Appointment
          </Link>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-1 bg-gray-100 rounded-xl p-1">
        {[
          { key: "all", label: "All" },
          { key: "pending", label: "Pending" },
          { key: "approved", label: "Approved" },
          { key: "cancelled", label: "Cancelled" },
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
            onClick={loadMyAppointments}
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
                    <span className="text-2xl">
                      {getStatusIcon(appointment.status)}
                    </span>
                    <h3 className="text-lg font-semibold text-slate-800">
                      Appointment for {appointment.petName}
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
                        <span className="font-medium">Date:</span>{" "}
                        {new Date(
                          appointment.requestedDateTime
                        ).toLocaleDateString()}
                      </p>
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Time:</span>{" "}
                        {new Date(
                          appointment.requestedDateTime
                        ).toLocaleTimeString()}
                      </p>
                      {appointment.vetName && (
                        <p className="text-sm text-gray-600">
                          <span className="font-medium">Veterinarian:</span>{" "}
                          {appointment.vetName}
                        </p>
                      )}
                    </div>
                    <div>
                      {appointment.reasonForVisit && (
                        <p className="text-sm text-gray-600">
                          <span className="font-medium">Reason:</span>{" "}
                          {appointment.reasonForVisit}
                        </p>
                      )}
                      <p className="text-sm text-gray-600">
                        <span className="font-medium">Requested:</span>{" "}
                        {new Date(appointment.createdAt).toLocaleDateString()}
                      </p>
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
              ? "You don't have any appointments yet."
              : `No ${filter} appointments found.`}
          </p>
          <p className="text-slate-500 mt-2">
            {filter === "all" &&
              "Request your first appointment to get started."}
          </p>
        </div>
      )}
    </div>
  );
};
