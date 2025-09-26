using FluentAssertions;
using Moq;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Pets.Queries.GetPets;
using PetCare.Domain.Pets;

namespace PetCare.Application.Tests.Pets.Queries;

public class GetPetsQueryHandlerTests
{
    private readonly Mock<IPetRepository> _petRepositoryMock;
    private readonly GetPetsQueryHandler _handler;

    public GetPetsQueryHandlerTests()
    {
        _petRepositoryMock = new Mock<IPetRepository>();
        _handler = new GetPetsQueryHandler(_petRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPetsExist_ShouldReturnPetSummaryDtos()
    {
        // Arrange
        var pets = new List<Pet>
        {
            new Pet
            {
                Id = Guid.NewGuid(),
                Name = "Buddy",
                Species = Species.Dog,
                Breed = "Golden Retriever",
                DateOfBirth = new DateTime(2020, 5, 15),
                IsActive = true,
                OwnerUserId = "user-1",
                CreatedAt = DateTime.UtcNow
            },
            new Pet
            {
                Id = Guid.NewGuid(),
                Name = "Whiskers",
                Species = Species.Cat,
                Breed = "Persian",
                DateOfBirth = new DateTime(2019, 3, 10),
                IsActive = true,
                OwnerUserId = "user-2",
                CreatedAt = DateTime.UtcNow
            },
            new Pet
            {
                Id = Guid.NewGuid(),
                Name = "Tweety",
                Species = Species.Bird,
                Breed = null,
                DateOfBirth = null,
                IsActive = false,
                OwnerUserId = "user-3",
                CreatedAt = DateTime.UtcNow
            }
        };

        _petRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        var query = new GetPetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var buddyDto = result.First(p => p.Name == "Buddy");
        buddyDto.Id.Should().Be(pets[0].Id);
        buddyDto.Name.Should().Be("Buddy");
        buddyDto.Species.Should().Be(Species.Dog);
        buddyDto.Breed.Should().Be("Golden Retriever");
        buddyDto.AgeInYears.Should().Be(pets[0].AgeInYears);
        buddyDto.IsActive.Should().BeTrue();
        buddyDto.OwnerFullName.Should().Be("Owner");

        var whiskersDto = result.First(p => p.Name == "Whiskers");
        whiskersDto.Id.Should().Be(pets[1].Id);
        whiskersDto.Name.Should().Be("Whiskers");
        whiskersDto.Species.Should().Be(Species.Cat);
        whiskersDto.Breed.Should().Be("Persian");
        whiskersDto.AgeInYears.Should().Be(pets[1].AgeInYears);
        whiskersDto.IsActive.Should().BeTrue();
        whiskersDto.OwnerFullName.Should().Be("Owner");

        var tweetyDto = result.First(p => p.Name == "Tweety");
        tweetyDto.Id.Should().Be(pets[2].Id);
        tweetyDto.Name.Should().Be("Tweety");
        tweetyDto.Species.Should().Be(Species.Bird);
        tweetyDto.Breed.Should().BeNull();
        tweetyDto.AgeInYears.Should().BeNull();
        tweetyDto.IsActive.Should().BeFalse();
        tweetyDto.OwnerFullName.Should().Be("Owner");

        _petRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoPetsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyPetList = new List<Pet>();

        _petRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPetList);

        var query = new GetPetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _petRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var pets = new List<Pet>
        {
            new Pet
            {
                Id = Guid.NewGuid(),
                Name = "Test Pet",
                Species = Species.Dog,
                IsActive = true,
                OwnerUserId = "user-1",
                CreatedAt = DateTime.UtcNow
            }
        };

        var cancellationToken = new CancellationToken();

        _petRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(pets);

        var query = new GetPetsQuery();

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _petRepositoryMock.Verify(
            x => x.GetAllAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_PetsWithVariousProperties_ShouldMapCorrectly()
    {
        // Arrange
        var pets = new List<Pet>
        {
            new Pet
            {
                Id = Guid.NewGuid(),
                Name = "Max",
                Species = Species.Dog,
                Breed = "Labrador",
                DateOfBirth = DateTime.Today.AddYears(-5),
                IsActive = true,
                OwnerUserId = "user-1",
                CreatedAt = DateTime.UtcNow
            },
            new Pet
            {
                Id = Guid.NewGuid(),
                Name = "Luna",
                Species = Species.Cat,
                Breed = null, // No breed
                DateOfBirth = null, // No date of birth
                IsActive = false,
                OwnerUserId = "user-2",
                CreatedAt = DateTime.UtcNow
            }
        };

        _petRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pets);

        var query = new GetPetsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        var maxDto = result.First(p => p.Name == "Max");
        maxDto.Breed.Should().Be("Labrador");
        maxDto.AgeInYears.Should().Be(pets[0].AgeInYears);
        maxDto.IsActive.Should().BeTrue();

        var lunaDto = result.First(p => p.Name == "Luna");
        lunaDto.Breed.Should().BeNull();
        lunaDto.AgeInYears.Should().BeNull();
        lunaDto.IsActive.Should().BeFalse();
    }
}