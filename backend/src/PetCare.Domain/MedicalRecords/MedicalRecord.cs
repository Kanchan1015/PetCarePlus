using System;
using PetCare.Domain.Pets;

namespace PetCare.Domain.MedicalRecords;

public class MedicalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PetId { get; set; }
    public string VetUserId { get; set; } = default!; // Reference to vet who created the record
    public MedicalRecordType RecordType { get; set; }
    public DateTime RecordDate { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Pet? Pet { get; set; }
}