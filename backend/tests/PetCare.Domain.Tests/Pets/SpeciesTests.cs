using FluentAssertions;
using PetCare.Domain.Pets;
using Xunit;

namespace PetCare.Domain.Tests.Pets;

public class SpeciesTests
{
    [Fact]
    public void Species_Should_Have_Expected_Values()
    {
        // Assert
        Enum.GetValues<Species>().Should().Contain(new[]
        {
            Species.Dog,
            Species.Cat,
            Species.Bird,
            Species.Fish,
            Species.Rabbit,
            Species.Hamster,
            Species.Guinea_Pig,
            Species.Reptile,
            Species.Other
        });
    }

    [Theory]
    [InlineData(Species.Dog, 1)]
    [InlineData(Species.Cat, 2)]
    [InlineData(Species.Bird, 3)]
    [InlineData(Species.Fish, 4)]
    [InlineData(Species.Rabbit, 5)]
    [InlineData(Species.Hamster, 6)]
    [InlineData(Species.Guinea_Pig, 7)]
    [InlineData(Species.Reptile, 8)]
    [InlineData(Species.Other, 99)]
    public void Species_Should_Have_Correct_Numeric_Values(Species species, int expectedValue)
    {
        // Act & Assert
        ((int)species).Should().Be(expectedValue);
    }

    [Fact]
    public void Species_Should_Be_Convertible_To_String()
    {
        // Arrange
        var species = Species.Dog;

        // Act
        var stringValue = species.ToString();

        // Assert
        stringValue.Should().Be("Dog");
    }

    [Fact]
    public void Species_Should_Be_Parseable_From_String()
    {
        // Arrange
        var speciesString = "Cat";

        // Act
        var success = Enum.TryParse<Species>(speciesString, out var species);

        // Assert
        success.Should().BeTrue();
        species.Should().Be(Species.Cat);
    }

    [Fact]
    public void Species_Count_Should_Be_Nine()
    {
        // Act
        var speciesCount = Enum.GetValues<Species>().Length;

        // Assert
        speciesCount.Should().Be(9);
    }
}