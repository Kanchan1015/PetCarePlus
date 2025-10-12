using System;
using System.Collections.Generic;

namespace PetCare.Domain.Pets;

public class Pet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Owner link (AspNetUsers.Id string)
    public string OwnerUserId { get; set; } = default!;

    // Required
    public string Name { get; set; } = default!;
    public Species Species { get; set; }

    // Optional
    public string? Breed { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Color { get; set; }
    public decimal? Weight { get; set; }
    public string? MedicalNotes { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for medical records
    public ICollection<MedicalRecords.MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecords.MedicalRecord>();
    public ICollection<MedicalRecords.Vaccination> Vaccinations { get; set; } = new List<MedicalRecords.Vaccination>();
    public ICollection<MedicalRecords.Treatment> Treatments { get; set; } = new List<MedicalRecords.Treatment>();
    public ICollection<MedicalRecords.Prescription> Prescriptions { get; set; } = new List<MedicalRecords.Prescription>();
    
    // Sprint Addition: Navigation property for appointments
    public ICollection<Appointments.Appointment> Appointments { get; set; } = new List<Appointments.Appointment>();

    // Computed properties
    public int? AgeInYears => DateOfBirth.HasValue 
        ? (int?)((DateTime.Now - DateOfBirth.Value).Days / 365.25) 
        : null;
}
