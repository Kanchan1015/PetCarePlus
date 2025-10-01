using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetCare.Api.Controllers;
using PetCare.Application.MedicalRecords.Commands.CreateMedicalRecord;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Application.MedicalRecords.Queries.GetMedicalRecordsByPet;
using PetCare.Domain.MedicalRecords;
using System.Security.Claims;
using Xunit;
using static PetCare.Api.Controllers.MedicalRecordsController;

namespace PetCare.Api.Tests.Controllers;

public class MedicalRecordsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MedicalRecordsController _controller;

    public MedicalRecordsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MedicalRecordsController(_mediatorMock.Object);
    }

    private void SetupUserClaims(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new("sub", userId),
            new(ClaimTypes.Role, role.ToUpperInvariant())
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

    private void SetupEmptyUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal() // Empty principal with no claims
            }
        };
    }

    #region CreateMedicalRecord Tests

    [Fact]
    public async Task CreateMedicalRecord_AsVet_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var vetId = "vet-123";
        SetupUserClaims(vetId, "Vet");

        var request = new CreateMedicalRecordRequest
        {
            PetId = Guid.NewGuid(),
            RecordType = MedicalRecordType.Checkup,
            RecordDate = DateTime.Today,
            Title = "Annual Checkup",
            Description = "Routine health examination",
            Notes = "Pet is healthy"
        };

        var expectedResult = new MedicalRecordDto(
            Guid.NewGuid(),
            request.PetId,
            vetId,
            "Dr. Smith",
            request.RecordType,
            request.RecordDate,
            request.Title,
            request.Description,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateMedicalRecord(request);

        // Assert
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(MedicalRecordsController.GetMedicalRecordsByPet));
        createdAtActionResult.RouteValues.Should().ContainKey("petId").WhoseValue.Should().Be(request.PetId);
        
        var returnedRecord = createdAtActionResult.Value.Should().BeOfType<MedicalRecordDto>().Subject;
        returnedRecord.Should().BeEquivalentTo(expectedResult);

        _mediatorMock.Verify(x => x.Send(It.Is<CreateMedicalRecordCommand>(cmd => 
            cmd.PetId == request.PetId &&
            cmd.RecordType == request.RecordType &&
            cmd.Title == request.Title &&
            cmd.VetUserId == vetId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMedicalRecord_AsAdmin_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "Admin");

        var request = new CreateMedicalRecordRequest
        {
            PetId = Guid.NewGuid(),
            RecordType = MedicalRecordType.Surgery,
            RecordDate = DateTime.Today,
            Title = "Spay Surgery",
            Description = "Routine spay procedure",
            Notes = "Surgery completed successfully"
        };

        var expectedResult = new MedicalRecordDto(
            Guid.NewGuid(),
            request.PetId,
            adminId,
            "Admin User",
            request.RecordType,
            request.RecordDate,
            request.Title,
            request.Description,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateMedicalRecord(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateMedicalRecord_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateMedicalRecordRequest
        {
            PetId = Guid.NewGuid(),
            RecordType = MedicalRecordType.Checkup,
            RecordDate = DateTime.Today,
            Title = "Test Record",
            Description = "Test Description"
        };

        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.CreateMedicalRecord(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateMedicalRecord_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreateMedicalRecordRequest
        {
            PetId = Guid.NewGuid(),
            RecordType = MedicalRecordType.Checkup,
            RecordDate = DateTime.Today,
            Title = "Test Record"
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Pet not found"));

        // Act
        var result = await _controller.CreateMedicalRecord(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Pet not found" });
    }

    [Fact]
    public async Task CreateMedicalRecord_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreateMedicalRecordRequest
        {
            PetId = Guid.NewGuid(),
            RecordType = MedicalRecordType.Checkup,
            RecordDate = DateTime.Today,
            Title = "Test Record"
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateMedicalRecordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.CreateMedicalRecord(request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region GetMedicalRecordsByPet Tests

    [Fact]
    public async Task GetMedicalRecordsByPet_AsVet_ShouldReturnOkWithRecords()
    {
        // Arrange
        var vetId = "vet-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(vetId, "Vet");

        var expectedRecords = new List<MedicalRecordDto>
        {
            new(Guid.NewGuid(), petId, vetId, "Dr. Smith", MedicalRecordType.Checkup, DateTime.Today.AddDays(-7), 
                "Weekly Checkup", "Routine examination", "All good", DateTime.UtcNow.AddDays(-7), null),
            new(Guid.NewGuid(), petId, vetId, "Dr. Smith", MedicalRecordType.Vaccination, DateTime.Today.AddDays(-30), 
                "Rabies Vaccination", "Annual rabies shot", "No reactions", DateTime.UtcNow.AddDays(-30), null)
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetMedicalRecordsByPetQuery>(q => 
                q.PetId == petId && 
                q.UserId == vetId && 
                q.IsOwner == false
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecords);

        // Act
        var result = await _controller.GetMedicalRecordsByPet(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRecords = okResult.Value.Should().BeAssignableTo<IReadOnlyList<MedicalRecordDto>>().Subject;
        returnedRecords.Should().BeEquivalentTo(expectedRecords);
    }

    [Fact]
    public async Task GetMedicalRecordsByPet_AsOwner_ShouldReturnOkWithRecords()
    {
        // Arrange
        var ownerId = "owner-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(ownerId, "Owner");

        var expectedRecords = new List<MedicalRecordDto>
        {
            new(Guid.NewGuid(), petId, "vet-456", "Dr. Johnson", MedicalRecordType.Checkup, DateTime.Today.AddDays(-14), 
                "Checkup", "Regular health check", "Pet is healthy", DateTime.UtcNow.AddDays(-14), null)
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetMedicalRecordsByPetQuery>(q => 
                q.PetId == petId && 
                q.UserId == ownerId && 
                q.IsOwner == true
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecords);

        // Act
        var result = await _controller.GetMedicalRecordsByPet(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRecords = okResult.Value.Should().BeAssignableTo<IReadOnlyList<MedicalRecordDto>>().Subject;
        returnedRecords.Should().BeEquivalentTo(expectedRecords);
    }

    [Fact]
    public async Task GetMedicalRecordsByPet_AsAdmin_ShouldReturnOkWithRecords()
    {
        // Arrange
        var adminId = "admin-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(adminId, "Admin");

        var expectedRecords = new List<MedicalRecordDto>();

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetMedicalRecordsByPetQuery>(q => 
                q.PetId == petId && 
                q.UserId == adminId && 
                q.IsOwner == false // Admin should be treated like vet/staff
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecords);

        // Act
        var result = await _controller.GetMedicalRecordsByPet(petId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMedicalRecordsByPet_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var petId = Guid.NewGuid();
        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.GetMedicalRecordsByPet(petId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetMedicalRecordsByPet_WhenArgumentException_ShouldReturnNotFound()
    {
        // Arrange
        var petId = Guid.NewGuid();
        SetupUserClaims("vet-123", "Vet");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetMedicalRecordsByPetQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Pet not found"));

        // Act
        var result = await _controller.GetMedicalRecordsByPet(petId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pet not found" });
    }

    [Fact]
    public async Task GetMedicalRecordsByPet_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        var petId = Guid.NewGuid();
        SetupUserClaims("owner-123", "Owner");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetMedicalRecordsByPetQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.GetMedicalRecordsByPet(petId);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion
}
