using MediatR;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Queries.GetPets;

public record GetPetsQuery() : IRequest<IReadOnlyList<PetSummaryDto>>;