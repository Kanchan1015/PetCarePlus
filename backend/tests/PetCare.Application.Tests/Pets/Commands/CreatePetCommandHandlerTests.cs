using FluentAssertions;
using Moq;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.Commands.CreatePet;
using PetCare.Domain.Pets;

namespace PetCare.Application.Tests.Pets.Commands;

public class CreatePetCommandHandlerTests
{
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly CreatePetCommandHandler _handler;

    public CreatePetCommandHandlerTests()
    {
        _petRepositoryMock = new Mock<IPetRepository>();
        _handler = new CreatePetCommandHandler(_petRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePetAndReturnDto()
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

        var createdPet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Species = command.Species,
            Breed = command.Breed,
            DateOfBirth = command.DateOfBirth,
            Color = command.Color,
            Weight = command.Weight,
            MedicalNotes = command.MedicalNotes,
            OwnerUserId = command.OwnerUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _petRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdPet.Id);
        result.Name.Should().Be(command.Name);
        result.Species.Should().Be(command.Species);
        result.Breed.Should().Be(command.Breed);
        result.DateOfBirth.Should().Be(command.DateOfBirth);
        result.Color.Should().Be(command.Color);
        result.Weight.Should().Be(command.Weight);
        result.MedicalNotes.Should().Be(command.MedicalNotes);
        result.OwnerUserId.Should().Be(command.OwnerUserId);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.AgeInYears.Should().Be(createdPet.AgeInYears);

        _petRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Pet>(p =>
                p.Name == command.Name &&
                p.Species == command.Species &&
                p.Breed == command.Breed &&
                p.DateOfBirth == command.DateOfBirth &&
                p.Color == command.Color &&
                p.Weight == command.Weight &&
                p.MedicalNotes == command.MedicalNotes &&
                p.OwnerUserId == command.OwnerUserId &&
                p.CreatedAt <= DateTime.UtcNow
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CommandWithoutOptionalFields_ShouldCreatePetWithNullValues()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Whiskers",
            Species: Species.Cat,
            Breed: null,
            DateOfBirth: null,
            Color: null,
            Weight: null,
            MedicalNotes: null,
            OwnerUserId: "user-456"
        );

        var createdPet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Species = command.Species,
            Breed = command.Breed,
            DateOfBirth = command.DateOfBirth,
            Color = command.Color,
            Weight = command.Weight,
            MedicalNotes = command.MedicalNotes,
            OwnerUserId = command.OwnerUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _petRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Whiskers");
        result.Species.Should().Be(Species.Cat);
        result.Breed.Should().BeNull();
        result.DateOfBirth.Should().BeNull();
        result.Color.Should().BeNull();
        result.Weight.Should().BeNull();
        result.MedicalNotes.Should().BeNull();
        result.OwnerUserId.Should().Be("user-456");
        result.AgeInYears.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CommandWithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var command = new CreatePetCommand(
            Name: "Rex",
            Species: Species.Dog,
            Breed: "German Shepherd",
            DateOfBirth: new DateTime(2019, 3, 10),
            Color: "Black",
            Weight: 30.0m,
            MedicalNotes: "Hip dysplasia",
            OwnerUserId: "user-789"
        );

        var createdPet = new Pet
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Species = command.Species,
            OwnerUserId = command.OwnerUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var cancellationToken = new CancellationToken();

        _petRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Pet>(), cancellationToken))
            .ReturnsAsync(createdPet);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _petRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Pet>(), cancellationToken),
            Times.Once
        );
    }
}