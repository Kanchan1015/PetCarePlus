using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetCare.Api.Controllers;
using PetCare.Application.MedicalRecords.Commands.CreateVaccination;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Application.MedicalRecords.Queries.GetVaccinationReport;
using PetCare.Domain.MedicalRecords;
using System.Security.Claims;
using Xunit;
using static PetCare.Api.Controllers.VaccinationsController;

namespace PetCare.Api.Tests.Controllers;

public class VaccinationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VaccinationsController _controller;

    public VaccinationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new VaccinationsController(_mediatorMock.Object);
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

    #region CreateVaccination Tests

    [Fact]
    public async Task CreateVaccination_AsVet_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var vetId = "vet-123";
        SetupUserClaims(vetId, "Vet");

        var request = new CreateVaccinationRequest
        {
            PetId = Guid.NewGuid(),
            VaccineName = "Rabies",
            VaccinationDate = DateTime.Today,
            NextDueDate = DateTime.Today.AddYears(1),
            BatchNumber = "RB001",
            Manufacturer = "VetPharm Inc",
            Notes = "No adverse reactions observed"
        };

        var expectedResult = new VaccinationDto(
            Guid.NewGuid(),
            request.PetId,
            vetId,
            "Dr. Smith",
            request.VaccineName,
            request.VaccinationDate,
            request.NextDueDate,
            request.BatchNumber,
            request.Manufacturer,
            VaccinationStatus.Current,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateVaccinationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateVaccination(request);

        // Assert
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(VaccinationsController.GetVaccinationReport));
        createdAtActionResult.RouteValues.Should().ContainKey("petId").WhoseValue.Should().Be(request.PetId);
        
        var returnedVaccination = createdAtActionResult.Value.Should().BeOfType<VaccinationDto>().Subject;
        returnedVaccination.Should().BeEquivalentTo(expectedResult);

        _mediatorMock.Verify(x => x.Send(It.Is<CreateVaccinationCommand>(cmd => 
            cmd.PetId == request.PetId &&
            cmd.VaccineName == request.VaccineName &&
            cmd.VaccinationDate == request.VaccinationDate &&
            cmd.VetUserId == vetId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVaccination_AsAdmin_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "Admin");

        var request = new CreateVaccinationRequest
        {
            PetId = Guid.NewGuid(),
            VaccineName = "DHPP",
            VaccinationDate = DateTime.Today,
            NextDueDate = DateTime.Today.AddYears(1),
            BatchNumber = "DHPP002",
            Manufacturer = "AnimalHealth Corp",
            Notes = "Annual booster"
        };

        var expectedResult = new VaccinationDto(
            Guid.NewGuid(),
            request.PetId,
            adminId,
            "Admin User",
            request.VaccineName,
            request.VaccinationDate,
            request.NextDueDate,
            request.BatchNumber,
            request.Manufacturer,
            VaccinationStatus.Current,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateVaccinationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateVaccination(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateVaccination_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateVaccinationRequest
        {
            PetId = Guid.NewGuid(),
            VaccineName = "Rabies",
            VaccinationDate = DateTime.Today
        };

        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.CreateVaccination(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateVaccination_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreateVaccinationRequest
        {
            PetId = Guid.NewGuid(),
            VaccineName = "InvalidVaccine",
            VaccinationDate = DateTime.Today
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateVaccinationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid vaccination data"));

        // Act
        var result = await _controller.CreateVaccination(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Invalid vaccination data" });
    }

    [Fact]
    public async Task CreateVaccination_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreateVaccinationRequest
        {
            PetId = Guid.NewGuid(),
            VaccineName = "Rabies",
            VaccinationDate = DateTime.Today
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateVaccinationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.CreateVaccination(request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region GetVaccinationReport Tests

    [Fact]
    public async Task GetVaccinationReport_AsVet_ShouldReturnOkWithReport()
    {
        // Arrange
        var vetId = "vet-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(vetId, "Vet");

        var expectedReport = new VaccinationReportDto(
            petId,
            "Buddy",
            new List<VaccinationDto>
            {
                new(Guid.NewGuid(), petId, vetId, "Dr. Smith", "Rabies", 
                    DateTime.Today.AddYears(-1), DateTime.Today.AddMonths(1), 
                    "RB001", "VetPharm Inc", VaccinationStatus.Upcoming, 
                    "Annual rabies", DateTime.UtcNow.AddYears(-1), null),
                new(Guid.NewGuid(), petId, vetId, "Dr. Smith", "DHPP", 
                    DateTime.Today.AddMonths(-6), DateTime.Today.AddMonths(6), 
                    "DHPP002", "AnimalHealth Corp", VaccinationStatus.Current, 
                    "6-month booster", DateTime.UtcNow.AddMonths(-6), null)
            },
            new List<VaccinationDto>
            {
                new(Guid.NewGuid(), petId, vetId, "Dr. Smith", "Rabies", 
                    DateTime.Today.AddYears(-1), DateTime.Today.AddMonths(1), 
                    "RB001", "VetPharm Inc", VaccinationStatus.Upcoming, 
                    "Annual rabies", DateTime.UtcNow.AddYears(-1), null)
            },
            new List<VaccinationDto>(),
            true
        );

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetVaccinationReportQuery>(q => 
                q.PetId == petId && 
                q.UserId == vetId && 
                q.IsOwner == false
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.GetVaccinationReport(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedReport = okResult.Value.Should().BeOfType<VaccinationReportDto>().Subject;
        returnedReport.Should().BeEquivalentTo(expectedReport);
    }

    [Fact]
    public async Task GetVaccinationReport_AsOwner_ShouldReturnOkWithReport()
    {
        // Arrange
        var ownerId = "owner-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(ownerId, "Owner");

        var expectedReport = new VaccinationReportDto(
            petId,
            "Buddy",
            new List<VaccinationDto>(),
            new List<VaccinationDto>(),
            new List<VaccinationDto>(),
            true
        );

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetVaccinationReportQuery>(q => 
                q.PetId == petId && 
                q.UserId == ownerId && 
                q.IsOwner == true
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.GetVaccinationReport(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedReport = okResult.Value.Should().BeOfType<VaccinationReportDto>().Subject;
        returnedReport.Should().BeEquivalentTo(expectedReport);
    }

    [Fact]
    public async Task GetVaccinationReport_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var petId = Guid.NewGuid();
        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.GetVaccinationReport(petId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetVaccinationReport_WhenArgumentException_ShouldReturnNotFound()
    {
        // Arrange
        var petId = Guid.NewGuid();
        SetupUserClaims("vet-123", "Vet");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetVaccinationReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Pet not found"));

        // Act
        var result = await _controller.GetVaccinationReport(petId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pet not found" });
    }

    [Fact]
    public async Task GetVaccinationReport_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        var petId = Guid.NewGuid();
        SetupUserClaims("owner-123", "Owner");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetVaccinationReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.GetVaccinationReport(petId);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion
}
