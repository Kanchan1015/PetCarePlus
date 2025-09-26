using MediatR;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Queries.GetPetById;

public record GetPetByIdQuery(Guid Id) : IRequest<PetDto?>;