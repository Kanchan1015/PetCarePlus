using MediatR;
using PetCare.Application.Pets.DTOs;

namespace PetCare.Application.Pets.Commands.AssignPetToOwner;

public record AssignPetToOwnerCommand(
    Guid PetId,
    string NewOwnerUserId
) : IRequest<PetDto>;