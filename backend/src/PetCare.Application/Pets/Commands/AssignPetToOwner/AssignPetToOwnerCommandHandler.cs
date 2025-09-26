using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Commands.AssignPetToOwner;

public class AssignPetToOwnerCommandHandler : IRequestHandler<AssignPetToOwnerCommand, PetDto>
{
    private readonly IPetRepository _petRepository;

    public AssignPetToOwnerCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<PetDto> Handle(AssignPetToOwnerCommand request, CancellationToken cancellationToken)
    {
        // Verify pet exists
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.PetId} not found");
        }

        // Update pet ownership
        pet.OwnerUserId = request.NewOwnerUserId;
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