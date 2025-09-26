using FluentValidation;

namespace PetCare.Application.Pets.Commands.AssignPetToOwner;

public class AssignPetToOwnerCommandValidator : AbstractValidator<AssignPetToOwnerCommand>
{
    public AssignPetToOwnerCommandValidator()
    {
        RuleFor(x => x.PetId)
            .NotEmpty().WithMessage("Pet ID is required");

        RuleFor(x => x.NewOwnerUserId)
            .NotEmpty().WithMessage("New owner user ID is required");
    }
}