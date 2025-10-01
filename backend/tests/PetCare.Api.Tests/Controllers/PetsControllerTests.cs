using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetCare.Api.Controllers;
using PetCare.Application.Pets.Commands.AssignPetToOwner;
using PetCare.Application.Pets.Commands.CreatePet;
using PetCare.Application.Pets.Commands.DeletePet;
using PetCare.Application.Pets.Commands.UpdatePet;
using PetCare.Application.Pets.DTOs;
using PetCare.Application.Pets.Queries.GetPetById;
using PetCare.Application.Pets.Queries.GetPets;
using PetCare.Application.Pets.Queries.GetPetsByOwner;
using PetCare.Domain.Pets;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PetCare.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace PetCare.Api.Tests.Controllers;

public class PetsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly PetsController _controller;

    public PetsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _userManagerMock = CreateMockUserManager();
        _controller = new PetsController(_mediatorMock.Object, _userManagerMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            optionsAccessor.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);
    }

    private void SetupUserClaims(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new("sub", userId),
            new(ClaimTypes.Role, role.ToUpperInvariant()) // Normalize role to uppercase to match JWT behavior
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    #region GetAllPets Tests

    [Fact]
    public async Task GetAllPets_WhenCalled_ShouldReturnOkWithPetSummaryList()
    {
        // Arrange
        var expectedPets = new List<PetSummaryDto>
        {
            new(Guid.NewGuid(), "Buddy", Species.Dog, "Golden Retriever", 3, true, "John Doe"),
            new(Guid.NewGuid(), "Whiskers", Species.Cat, "Persian", 2, true, "Jane Smith")
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPetsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPets);

        // Act
        var result = await _controller.GetAllPets();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPets = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PetSummaryDto>>().Subject;
        returnedPets.Should().HaveCount(2);
        returnedPets.Should().BeEquivalentTo(expectedPets);

        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetPetsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region GetPetById Tests

    [Fact]
    public async Task GetPetById_AsAdmin_WhenPetExists_ShouldReturnOkWithPet()
    {
        // Arrange
        SetupUserClaims("admin-user", "Admin");
        var petId = Guid.NewGuid();
        var expectedPet = new PetDto(
            petId, "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, "owner-123", "John Doe", 3);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetByIdQuery>(q => q.Id == petId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPet);

        // Act
        var result = await _controller.GetPetById(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPet = okResult.Value.Should().BeOfType<PetDto>().Subject;
        returnedPet.Should().BeEquivalentTo(expectedPet);
    }

    [Fact]
    public async Task GetPetById_AsOwner_WhenAccessingOwnPet_ShouldReturnOkWithPet()
    {
        // Arrange
        var userId = "owner-123";
        SetupUserClaims(userId, "Owner");
        var petId = Guid.NewGuid();
        var expectedPet = new PetDto(
            petId, "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, userId, "John Doe", 3);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetByIdQuery>(q => q.Id == petId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPet);

        // Act
        var result = await _controller.GetPetById(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPet = okResult.Value.Should().BeOfType<PetDto>().Subject;
        returnedPet.Should().BeEquivalentTo(expectedPet);
    }

    [Fact]
    public async Task GetPetById_AsOwner_WhenAccessingOthersPet_ShouldReturnForbid()
    {
        // Arrange
        SetupUserClaims("owner-123", "Owner");
        var petId = Guid.NewGuid();
        var otherOwnerPet = new PetDto(
            petId, "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, "other-owner", "Other Owner", 3);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetByIdQuery>(q => q.Id == petId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherOwnerPet);

        // Act
        var result = await _controller.GetPetById(petId);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetPetById_WhenPetNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetupUserClaims("admin-user", "Admin");
        var petId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetByIdQuery>(q => q.Id == petId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PetDto?)null);

        // Act
        var result = await _controller.GetPetById(petId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Pet with ID {petId} not found");
    }

    #endregion

    #region GetPetsByOwner Tests

    [Fact]
    public async Task GetPetsByOwner_AsAdmin_ShouldReturnOkWithPets()
    {
        // Arrange
        SetupUserClaims("admin-user", "Admin");
        var ownerId = "owner-123";
        var expectedPets = new List<PetDto>
        {
            new(Guid.NewGuid(), "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
                "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, ownerId, "John Doe", 3)
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetsByOwnerQuery>(q => q.OwnerUserId == ownerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPets);

        // Act
        var result = await _controller.GetPetsByOwner(ownerId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPets = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PetDto>>().Subject;
        returnedPets.Should().BeEquivalentTo(expectedPets);
    }

    [Fact]
    public async Task GetPetsByOwner_AsOwner_WhenAccessingOwnPets_ShouldReturnOkWithPets()
    {
        // Arrange
        var userId = "owner-123";
        SetupUserClaims(userId, "Owner");
        var expectedPets = new List<PetDto>
        {
            new(Guid.NewGuid(), "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
                "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, userId, "John Doe", 3)
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetsByOwnerQuery>(q => q.OwnerUserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPets);

        // Act
        var result = await _controller.GetPetsByOwner(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPets = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PetDto>>().Subject;
        returnedPets.Should().BeEquivalentTo(expectedPets);
    }

    [Fact]
    public async Task GetPetsByOwner_AsOwner_WhenAccessingOthersPets_ShouldReturnForbid()
    {
        // Arrange
        SetupUserClaims("owner-123", "Owner");
        var otherOwnerId = "other-owner";

        // Act
        var result = await _controller.GetPetsByOwner(otherOwnerId);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region CreatePet Tests

    [Fact]
    public async Task CreatePet_ValidRequest_ShouldReturnCreatedAtActionWithPet()
    {
        // Arrange
        var createPetDto = new CreatePetDto(
            "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", "owner-123");

        var expectedPet = new PetDto(
            Guid.NewGuid(), "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, "owner-123", "John Doe", 3);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPet);

        // Act
        var result = await _controller.CreatePet(createPetDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PetsController.GetPetById));
        createdResult.RouteValues!["id"].Should().Be(expectedPet.Id);
        var returnedPet = createdResult.Value.Should().BeOfType<PetDto>().Subject;
        returnedPet.Should().BeEquivalentTo(expectedPet);

        _mediatorMock.Verify(
            x => x.Send(It.Is<CreatePetCommand>(c =>
                c.Name == createPetDto.Name &&
                c.Species == createPetDto.Species &&
                c.Breed == createPetDto.Breed &&
                c.DateOfBirth == createPetDto.DateOfBirth &&
                c.Color == createPetDto.Color &&
                c.Weight == createPetDto.Weight &&
                c.MedicalNotes == createPetDto.MedicalNotes &&
                c.OwnerUserId == createPetDto.OwnerUserId
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreatePet_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var createPetDto = new CreatePetDto(
            "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", "invalid-owner");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePetCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Owner not found"));

        // Act
        var result = await _controller.CreatePet(createPetDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Owner not found");
    }

    #endregion

    #region UpdatePet Tests

    [Fact]
    public async Task UpdatePet_ValidRequest_ShouldReturnOkWithUpdatedPet()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var updatePetDto = new UpdatePetDto(
            "Updated Buddy", Species.Dog, "Labrador", new DateTime(2020, 5, 15),
            "Yellow", 27.0m, "Updated notes", true);

        var expectedPet = new PetDto(
            petId, "Updated Buddy", Species.Dog, "Labrador", new DateTime(2020, 5, 15),
            "Yellow", 27.0m, "Updated notes", true, DateTime.UtcNow, DateTime.UtcNow, "owner-123", "John Doe", 3);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPet);

        // Act
        var result = await _controller.UpdatePet(petId, updatePetDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPet = okResult.Value.Should().BeOfType<PetDto>().Subject;
        returnedPet.Should().BeEquivalentTo(expectedPet);

        _mediatorMock.Verify(
            x => x.Send(It.Is<UpdatePetCommand>(c =>
                c.Id == petId &&
                c.Name == updatePetDto.Name &&
                c.Species == updatePetDto.Species &&
                c.Breed == updatePetDto.Breed &&
                c.DateOfBirth == updatePetDto.DateOfBirth &&
                c.Color == updatePetDto.Color &&
                c.Weight == updatePetDto.Weight &&
                c.MedicalNotes == updatePetDto.MedicalNotes &&
                c.IsActive == updatePetDto.IsActive
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdatePet_WhenPetNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var updatePetDto = new UpdatePetDto(
            "Updated Buddy", Species.Dog, "Labrador", new DateTime(2020, 5, 15),
            "Yellow", 27.0m, "Updated notes", true);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException($"Pet with ID {petId} not found"));

        // Act
        var result = await _controller.UpdatePet(petId, updatePetDto);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Pet with ID {petId} not found");
    }

    #endregion

    #region DeletePet Tests

    [Fact]
    public async Task DeletePet_ValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        var petId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DeletePetCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePet(petId);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mediatorMock.Verify(
            x => x.Send(It.Is<DeletePetCommand>(c => c.Id == petId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeletePet_WhenPetNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var petId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DeletePetCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException($"Pet with ID {petId} not found"));

        // Act
        var result = await _controller.DeletePet(petId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be($"Pet with ID {petId} not found");
    }

    #endregion

    #region AssignPetToOwner Tests

    [Fact]
    public async Task AssignPetToOwner_ValidRequest_ShouldReturnOkWithUpdatedPet()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var assignPetDto = new AssignPetDto(petId, "new-owner-123");

        var expectedPet = new PetDto(
            petId, "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
            "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, DateTime.UtcNow, "new-owner-123", "New Owner", 3);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<AssignPetToOwnerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPet);

        // Act
        var result = await _controller.AssignPetToOwner(petId, assignPetDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPet = okResult.Value.Should().BeOfType<PetDto>().Subject;
        returnedPet.Should().BeEquivalentTo(expectedPet);

        _mediatorMock.Verify(
            x => x.Send(It.Is<AssignPetToOwnerCommand>(c =>
                c.PetId == petId &&
                c.NewOwnerUserId == assignPetDto.NewOwnerUserId
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AssignPetToOwner_WhenPetIdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var urlPetId = Guid.NewGuid();
        var bodyPetId = Guid.NewGuid();
        var assignPetDto = new AssignPetDto(bodyPetId, "new-owner-123");

        // Act
        var result = await _controller.AssignPetToOwner(urlPetId, assignPetDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Pet ID in URL does not match Pet ID in request body");
    }

    [Fact]
    public async Task AssignPetToOwner_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var assignPetDto = new AssignPetDto(petId, "invalid-owner");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<AssignPetToOwnerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("New owner not found"));

        // Act
        var result = await _controller.AssignPetToOwner(petId, assignPetDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("New owner not found");
    }

    #endregion

    #region GetMyPets Tests

    [Fact]
    public async Task GetMyPets_WithValidUser_ShouldReturnOkWithUsersPets()
    {
        // Arrange
        var userId = "owner-123";
        SetupUserClaims(userId, "Owner");
        var expectedPets = new List<PetDto>
        {
            new(Guid.NewGuid(), "Buddy", Species.Dog, "Golden Retriever", new DateTime(2020, 5, 15),
                "Golden", 25.5m, "No allergies", true, DateTime.UtcNow, null, userId, "John Doe", 3)
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetPetsByOwnerQuery>(q => q.OwnerUserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPets);

        // Act
        var result = await _controller.GetMyPets();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPets = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PetDto>>().Subject;
        returnedPets.Should().BeEquivalentTo(expectedPets);

        _mediatorMock.Verify(
            x => x.Send(It.Is<GetPetsByOwnerQuery>(q => q.OwnerUserId == userId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetMyPets_WithoutUserIdInToken_ShouldReturnUnauthorized()
    {
        // Arrange - setup controller without user claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        // Act
        var result = await _controller.GetMyPets();

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().Be("User ID not found in token");
    }

    #endregion
}