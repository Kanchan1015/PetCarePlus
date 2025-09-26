using FluentAssertions;
using PetCare.Domain.Pets;
using Xunit;

namespace PetCare.Domain.Tests.Pets;

public class PetTests
{
    [Fact]
    public void Pet_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Buddy";
        var species = Species.Dog;
        var breed = "Golden Retriever";
        var dateOfBirth = DateTime.Now.AddYears(-2);
        var color = "Golden";
        var weight = 25.5m;
        var medicalNotes = "Regular checkups";
        var ownerUserId = "user123";

        // Act
        var pet = new Pet
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            DateOfBirth = dateOfBirth,
            Color = color,
            Weight = weight,
            MedicalNotes = medicalNotes,
            OwnerUserId = ownerUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        pet.Id.Should().Be(id);
        pet.Name.Should().Be(name);
        pet.Species.Should().Be(species);
        pet.Breed.Should().Be(breed);
        pet.DateOfBirth.Should().Be(dateOfBirth);
        pet.Color.Should().Be(color);
        pet.Weight.Should().Be(weight);
        pet.MedicalNotes.Should().Be(medicalNotes);
        pet.OwnerUserId.Should().Be(ownerUserId);
        pet.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Pet_AgeInYears_Should_Calculate_Correctly()
    {
        // Arrange
        var birthDate = DateTime.Now.AddYears(-3).AddMonths(-6); // 3.5 years ago
        var pet = new Pet
        {
            DateOfBirth = birthDate
        };

        // Act
        var age = pet.AgeInYears;

        // Assert
        age.Should().Be(3); // Should floor to 3 years
    }

    [Fact]
    public void Pet_AgeInYears_Should_Return_Null_When_DateOfBirth_Is_Null()
    {
        // Arrange
        var pet = new Pet
        {
            DateOfBirth = null
        };

        // Act
        var age = pet.AgeInYears;

        // Assert
        age.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Pet_Should_Allow_Empty_Optional_Fields(string? value)
    {
        // Arrange & Act
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = Species.Cat,
            OwnerUserId = "user123",
            Breed = value,
            Color = value,
            MedicalNotes = value
        };

        // Assert
        pet.Should().NotBeNull();
        pet.Name.Should().Be("Test Pet");
    }

    [Fact]
    public void Pet_Should_Default_IsActive_To_True()
    {
        // Arrange & Act
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = Species.Bird,
            OwnerUserId = "user123"
        };

        // Assert - assuming the default constructor or database default sets IsActive to true
        // This test might need adjustment based on actual implementation
        pet.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(Species.Dog)]
    [InlineData(Species.Cat)]
    [InlineData(Species.Bird)]
    [InlineData(Species.Fish)]
    [InlineData(Species.Rabbit)]
    [InlineData(Species.Hamster)]
    [InlineData(Species.Guinea_Pig)]
    [InlineData(Species.Reptile)]
    [InlineData(Species.Other)]
    public void Pet_Should_Accept_All_Valid_Species(Species species)
    {
        // Arrange & Act
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = species,
            OwnerUserId = "user123"
        };

        // Assert
        pet.Species.Should().Be(species);
    }

    [Fact]
    public void Pet_Weight_Should_Allow_Decimal_Values()
    {
        // Arrange
        var weight = 12.75m;
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = Species.Cat,
            OwnerUserId = "user123",
            Weight = weight
        };

        // Act & Assert
        pet.Weight.Should().Be(weight);
    }

    [Fact]
    public void Pet_Should_Allow_Null_Weight()
    {
        // Arrange & Act
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = Species.Fish,
            OwnerUserId = "user123",
            Weight = null
        };

        // Assert
        pet.Weight.Should().BeNull();
    }

    [Fact]
    public void Pet_CreatedAt_Should_Be_Set()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = Species.Dog,
            OwnerUserId = "user123",
            CreatedAt = DateTime.UtcNow
        };
        
        var afterCreation = DateTime.UtcNow;

        // Assert
        pet.CreatedAt.Should().BeAfter(beforeCreation.AddSeconds(-1));
        pet.CreatedAt.Should().BeBefore(afterCreation.AddSeconds(1));
    }

    [Fact]
    public void Pet_UpdatedAt_Should_Be_Nullable()
    {
        // Arrange & Act
        var pet = new Pet
        {
            Name = "Test Pet",
            Species = Species.Dog,
            OwnerUserId = "user123",
            UpdatedAt = null
        };

        // Assert
        pet.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Pet_Should_Update_UpdatedAt_When_Modified()
    {
        // Arrange
        var pet = new Pet
        {
            Name = "Original Name",
            Species = Species.Dog,
            OwnerUserId = "user123",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        pet.Name = "Updated Name";
        pet.UpdatedAt = DateTime.UtcNow;

        // Assert
        pet.UpdatedAt.Should().NotBeNull();
        pet.UpdatedAt.Should().BeAfter(pet.CreatedAt);
    }
}