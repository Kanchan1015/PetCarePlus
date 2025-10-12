// AppointmentSummaryReportDto.cs
// Purpose: Response DTO for appointment summary report data for admin workload tracking

using System;
using System.Collections.Generic;

namespace PetCare.Application.Appointments.DTOs;

public class AppointmentSummaryReportDto
{
    public DateTime GeneratedAt { get; set; }
    public string ReportPeriod { get; set; } = default!;
    
    // Upcoming appointments
    public int UpcomingAppointmentsCount { get; set; }
    public List<AppointmentDto> UpcomingAppointments { get; set; } = new();
    
    // Pending appointments
    public int PendingAppointmentsCount { get; set; }
    public List<AppointmentDto> PendingAppointments { get; set; } = new();
    
    // Past appointments (recent)
    public int PastAppointmentsCount { get; set; }
    public List<AppointmentDto> PastAppointments { get; set; } = new();
    
    // Overall statistics
    public int TotalAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int CancelledAppointmentsCount { get; set; }
    public int NoShowAppointmentsCount { get; set; }
    
    // Workload metrics
    public double AverageAppointmentsPerDay { get; set; }
    public string BusiestDayOfWeek { get; set; } = default!;
    public int PeakAppointmentHour { get; set; }
}