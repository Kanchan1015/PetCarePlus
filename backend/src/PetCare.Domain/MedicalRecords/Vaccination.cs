using System;
using PetCare.Domain.Pets;

namespace PetCare.Domain.MedicalRecords;

public class Vaccination
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PetId { get; set; }
    public string VetUserId { get; set; } = default!;
    public string VaccineName { get; set; } = default!;
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? BatchNumber { get; set; }
    public string? Manufacturer { get; set; }
    public VaccinationStatus Status { get; set; } = VaccinationStatus.Current;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Pet? Pet { get; set; }
}