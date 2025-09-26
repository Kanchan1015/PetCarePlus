using FluentAssertions;
using Moq;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.Commands.UpdatePet;
using PetCare.Domain.Pets;

namespace PetCare.Application.Tests.Pets.Commands;

public class UpdatePetCommandHandlerTests
{
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly UpdatePetCommandHandler _handler;

    public UpdatePetCommandHandlerTests()
    {
        _petRepositoryMock = new Mock<IPetRepository>();
        _handler = new UpdatePetCommandHandler(_petRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePetAndReturnDto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var existingPet = new Pet
        {
            Id = petId,
            Name = "Old Name",
            Species = Species.Dog,
            Breed = "Old Breed",
            DateOfBirth = new DateTime(2020, 1, 1),
            Color = "Old Color",
            Weight = 20.0m,
            MedicalNotes = "Old notes",
            IsActive = true,
            OwnerUserId = "user-123",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var command = new UpdatePetCommand(
            Id: petId,
            Name: "Updated Name",
            Species: Species.Cat,
            Breed: "Updated Breed",
            DateOfBirth: new DateTime(2021, 5, 15),
            Color: "Updated Color",
            Weight: 15.5m,
            MedicalNotes: "Updated notes",
            IsActive: false
        );

        var updatedPet = new Pet
        {
            Id = petId,
            Name = command.Name,
            Species = command.Species,
            Breed = command.Breed,
            DateOfBirth = command.DateOfBirth,
            Color = command.Color,
            Weight = command.Weight,
            MedicalNotes = command.MedicalNotes,
            IsActive = command.IsActive,
            OwnerUserId = existingPet.OwnerUserId,
            CreatedAt = existingPet.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        _petRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(petId);
        result.Name.Should().Be(command.Name);
        result.Species.Should().Be(command.Species);
        result.Breed.Should().Be(command.Breed);
        result.DateOfBirth.Should().Be(command.DateOfBirth);
        result.Color.Should().Be(command.Color);
        result.Weight.Should().Be(command.Weight);
        result.MedicalNotes.Should().Be(command.MedicalNotes);
        result.IsActive.Should().Be(command.IsActive);
        result.OwnerUserId.Should().Be(existingPet.OwnerUserId);
        result.CreatedAt.Should().Be(existingPet.CreatedAt);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _petRepositoryMock.Verify(
            x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _petRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Pet>(p =>
                p.Id == petId &&
                p.Name == command.Name &&
                p.Species == command.Species &&
                p.Breed == command.Breed &&
                p.DateOfBirth == command.DateOfBirth &&
                p.Color == command.Color &&
                p.Weight == command.Weight &&
                p.MedicalNotes == command.MedicalNotes &&
                p.IsActive == command.IsActive &&
                p.UpdatedAt <= DateTime.UtcNow
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_PetNotFound_ShouldThrowArgumentException()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var command = new UpdatePetCommand(
            Id: petId,
            Name: "Updated Name",
            Species: Species.Cat,
            Breed: "Updated Breed",
            DateOfBirth: new DateTime(2021, 5, 15),
            Color: "Updated Color",
            Weight: 15.5m,
            MedicalNotes: "Updated notes",
            IsActive: false
        );

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be($"Pet with ID {petId} not found");

        _petRepositoryMock.Verify(
            x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _petRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_UpdateWithNullOptionalFields_ShouldUpdatePetCorrectly()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var existingPet = new Pet
        {
            Id = petId,
            Name = "Buddy",
            Species = Species.Dog,
            Breed = "Golden Retriever",
            DateOfBirth = new DateTime(2020, 1, 1),
            Color = "Golden",
            Weight = 25.0m,
            MedicalNotes = "Some notes",
            IsActive = true,
            OwnerUserId = "user-123",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var command = new UpdatePetCommand(
            Id: petId,
            Name: "Updated Buddy",
            Species: Species.Dog,
            Breed: null,
            DateOfBirth: null,
            Color: null,
            Weight: null,
            MedicalNotes: null,
            IsActive: true
        );

        var updatedPet = new Pet
        {
            Id = petId,
            Name = command.Name,
            Species = command.Species,
            Breed = command.Breed,
            DateOfBirth = command.DateOfBirth,
            Color = command.Color,
            Weight = command.Weight,
            MedicalNotes = command.MedicalNotes,
            IsActive = command.IsActive,
            OwnerUserId = existingPet.OwnerUserId,
            CreatedAt = existingPet.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        _petRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Buddy");
        result.Breed.Should().BeNull();
        result.DateOfBirth.Should().BeNull();
        result.Color.Should().BeNull();
        result.Weight.Should().BeNull();
        result.MedicalNotes.Should().BeNull();
        result.AgeInYears.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeactivatePet_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var existingPet = new Pet
        {
            Id = petId,
            Name = "Active Pet",
            Species = Species.Dog,
            IsActive = true,
            OwnerUserId = "user-123",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var command = new UpdatePetCommand(
            Id: petId,
            Name: "Active Pet",
            Species: Species.Dog,
            Breed: null,
            DateOfBirth: null,
            Color: null,
            Weight: null,
            MedicalNotes: null,
            IsActive: false
        );

        var updatedPet = new Pet
        {
            Id = petId,
            Name = command.Name,
            Species = command.Species,
            IsActive = false,
            OwnerUserId = existingPet.OwnerUserId,
            CreatedAt = existingPet.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPet);

        _petRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsActive.Should().BeFalse();

        _petRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Pet>(p => p.IsActive == false), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CommandWithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var existingPet = new Pet
        {
            Id = petId,
            Name = "Test Pet",
            Species = Species.Dog,
            IsActive = true,
            OwnerUserId = "user-123",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var command = new UpdatePetCommand(
            Id: petId,
            Name: "Updated Pet",
            Species: Species.Dog,
            Breed: null,
            DateOfBirth: null,
            Color: null,
            Weight: null,
            MedicalNotes: null,
            IsActive: true
        );

        var cancellationToken = new CancellationToken();

        _petRepositoryMock
            .Setup(x => x.GetByIdAsync(petId, cancellationToken))
            .ReturnsAsync(existingPet);

        _petRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Pet>(), cancellationToken))
            .ReturnsAsync(existingPet);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _petRepositoryMock.Verify(
            x => x.GetByIdAsync(petId, cancellationToken),
            Times.Once
        );

        _petRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Pet>(), cancellationToken),
            Times.Once
        );
    }
}