using FluentAssertions;
using FluentValidation.TestHelper;
using PetCare.Application.Pets.Commands.CreatePet;
using PetCare.Domain.Pets;

namespace PetCare.Application.Tests.Pets.Commands;

public class CreatePetCommandValidatorTests
{
    private readonly CreatePetCommandValidator _validator;

    public CreatePetCommandValidatorTests()
    {
        _validator = new CreatePetCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyOrNullName_ShouldHaveValidationError(string name)
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: name,
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Pet name is required");
    }

    [Fact]
    public void Validate_NullName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: null!,
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Pet name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 61); // 61 characters
        var command = new CreatePetCommand(
            Name: longName,
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Pet name must be between 1 and 60 characters");
    }

    [Fact]
    public void Validate_InvalidSpecies_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: (Species)999, // Invalid enum value
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Species)
            .WithErrorMessage("Valid species is required");
    }

    [Fact]
    public void Validate_BreedTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longBreed = new string('B', 61); // 61 characters
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: longBreed,
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Breed)
            .WithErrorMessage("Breed must not exceed 60 characters");
    }

    [Fact]
    public void Validate_DateOfBirthInFuture_ShouldHaveValidationError()
    {
        // Arrange
        var futureDate = DateTime.Today.AddDays(1);
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: futureDate,
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth cannot be in the future");
    }

    [Fact]
    public void Validate_DateOfBirthTooOld_ShouldHaveValidationError()
    {
        // Arrange
        var tooOldDate = DateTime.Today.AddYears(-51);
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: tooOldDate,
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth cannot be more than 50 years ago");
    }

    [Fact]
    public void Validate_ColorTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longColor = new string('C', 31); // 31 characters
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: longColor,
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Color)
            .WithErrorMessage("Color must not exceed 30 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void Validate_WeightZeroOrNegative_ShouldHaveValidationError(decimal weight)
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: weight,
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Weight)
            .WithErrorMessage("Weight must be greater than 0");
    }

    [Fact]
    public void Validate_WeightTooHigh_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 1000m, // Too high
            MedicalNotes: "No allergies",
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Weight)
            .WithErrorMessage("Weight must be less than 1000 kg");
    }

    [Fact]
    public void Validate_MedicalNotesTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longNotes = new string('N', 1001); // 1001 characters
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: longNotes,
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MedicalNotes)
            .WithErrorMessage("Medical notes must not exceed 1000 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyOrNullOwnerUserId_ShouldHaveValidationError(string ownerUserId)
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: ownerUserId
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OwnerUserId)
            .WithErrorMessage("Owner user ID is required");
    }

    [Fact]
    public void Validate_NullOwnerUserId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            DateOfBirth: new DateTime(2020, 5, 15),
            Color: "Golden",
            Weight: 25.5m,
            MedicalNotes: "No allergies",
            OwnerUserId: null!
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OwnerUserId)
            .WithErrorMessage("Owner user ID is required");
    }

    [Fact]
    public void Validate_NullOptionalFields_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Buddy",
            Species: Species.Dog,
            Breed: null,
            DateOfBirth: null,
            Color: null,
            Weight: null,
            MedicalNotes: null,
            OwnerUserId: "user-123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Breed);
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
        result.ShouldNotHaveValidationErrorFor(x => x.Color);
        result.ShouldNotHaveValidationErrorFor(x => x.Weight);
        result.ShouldNotHaveValidationErrorFor(x => x.MedicalNotes);
    }
}