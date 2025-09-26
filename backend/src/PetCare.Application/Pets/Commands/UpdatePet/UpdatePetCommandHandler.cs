using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Commands.UpdatePet;

public class UpdatePetCommandHandler : IRequestHandler<UpdatePetCommand, PetDto>
{
    private readonly IPetRepository _petRepository;

    public UpdatePetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<PetDto> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.Id} not found");
        }

        // Update pet properties
        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.DateOfBirth = request.DateOfBirth;
        pet.Color = request.Color;
        pet.Weight = request.Weight;
        pet.MedicalNotes = request.MedicalNotes;
        pet.IsActive = request.IsActive;
        pet.UpdatedAt = DateTime.UtcNow;

        var updatedPet = await _petRepository.UpdateAsync(pet, cancellationToken);

        return new PetDto(
            updatedPet.Id,
            updatedPet.Name,
            updatedPet.Species,
            updatedPet.Breed,
            updatedPet.DateOfBirth,
            updatedPet.Color,
            updatedPet.Weight,
            updatedPet.MedicalNotes,
            updatedPet.IsActive,
            updatedPet.CreatedAt,
            updatedPet.UpdatedAt,
            updatedPet.OwnerUserId,
            null, // OwnerFullName will be populated in controller
            updatedPet.AgeInYears
        );
    }
}