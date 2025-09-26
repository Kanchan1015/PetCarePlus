using MediatR;

namespace PetCare.Application.Pets.Commands.DeletePet;

public record DeletePetCommand(Guid Id) : IRequest;