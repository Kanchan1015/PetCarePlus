using FluentValidation;

namespace PetCare.Application.Pets.Commands.UpdatePet;

public class UpdatePetCommandValidator : AbstractValidator<UpdatePetCommand>
{
    public UpdatePetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pet ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pet name is required")
            .Length(1, 60).WithMessage("Pet name must be between 1 and 60 characters");

        RuleFor(x => x.Species)
            .IsInEnum().WithMessage("Valid species is required");

        RuleFor(x => x.Breed)
            .MaximumLength(60).WithMessage("Breed must not exceed 60 characters");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date of birth cannot be in the future")
            .GreaterThan(DateTime.Today.AddYears(-50)).WithMessage("Date of birth cannot be more than 50 years ago")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Color)
            .MaximumLength(30).WithMessage("Color must not exceed 30 characters");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .LessThan(1000).WithMessage("Weight must be less than 1000 kg")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(1000).WithMessage("Medical notes must not exceed 1000 characters");
    }
}