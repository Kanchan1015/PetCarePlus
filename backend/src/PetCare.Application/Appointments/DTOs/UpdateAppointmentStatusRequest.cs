// UpdateAppointmentStatusRequest.cs
// Purpose: DTO for admin to approve/reject appointments with notes

using System.ComponentModel.DataAnnotations;
using PetCare.Domain.Appointments;

namespace PetCare.Application.Appointments.DTOs;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public AppointmentStatus Status { get; set; }

    [StringLength(1000)]
    public string? AdminNotes { get; set; }

    // Optional vet assignment when approving
    public string? VetUserId { get; set; }
}
