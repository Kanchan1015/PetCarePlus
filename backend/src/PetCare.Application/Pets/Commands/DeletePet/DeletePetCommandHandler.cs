using MediatR;
using PetCare.Application.Common.Interfaces;

namespace PetCare.Application.Pets.Commands.DeletePet;

public class DeletePetCommandHandler : IRequestHandler<DeletePetCommand>
{
    private readonly IPetRepository _petRepository;

    public DeletePetCommandHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        if (!await _petRepository.ExistsAsync(request.Id, cancellationToken))
        {
            throw new ArgumentException($"Pet with ID {request.Id} not found");
        }

        await _petRepository.DeleteAsync(request.Id, cancellationToken);
    }
}