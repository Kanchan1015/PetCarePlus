import React, { useState, useEffect } from "react";
import jsPDF from "jspdf";
import { appointmentsApi } from "../../shared/api/appointmentsApi";
import type { Appointment, AppointmentSummaryReport } from "../../shared/types/appointment";

export const AdminAppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<
    "all" | "pending" | "approved" | "cancelled"
  >("pending");
  const [processingId, setProcessingId] = useState<string | null>(null);
  const [isGeneratingReport, setIsGeneratingReport] = useState(false);

  useEffect(() => {
    loadAppointments();
  }, [filter]);

  const loadAppointments = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data =
        filter === "pending"
          ? await appointmentsApi.getPending()
          : await appointmentsApi.getAll();
      setAppointments(data as Appointment[]);
    } catch (error) {
      console.error("Failed to load appointments:", error);
      setError("Failed to load appointments. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleApprove = async (appointmentId: string) => {
    try {
      setProcessingId(appointmentId);
      await appointmentsApi.updateStatus(appointmentId, {
        status: "Approved",
        adminNotes: "Appointment approved by admin",
      });
      await loadAppointments();
      alert("Appointment approved successfully! Owner will be notified.");
    } catch (error) {
      console.error("Failed to approve appointment:", error);
      alert("Failed to approve appointment. Please try again.");
    } finally {
      setProcessingId(null);
    }
  };

  const handleReject = async (appointmentId: string, reason?: string) => {
    try {
      setProcessingId(appointmentId);
      await appointmentsApi.updateStatus(appointmentId, {
        status: "Cancelled",
        adminNotes: reason || "Appointment cancelled by admin",
      });
      await loadAppointments();
      alert("Appointment rejected successfully! Owner will be notified.");
    } catch (error) {
      console.error("Failed to reject appointment:", error);
      alert("Failed to reject appointment. Please try again.");
    } finally {
      setProcessingId(null);
    }
  };

  const generateSummaryReport = async () => {
    try {
      setIsGeneratingReport(true);
      
      // Fetch summary report data from backend
      const reportData: AppointmentSummaryReport = await appointmentsApi.getSummaryReport();
      
      // Create PDF document
      const doc = new jsPDF();
      const pageWidth = doc.internal.pageSize.getWidth();
      let yPosition = 20;
      
      // Header
      doc.setFontSize(20);
      doc.setFont("helvetica", "bold");
      doc.text("PetCare Plus - Appointment Summary Report", pageWidth / 2, yPosition, { align: "center" });
      
      yPosition += 15;
      doc.setFontSize(12);
      doc.setFont("helvetica", "normal");
      doc.text(`Generated: ${reportData.generatedAt}`, pageWidth / 2, yPosition, { align: "center" });
      doc.text(`Report Period: ${reportData.reportPeriod}`, pageWidth / 2, yPosition + 8, { align: "center" });
      
      yPosition += 30;
      
      // Overall Statistics Section
      doc.setFontSize(16);
      doc.setFont("helvetica", "bold");
      doc.text("Overall Statistics", 20, yPosition);
      yPosition += 15;
      
      doc.setFontSize(11);
      doc.setFont("helvetica", "normal");
      const stats = [
        `Total Appointments: ${reportData.totalAppointmentsCount}`,
        `Completed Appointments: ${reportData.completedAppointmentsCount}`,
        `Cancelled Appointments: ${reportData.cancelledAppointmentsCount}`,
        `No-Show Appointments: ${reportData.noShowAppointmentsCount}`
      ];
      
      stats.forEach((stat) => {
        doc.text(stat, 25, yPosition);
        yPosition += 8;
      });
      
      yPosition += 10;
      
      // Workload Metrics Section
      doc.setFontSize(16);
      doc.setFont("helvetica", "bold");
      doc.text("Workload Metrics", 20, yPosition);
      yPosition += 15;
      
      doc.setFontSize(11);
      doc.setFont("helvetica", "normal");
      const workloadMetrics = [
        `Average Appointments per Day: ${reportData.averageAppointmentsPerDay.toFixed(1)}`,
        `Busiest Day of Week: ${reportData.busiestDayOfWeek}`,
        `Peak Appointment Hour: ${reportData.peakAppointmentHour}:00`
      ];
      
      workloadMetrics.forEach((metric) => {
        doc.text(metric, 25, yPosition);
        yPosition += 8;
      });
      
      yPosition += 15;
      
      // Appointment Categories Section
      doc.setFontSize(16);
      doc.setFont("helvetica", "bold");
      doc.text("Appointment Categories", 20, yPosition);
      yPosition += 15;
      
      doc.setFontSize(11);
      doc.setFont("helvetica", "normal");
      
      // Upcoming Appointments
      doc.setFont("helvetica", "bold");
      doc.text(`Upcoming Appointments: ${reportData.upcomingAppointmentsCount}`, 25, yPosition);
      yPosition += 10;
      
      if (reportData.upcomingAppointments.length > 0) {
        doc.setFont("helvetica", "normal");
        reportData.upcomingAppointments.slice(0, 5).forEach((apt) => {
          const date = new Date(apt.requestedDateTime).toLocaleDateString();
          const time = new Date(apt.requestedDateTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
          doc.text(`• ${apt.petName} (${apt.ownerName}) - ${date} ${time}`, 30, yPosition);
          yPosition += 6;
        });
        if (reportData.upcomingAppointments.length > 5) {
          doc.text(`... and ${reportData.upcomingAppointments.length - 5} more`, 30, yPosition);
          yPosition += 6;
        }
      }
      
      yPosition += 10;
      
      // Pending Appointments
      doc.setFont("helvetica", "bold");
      doc.text(`Pending Appointments: ${reportData.pendingAppointmentsCount}`, 25, yPosition);
      yPosition += 10;
      
      if (reportData.pendingAppointments.length > 0) {
        doc.setFont("helvetica", "normal");
        reportData.pendingAppointments.slice(0, 5).forEach((apt) => {
          const date = new Date(apt.requestedDateTime).toLocaleDateString();
          doc.text(`• ${apt.petName} (${apt.ownerName}) - ${date} - ${apt.reasonForVisit}`, 30, yPosition);
          yPosition += 6;
        });
        if (reportData.pendingAppointments.length > 5) {
          doc.text(`... and ${reportData.pendingAppointments.length - 5} more`, 30, yPosition);
          yPosition += 6;
        }
      }
      
      // Check if we need a new page
      if (yPosition > 250) {
        doc.addPage();
        yPosition = 20;
      }
      
      yPosition += 10;
      
      // Recent Past Appointments
      doc.setFont("helvetica", "bold");
      doc.text(`Recent Past Appointments (Last 30 days): ${reportData.pastAppointmentsCount}`, 25, yPosition);
      yPosition += 10;
      
      if (reportData.pastAppointments.length > 0) {
        doc.setFont("helvetica", "normal");
        reportData.pastAppointments.slice(0, 5).forEach((apt) => {
          const date = new Date(apt.requestedDateTime).toLocaleDateString();
          doc.text(`• ${apt.petName} (${apt.ownerName}) - ${date} - Status: ${apt.status}`, 30, yPosition);
          yPosition += 6;
        });
        if (reportData.pastAppointments.length > 5) {
          doc.text(`... and ${reportData.pastAppointments.length - 5} more`, 30, yPosition);
          yPosition += 6;
        }
      }
      
      // Footer
      const pageHeight = doc.internal.pageSize.getHeight();
      doc.setFontSize(8);
      doc.text(`Report generated on ${new Date().toLocaleString()}`, pageWidth / 2, pageHeight - 10, { align: "center" });
      
      // Save the PDF
      const fileName = `appointment-summary-report-${new Date().toISOString().split('T')[0]}.pdf`;
      doc.save(fileName);
      
    } catch (error) {
      console.error("Failed to generate report:", error);
      alert("Failed to generate appointment summary report. Please try again.");
    } finally {
      setIsGeneratingReport(false);
    }
  };

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

  const filteredAppointments = appointments.filter((a) =>
    filter === "all" ? true : a.status.toLowerCase() === filter
  );

  if (isLoading) {
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
      <div className="flex justify-between items-start">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-800">
            Appointment Requests
          </h1>
          <p className="mt-1 text-slate-600">
            Manage pet owner appointment requests
          </p>
        </div>
        <div className="flex space-x-3">
          <button
            onClick={generateSummaryReport}
            disabled={isGeneratingReport}
            className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isGeneratingReport ? (
              <>
                <svg className="w-4 h-4 mr-2 animate-spin" viewBox="0 0 24 24">
                  <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" className="opacity-25"></circle>
                  <path fill="currentColor" className="opacity-75" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Generating...
              </>
            ) : (
              <>
                📊 Download Summary Report
              </>
            )}
          </button>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-1 bg-gray-100 rounded-lg p-1">
        {["pending", "approved", "cancelled", "all"].map((tab) => (
          <button
            key={tab}
            onClick={() => setFilter(tab as any)}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              filter === tab
                ? "bg-white text-gray-900 shadow-sm"
                : "text-gray-600 hover:text-gray-900"
            }`}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
            <span className="ml-2 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-gray-600 bg-gray-200 rounded-full">
              {
                appointments.filter((a) =>
                  tab === "all" ? true : a.status.toLowerCase() === tab
                ).length
              }
            </span>
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

      {/* Appointment List */}
      {filteredAppointments.length > 0 ? (
        <div className="space-y-4">
          {filteredAppointments.map((appointment) => (
            <div
              key={appointment.id}
              className="bg-white rounded-xl shadow-md p-6 border-l-4 border-blue-500 transition transform hover:-translate-y-1 hover:shadow-lg"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1 space-y-3">
                  <div className="flex items-center space-x-3">
                    <h3 className="text-lg font-semibold text-slate-800">
                      {appointment.petName} - {appointment.ownerName}
                    </h3>
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(
                        appointment.status
                      )}`}
                    >
                      {appointment.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
                    <div>
                      <p>
                        <span className="font-medium">Requested Date:</span>{" "}
                        {new Date(
                          appointment.requestedDateTime
                        ).toLocaleDateString()}
                      </p>
                      <p>
                        <span className="font-medium">Requested Time:</span>{" "}
                        {new Date(
                          appointment.requestedDateTime
                        ).toLocaleTimeString()}
                      </p>
                      <p>
                        <span className="font-medium">Reason:</span>{" "}
                        {appointment.reasonForVisit}
                      </p>
                    </div>
                    <div>
                      <p>
                        <span className="font-medium">Owner:</span>{" "}
                        {appointment.ownerName}
                      </p>
                      <p>
                        <span className="font-medium">Pet:</span>{" "}
                        {appointment.petName}
                      </p>
                      <p>
                        <span className="font-medium">Requested:</span>{" "}
                        {new Date(appointment.createdAt).toLocaleDateString()}
                      </p>
                    </div>
                  </div>

                  {appointment.notes && (
                    <div className="mt-2 p-3 bg-gray-50 rounded-lg text-sm text-gray-700">
                      <span className="font-medium">Owner Notes:</span>{" "}
                      {appointment.notes}
                    </div>
                  )}

                  {appointment.adminNotes && (
                    <div className="mt-2 p-3 bg-blue-50 rounded-lg text-sm text-gray-700">
                      <span className="font-medium">Admin Notes:</span>{" "}
                      {appointment.adminNotes}
                    </div>
                  )}
                </div>

                {/* Action Buttons */}
                {appointment.status.toLowerCase() === "pending" && (
                  <div className="flex flex-col space-y-2 ml-4">
                    <button
                      onClick={() => handleApprove(appointment.id)}
                      disabled={processingId === appointment.id}
                      className="inline-flex items-center px-3 py-2 text-sm font-medium text-white bg-green-600 rounded-lg hover:bg-green-700 focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {processingId === appointment.id
                        ? "Processing..."
                        : " Approve"}
                    </button>
                    <button
                      onClick={() => {
                        const reason = prompt(
                          "Reason for rejection (optional):"
                        );
                        if (reason !== null)
                          handleReject(appointment.id, reason);
                      }}
                      disabled={processingId === appointment.id}
                      className="inline-flex items-center px-3 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 focus:ring-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Reject
                    </button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="rounded-2xl bg-white p-8 text-center ring-1 ring-slate-200">
          <div className="mb-3 text-6xl text-slate-400">📅</div>
          <p className="text-lg text-slate-600">
            {filter === "pending"
              ? "No pending appointment requests."
              : `No ${filter} appointments found.`}
          </p>
          {filter === "pending" && (
            <p className="text-slate-500 mt-1">
              New requests will appear here when owners submit them.
            </p>
          )}
        </div>
      )}
    </div>
  );
};
