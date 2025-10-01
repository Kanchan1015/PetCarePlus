using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.DTOs;

public record PrescriptionDto(
    Guid Id,
    Guid PetId,
    string VetUserId,
    string? VetFullName,
    string MedicationName,
    string Dosage,
    string Frequency,
    DateTime PrescribedDate,
    DateTime? StartDate,
    DateTime? EndDate,
    int? DurationDays,
    string? Instructions,
    PrescriptionStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreatePrescriptionDto(
    Guid PetId,
    string MedicationName,
    string Dosage,
    string Frequency,
    DateTime PrescribedDate,
    DateTime? StartDate,
    DateTime? EndDate,
    int? DurationDays,
    string? Instructions,
    string? Notes
);

public record UpdatePrescriptionDto(
    string MedicationName,
    string Dosage,
    string Frequency,
    DateTime PrescribedDate,
    DateTime? StartDate,
    DateTime? EndDate,
    int? DurationDays,
    string? Instructions,
    PrescriptionStatus Status,
    string? Notes
);