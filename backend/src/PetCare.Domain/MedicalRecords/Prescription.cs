using System;
using PetCare.Domain.Pets;

namespace PetCare.Domain.MedicalRecords;

public class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PetId { get; set; }
    public string VetUserId { get; set; } = default!;
    public string MedicationName { get; set; } = default!;
    public string Dosage { get; set; } = default!;
    public string Frequency { get; set; } = default!;
    public DateTime PrescribedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Pet? Pet { get; set; }
}