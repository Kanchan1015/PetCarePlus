using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Queries.GetPets;

public class GetPetsQueryHandler : IRequestHandler<GetPetsQuery, IReadOnlyList<PetSummaryDto>>
{
    private readonly IPetRepository _petRepository;

    public GetPetsQueryHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<IReadOnlyList<PetSummaryDto>> Handle(GetPetsQuery request, CancellationToken cancellationToken)
    {
        var pets = await _petRepository.GetAllAsync(cancellationToken);
        var result = new List<PetSummaryDto>();

        foreach (var pet in pets)
        {
            result.Add(new PetSummaryDto(
                pet.Id,
                pet.Name,
                pet.Species,
                pet.Breed,
                pet.AgeInYears,
                pet.IsActive,
                "Owner" // Will be populated in controller with actual owner name
            ));
        }

        return result;
    }
}