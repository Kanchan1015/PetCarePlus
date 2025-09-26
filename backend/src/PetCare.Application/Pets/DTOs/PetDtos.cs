using PetCare.Domain.Pets;

namespace PetCare.Application.Pets.DTOs;

public record PetDto(
    Guid Id,
    string Name,
    Species Species,
    string? Breed,
    DateTime? DateOfBirth,
    string? Color,
    decimal? Weight,
    string? MedicalNotes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string OwnerUserId,
    string? OwnerFullName,
    int? AgeInYears
);

public record PetSummaryDto(
    Guid Id,
    string Name,
    Species Species,
    string? Breed,
    int? AgeInYears,
    bool IsActive,
    string OwnerFullName
);

public record CreatePetDto(
    string Name,
    Species Species,
    string? Breed,
    DateTime? DateOfBirth,
    string? Color,
    decimal? Weight,
    string? MedicalNotes,
    string OwnerUserId
);

public record UpdatePetDto(
    string Name,
    Species Species,
    string? Breed,
    DateTime? DateOfBirth,
    string? Color,
    decimal? Weight,
    string? MedicalNotes,
    bool IsActive
);

public record AssignPetDto(
    Guid PetId,
    string NewOwnerUserId
);