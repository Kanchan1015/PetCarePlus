using MediatR;
using PetCare.Application.Pets.DTOs;
using PetCare.Domain.Pets;

namespace PetCare.Application.Pets.Commands.UpdatePet;

public record UpdatePetCommand(
    Guid Id,
    string Name,
    Species Species,
    string? Breed,
    DateTime? DateOfBirth,
    string? Color,
    decimal? Weight,
    string? MedicalNotes,
    bool IsActive
) : IRequest<PetDto>;