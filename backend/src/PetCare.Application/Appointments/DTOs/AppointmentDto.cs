// AppointmentDto.cs
// Purpose: Response DTO for appointment data with computed properties for frontend

using System;
using PetCare.Domain.Appointments;

namespace PetCare.Application.Appointments.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string PetName { get; set; } = default!;
    public string OwnerUserId { get; set; } = default!;
    public string OwnerName { get; set; } = default!;
    public string? VetUserId { get; set; }
    public string? VetName { get; set; }
    
    public DateTime RequestedDateTime { get; set; }
    public DateTime? ActualDateTime { get; set; }
    public string ReasonForVisit { get; set; } = default!;
    public string? Notes { get; set; }
    public string? AdminNotes { get; set; }
    
    public AppointmentStatus Status { get; set; }
    public string StatusDisplayName { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties for frontend
    public bool CanBeCancelled { get; set; }
    public bool RequiresAction { get; set; }
}
