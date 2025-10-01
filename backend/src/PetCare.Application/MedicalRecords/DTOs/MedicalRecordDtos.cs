using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.DTOs;

public record MedicalRecordDto(
    Guid Id,
    Guid PetId,
    string VetUserId,
    string? VetFullName,
    MedicalRecordType RecordType,
    DateTime RecordDate,
    string Title,
    string? Description,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateMedicalRecordDto(
    Guid PetId,
    MedicalRecordType RecordType,
    DateTime RecordDate,
    string Title,
    string? Description,
    string? Notes
);

public record UpdateMedicalRecordDto(
    MedicalRecordType RecordType,
    DateTime RecordDate,
    string Title,
    string? Description,
    string? Notes
);