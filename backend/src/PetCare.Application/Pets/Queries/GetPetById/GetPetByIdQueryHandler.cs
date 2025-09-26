using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Queries.GetPetById;

public class GetPetByIdQueryHandler : IRequestHandler<GetPetByIdQuery, PetDto?>
{
    private readonly IPetRepository _petRepository;

    public GetPetByIdQueryHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<PetDto?> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.Id, cancellationToken);
        if (pet == null)
        {
            return null;
        }

        return new PetDto(
            pet.Id,
            pet.Name,
            pet.Species,
            pet.Breed,
            pet.DateOfBirth,
            pet.Color,
            pet.Weight,
            pet.MedicalNotes,
            pet.IsActive,
            pet.CreatedAt,
            pet.UpdatedAt,
            pet.OwnerUserId,
            null, // OwnerFullName will be populated in controller
            pet.AgeInYears
        );
    }
}