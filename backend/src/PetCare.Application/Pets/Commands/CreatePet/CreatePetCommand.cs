using MediatR;
using PetCare.Application.Pets.DTOs;
using PetCare.Domain.Pets;

namespace PetCare.Application.Pets.Commands.CreatePet;

public record CreatePetCommand(
    string Name,
    Species Species,
    string? Breed,
    DateTime? DateOfBirth,
    string? Color,
    decimal? Weight,
    string? MedicalNotes,
    string OwnerUserId
) : IRequest<PetDto>;