using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.DTOs;
using PetCare.Domain.Pets;

namespace PetCare.Application.Pets.Commands.CreatePet;

public class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, PetDto>
{
    private readonly IPetRepository _petRepository;

    public CreatePetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<PetDto> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = new Pet
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            DateOfBirth = request.DateOfBirth,
            Color = request.Color,
            Weight = request.Weight,
            MedicalNotes = request.MedicalNotes,
            OwnerUserId = request.OwnerUserId,
            CreatedAt = DateTime.UtcNow
        };

        var createdPet = await _petRepository.AddAsync(pet, cancellationToken);

        return new PetDto(
            createdPet.Id,
            createdPet.Name,
            createdPet.Species,
            createdPet.Breed,
            createdPet.DateOfBirth,
            createdPet.Color,
            createdPet.Weight,
            createdPet.MedicalNotes,
            createdPet.IsActive,
            createdPet.CreatedAt,
            createdPet.UpdatedAt,
            createdPet.OwnerUserId,
            null, // OwnerFullName will be populated in controller
            createdPet.AgeInYears
        );
    }
}