// Appointment.cs
// Purpose: Domain entity for appointment requests and scheduling
// Features: Owner requests, admin approval/rejection, vet assignment, status tracking
// Foreign Keys: PetId (Guid), OwnerUserId (string), VetUserId (string, optional)

using System;
using PetCare.Domain.Pets;

namespace PetCare.Domain.Appointments;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Foreign Key relationships following existing pattern
    public Guid PetId { get; set; }
    public Pet Pet { get; set; } = default!; // Navigation property

    public string OwnerUserId { get; set; } = default!; // FK to AspNetUsers
    public string? VetUserId { get; set; } // Optional - assigned by admin

    // Appointment details
    public DateTime RequestedDateTime { get; set; }
    public DateTime? ActualDateTime { get; set; } // Set when appointment is confirmed
    public string ReasonForVisit { get; set; } = default!;
    public string? Notes { get; set; } // Owner's notes
    public string? AdminNotes { get; set; } // Admin's notes when approving/rejecting

    // Status tracking
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    // Audit fields following existing pattern
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; } // Who last updated (admin/vet)

    // Computed properties for frontend
    public string StatusDisplayName => Status.ToString();
    public bool CanBeCancelled => Status == AppointmentStatus.Pending || Status == AppointmentStatus.Approved;
    public bool RequiresAction => Status == AppointmentStatus.Pending;
}
