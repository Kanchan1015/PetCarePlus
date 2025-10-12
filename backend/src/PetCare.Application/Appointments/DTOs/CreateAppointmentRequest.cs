// CreateAppointmentRequest.cs
// Purpose: DTO for appointment creation requests from owners
// Validation: Required fields, future date validation will be in handler

using System;
using System.ComponentModel.DataAnnotations;

namespace PetCare.Application.Appointments.DTOs;

public class CreateAppointmentRequest
{
    [Required]
    public Guid PetId { get; set; }

    [Required]
    public DateTime RequestedDateTime { get; set; }

    [Required]
    [StringLength(200)]
    public string ReasonForVisit { get; set; } = default!;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
