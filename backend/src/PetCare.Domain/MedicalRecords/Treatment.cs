using System;
using PetCare.Domain.Pets;

namespace PetCare.Domain.MedicalRecords;

public class Treatment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PetId { get; set; }
    public string VetUserId { get; set; } = default!;
    public string TreatmentType { get; set; } = default!;
    public string Diagnosis { get; set; } = default!;
    public string? Treatment_Description { get; set; }
    public DateTime TreatmentDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public TreatmentStatus Status { get; set; } = TreatmentStatus.Completed;
    public string? Medications { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Pet? Pet { get; set; }
}