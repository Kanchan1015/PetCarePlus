using MediatR;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Commands.CreateVaccination;

public record CreateVaccinationCommand(
    Guid PetId,
    string VaccineName,
    DateTime VaccinationDate,
    DateTime? NextDueDate,
    string? BatchNumber,
    string? Manufacturer,
    string? Notes,
    string VetUserId
) : IRequest<VaccinationDto>;