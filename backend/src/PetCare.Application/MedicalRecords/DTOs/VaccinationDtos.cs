using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.DTOs;

public record VaccinationDto(
    Guid Id,
    Guid PetId,
    string VetUserId,
    string? VetFullName,
    string VaccineName,
    DateTime VaccinationDate,
    DateTime? NextDueDate,
    string? BatchNumber,
    string? Manufacturer,
    VaccinationStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateVaccinationDto(
    Guid PetId,
    string VaccineName,
    DateTime VaccinationDate,
    DateTime? NextDueDate,
    string? BatchNumber,
    string? Manufacturer,
    string? Notes
);

public record UpdateVaccinationDto(
    string VaccineName,
    DateTime VaccinationDate,
    DateTime? NextDueDate,
    string? BatchNumber,
    string? Manufacturer,
    VaccinationStatus Status,
    string? Notes
);

public record VaccinationReportDto(
    Guid PetId,
    string PetName,
    IReadOnlyList<VaccinationDto> Vaccinations,
    IReadOnlyList<VaccinationDto> OverdueVaccinations,
    IReadOnlyList<VaccinationDto> UpcomingVaccinations,
    bool IsUpToDate
);