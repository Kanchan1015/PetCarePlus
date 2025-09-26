using MediatR;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Queries.GetPetsByOwner;

public record GetPetsByOwnerQuery(string OwnerUserId) : IRequest<IReadOnlyList<PetDto>>;