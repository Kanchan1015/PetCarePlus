using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.DTOs;

public record TreatmentDto(
    Guid Id,
    Guid PetId,
    string VetUserId,
    string? VetFullName,
    string TreatmentType,
    string Diagnosis,
    string? TreatmentDescription,
    DateTime TreatmentDate,
    DateTime? FollowUpDate,
    TreatmentStatus Status,
    string? Medications,
    string? Instructions,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateTreatmentDto(
    Guid PetId,
    string TreatmentType,
    string Diagnosis,
    string? TreatmentDescription,
    DateTime TreatmentDate,
    DateTime? FollowUpDate,
    string? Medications,
    string? Instructions,
    string? Notes
);

public record UpdateTreatmentDto(
    string TreatmentType,
    string Diagnosis,
    string? TreatmentDescription,
    DateTime TreatmentDate,
    DateTime? FollowUpDate,
    TreatmentStatus Status,
    string? Medications,
    string? Instructions,
    string? Notes
);

public record TreatmentHistoryReportDto(
    Guid PetId,
    string PetName,
    IReadOnlyList<TreatmentDto> Treatments,
    DateTime? LastTreatmentDate,
    int TotalTreatments
);