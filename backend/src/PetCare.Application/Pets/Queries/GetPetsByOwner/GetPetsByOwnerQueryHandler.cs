using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Queries.GetPetsByOwner;

public class GetPetsByOwnerQueryHandler : IRequestHandler<GetPetsByOwnerQuery, IReadOnlyList<PetDto>>
{
    private readonly IPetRepository _petRepository;

    public GetPetsByOwnerQueryHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<IReadOnlyList<PetDto>> Handle(GetPetsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var pets = await _petRepository.GetByOwnerIdAsync(request.OwnerUserId, cancellationToken);
        var result = new List<PetDto>();

        foreach (var pet in pets)
        {
            result.Add(new PetDto(
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
            ));
        }

        return result;
    }
}