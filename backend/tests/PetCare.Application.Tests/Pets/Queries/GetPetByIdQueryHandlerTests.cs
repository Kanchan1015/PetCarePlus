using FluentAssertions;
using Moq;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.Queries.GetPetById;
using PetCare.Domain.Pets;

namespace PetCare.Application.Tests.Pets.Queries;

public class GetPetByIdQueryHandlerTests
{
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly GetPetByIdQueryHandler _handler;

    public GetPetByIdQueryHandlerTests()
    {
        _petRepositoryMock = new Mock<IPetRepository>();
        _handler = new GetPetByIdQueryHandler(_petRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPetExists_ShouldReturnPetDto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = new Pet
        {
            Id = petId,
            Name = "Buddy",
            Species = Species.Dog,
            Breed = "Golden Retriever",
            DateOfBirth = new DateTime(2020, 5, 15),
            Color = "Golden",
            Weight = 25.5m,
            MedicalNotes = "No allergies",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            OwnerUserId = "user-123"
        };

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        var query = new GetPetByIdQuery(petId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(petId);
        result.Name.Should().Be("Buddy");
        result.Species.Should().Be(Species.Dog);
        result.Breed.Should().Be("Golden Retriever");
        result.DateOfBirth.Should().Be(new DateTime(2020, 5, 15));
        result.Color.Should().Be("Golden");
        result.Weight.Should().Be(25.5m);
        result.MedicalNotes.Should().Be("No allergies");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(pet.CreatedAt);
        result.UpdatedAt.Should().Be(pet.UpdatedAt);
        result.OwnerUserId.Should().Be("user-123");
        result.OwnerFullName.Should().BeNull(); // Will be populated in controller
        result.AgeInYears.Should().Be(pet.AgeInYears);

        _petRepositoryMock.Verify(
            x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenPetDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var petId = Guid.NewGuid();

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);

        var query = new GetPetByIdQuery(petId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _petRepositoryMock.Verify(
            x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_PetWithNullOptionalFields_ShouldMapCorrectly()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = new Pet
        {
            Id = petId,
            Name = "Simple Pet",
            Species = Species.Cat,
            Breed = null,
            DateOfBirth = null,
            Color = null,
            Weight = null,
            MedicalNotes = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            OwnerUserId = "user-456"
        };

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        var query = new GetPetByIdQuery(petId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Simple Pet");
        result.Species.Should().Be(Species.Cat);
        result.Breed.Should().BeNull();
        result.DateOfBirth.Should().BeNull();
        result.Color.Should().BeNull();
        result.Weight.Should().BeNull();
        result.MedicalNotes.Should().BeNull();
        result.UpdatedAt.Should().BeNull();
        result.AgeInYears.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var pet = new Pet
        {
            Id = petId,
            Name = "Test Pet",
            Species = Species.Dog,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            OwnerUserId = "user-789"
        };

        var cancellationToken = new CancellationToken();

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, cancellationToken))
            .ReturnsAsync(pet);

        var query = new GetPetByIdQuery(petId);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _petRepositoryMock.Verify(
            x => x.GetByIdAsync(petId, cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_PetWithCompleteData_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var updatedAt = DateTime.UtcNow.AddDays(-5);
        var dateOfBirth = new DateTime(2018, 8, 20);

        var pet = new Pet
        {
            Id = petId,
            Name = "Comprehensive Pet",
            Species = Species.Rabbit,
            Breed = "Holland Lop",
            DateOfBirth = dateOfBirth,
            Color = "Brown and White",
            Weight = 2.5m,
            MedicalNotes = "Sensitive to certain vegetables",
            IsActive = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            OwnerUserId = "user-comprehensive"
        };

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        var query = new GetPetByIdQuery(petId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(petId);
        result.Name.Should().Be("Comprehensive Pet");
        result.Species.Should().Be(Species.Rabbit);
        result.Breed.Should().Be("Holland Lop");
        result.DateOfBirth.Should().Be(dateOfBirth);
        result.Color.Should().Be("Brown and White");
        result.Weight.Should().Be(2.5m);
        result.MedicalNotes.Should().Be("Sensitive to certain vegetables");
        result.IsActive.Should().BeFalse();
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
        result.OwnerUserId.Should().Be("user-comprehensive");
        result.OwnerFullName.Should().BeNull();
        result.AgeInYears.Should().Be(pet.AgeInYears);
    }
}