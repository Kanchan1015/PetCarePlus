using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetCare.Api.Controllers;
using PetCare.Application.MedicalRecords.Commands.CreatePrescription;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;
using System.Security.Claims;
using Xunit;
using static PetCare.Api.Controllers.PrescriptionsController;

namespace PetCare.Api.Tests.Controllers;

public class PrescriptionsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PrescriptionsController _controller;

    public PrescriptionsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PrescriptionsController(_mediatorMock.Object);
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

    #region CreatePrescription Tests

    [Fact]
    public async Task CreatePrescription_AsVet_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var vetId = "vet-123";
        SetupUserClaims(vetId, "Vet");

        var request = new CreatePrescriptionRequest
        {
            PetId = Guid.NewGuid(),
            MedicationName = "Amoxicillin",
            Dosage = "250mg",
            Frequency = "Twice daily",
            PrescribedDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(7),
            DurationDays = 7,
            Instructions = "Give with food to reduce stomach upset",
            Notes = "Monitor for allergic reactions"
        };

        var expectedResult = new PrescriptionDto(
            Guid.NewGuid(),
            request.PetId,
            vetId,
            "Dr. Smith",
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.PrescribedDate,
            request.StartDate,
            request.EndDate,
            request.DurationDays,
            request.Instructions,
            PrescriptionStatus.Active,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePrescriptionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreatePrescription(request);

        // Assert
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(PrescriptionsController.GetPrescription));
        createdAtActionResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(expectedResult.Id);
        
        var returnedPrescription = createdAtActionResult.Value.Should().BeOfType<PrescriptionDto>().Subject;
        returnedPrescription.Should().BeEquivalentTo(expectedResult);

        _mediatorMock.Verify(x => x.Send(It.Is<CreatePrescriptionCommand>(cmd => 
            cmd.PetId == request.PetId &&
            cmd.MedicationName == request.MedicationName &&
            cmd.Dosage == request.Dosage &&
            cmd.Frequency == request.Frequency &&
            cmd.PrescribedDate == request.PrescribedDate &&
            cmd.VetUserId == vetId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePrescription_AsAdmin_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "Admin");

        var request = new CreatePrescriptionRequest
        {
            PetId = Guid.NewGuid(),
            MedicationName = "Prednisone",
            Dosage = "5mg",
            Frequency = "Once daily",
            PrescribedDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            DurationDays = 5,
            Instructions = "Give in the morning with breakfast",
            Notes = "Short-term anti-inflammatory course"
        };

        var expectedResult = new PrescriptionDto(
            Guid.NewGuid(),
            request.PetId,
            adminId,
            "Admin User",
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.PrescribedDate,
            request.StartDate,
            request.EndDate,
            request.DurationDays,
            request.Instructions,
            PrescriptionStatus.Active,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePrescriptionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreatePrescription(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreatePrescription_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreatePrescriptionRequest
        {
            PetId = Guid.NewGuid(),
            MedicationName = "Test Medication",
            Dosage = "10mg",
            Frequency = "Once daily",
            PrescribedDate = DateTime.Today,
            DurationDays = 7
        };

        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.CreatePrescription(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreatePrescription_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreatePrescriptionRequest
        {
            PetId = Guid.NewGuid(),
            MedicationName = "Invalid Medication",
            Dosage = "Invalid dosage",
            Frequency = "Invalid frequency",
            PrescribedDate = DateTime.Today,
            DurationDays = -1 // Invalid duration
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePrescriptionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid prescription data"));

        // Act
        var result = await _controller.CreatePrescription(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Invalid prescription data" });
    }

    [Fact]
    public async Task CreatePrescription_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreatePrescriptionRequest
        {
            PetId = Guid.NewGuid(),
            MedicationName = "Test Medication",
            Dosage = "10mg",
            Frequency = "Once daily",
            PrescribedDate = DateTime.Today,
            DurationDays = 7
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreatePrescriptionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.CreatePrescription(request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region GetPrescription Tests (Basic placeholder test for future implementation)

    [Fact]
    public void GetPrescription_ShouldReturnNotFound()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();

        // Act
        var result = _controller.GetPrescription(prescriptionId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}