using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PetCare.Api.Controllers;
using PetCare.Application.MedicalRecords.Commands.CreateTreatment;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Application.MedicalRecords.Queries.GetTreatmentHistoryReport;
using PetCare.Domain.MedicalRecords;
using System.Security.Claims;
using Xunit;
using static PetCare.Api.Controllers.TreatmentsController;

namespace PetCare.Api.Tests.Controllers;

public class TreatmentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TreatmentsController _controller;

    public TreatmentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TreatmentsController(_mediatorMock.Object);
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

    #region CreateTreatment Tests

    [Fact]
    public async Task CreateTreatment_AsVet_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var vetId = "vet-123";
        SetupUserClaims(vetId, "Vet");

        var request = new CreateTreatmentRequest
        {
            PetId = Guid.NewGuid(),
            TreatmentType = "Dental Cleaning",
            Diagnosis = "Dental tartar buildup",
            TreatmentDescription = "Professional dental cleaning and polishing",
            TreatmentDate = DateTime.Today,
            FollowUpDate = DateTime.Today.AddDays(14),
            Medications = "Antibiotic gel",
            Instructions = "Soft food for 24 hours",
            Notes = "Patient tolerated procedure well"
        };

        var expectedResult = new TreatmentDto(
            Guid.NewGuid(),
            request.PetId,
            vetId,
            "Dr. Smith",
            request.TreatmentType,
            request.Diagnosis,
            request.TreatmentDescription,
            request.TreatmentDate,
            request.FollowUpDate,
            TreatmentStatus.Completed,
            request.Medications,
            request.Instructions,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateTreatmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateTreatment(request);

        // Assert
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(TreatmentsController.GetTreatmentHistoryReport));
        createdAtActionResult.RouteValues.Should().ContainKey("petId").WhoseValue.Should().Be(request.PetId);
        
        var returnedTreatment = createdAtActionResult.Value.Should().BeOfType<TreatmentDto>().Subject;
        returnedTreatment.Should().BeEquivalentTo(expectedResult);

        _mediatorMock.Verify(x => x.Send(It.Is<CreateTreatmentCommand>(cmd => 
            cmd.PetId == request.PetId &&
            cmd.TreatmentType == request.TreatmentType &&
            cmd.Diagnosis == request.Diagnosis &&
            cmd.TreatmentDate == request.TreatmentDate &&
            cmd.VetUserId == vetId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTreatment_AsAdmin_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var adminId = "admin-123";
        SetupUserClaims(adminId, "Admin");

        var request = new CreateTreatmentRequest
        {
            PetId = Guid.NewGuid(),
            TreatmentType = "Surgery",
            Diagnosis = "Foreign object ingestion",
            TreatmentDescription = "Surgical removal of foreign object",
            TreatmentDate = DateTime.Today,
            FollowUpDate = DateTime.Today.AddDays(3),
            Medications = "Pain medication, antibiotics",
            Instructions = "Keep incision clean and dry",
            Notes = "Surgery successful, object removed"
        };

        var expectedResult = new TreatmentDto(
            Guid.NewGuid(),
            request.PetId,
            adminId,
            "Admin User",
            request.TreatmentType,
            request.Diagnosis,
            request.TreatmentDescription,
            request.TreatmentDate,
            request.FollowUpDate,
            TreatmentStatus.Completed,
            request.Medications,
            request.Instructions,
            request.Notes,
            DateTime.UtcNow,
            null
        );

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateTreatmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateTreatment(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateTreatment_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateTreatmentRequest
        {
            PetId = Guid.NewGuid(),
            TreatmentType = "Examination",
            Diagnosis = "Routine checkup",
            TreatmentDate = DateTime.Today
        };

        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.CreateTreatment(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateTreatment_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreateTreatmentRequest
        {
            PetId = Guid.NewGuid(),
            TreatmentType = "Invalid Treatment",
            Diagnosis = "Test diagnosis",
            TreatmentDate = DateTime.Today
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateTreatmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid treatment data"));

        // Act
        var result = await _controller.CreateTreatment(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Invalid treatment data" });
    }

    [Fact]
    public async Task CreateTreatment_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        SetupUserClaims("vet-123", "Vet");

        var request = new CreateTreatmentRequest
        {
            PetId = Guid.NewGuid(),
            TreatmentType = "Examination",
            Diagnosis = "Routine checkup",
            TreatmentDate = DateTime.Today
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateTreatmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.CreateTreatment(request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region GetTreatmentHistoryReport Tests

    [Fact]
    public async Task GetTreatmentHistoryReport_AsVet_ShouldReturnOkWithReport()
    {
        // Arrange
        var vetId = "vet-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(vetId, "Vet");

        var expectedReport = new TreatmentHistoryReportDto(
            petId,
            "Buddy",
            new List<TreatmentDto>
            {
                new(Guid.NewGuid(), petId, vetId, "Dr. Smith", "Dental Cleaning", 
                    "Tartar buildup", "Professional cleaning", DateTime.Today.AddMonths(-1), 
                    null, TreatmentStatus.Completed, "None", "Regular dental care", 
                    "Successful procedure", DateTime.UtcNow.AddMonths(-1), null),
                new(Guid.NewGuid(), petId, vetId, "Dr. Smith", "Vaccination", 
                    "Routine immunization", "Annual vaccines", DateTime.Today.AddMonths(-2), 
                    DateTime.Today.AddMonths(10), TreatmentStatus.Completed, "None", 
                    "Monitor for reactions", "No adverse effects", DateTime.UtcNow.AddMonths(-2), null)
            },
            DateTime.Today.AddMonths(-1),
            2
        );

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetTreatmentHistoryReportQuery>(q => 
                q.PetId == petId && 
                q.UserId == vetId && 
                q.IsOwner == false
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.GetTreatmentHistoryReport(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedReport = okResult.Value.Should().BeOfType<TreatmentHistoryReportDto>().Subject;
        returnedReport.Should().BeEquivalentTo(expectedReport);
    }

    [Fact]
    public async Task GetTreatmentHistoryReport_AsOwner_ShouldReturnOkWithReport()
    {
        // Arrange
        var ownerId = "owner-123";
        var petId = Guid.NewGuid();
        SetupUserClaims(ownerId, "Owner");

        var expectedReport = new TreatmentHistoryReportDto(
            petId,
            "Buddy",
            new List<TreatmentDto>(),
            null,
            0
        );

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetTreatmentHistoryReportQuery>(q => 
                q.PetId == petId && 
                q.UserId == ownerId && 
                q.IsOwner == true
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.GetTreatmentHistoryReport(petId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedReport = okResult.Value.Should().BeOfType<TreatmentHistoryReportDto>().Subject;
        returnedReport.Should().BeEquivalentTo(expectedReport);
    }

    [Fact]
    public async Task GetTreatmentHistoryReport_WithoutUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var petId = Guid.NewGuid();
        // No claims setup - user has no ID
        SetupEmptyUser();

        // Act
        var result = await _controller.GetTreatmentHistoryReport(petId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTreatmentHistoryReport_WhenArgumentException_ShouldReturnNotFound()
    {
        // Arrange
        var petId = Guid.NewGuid();
        SetupUserClaims("vet-123", "Vet");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTreatmentHistoryReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Pet not found"));

        // Act
        var result = await _controller.GetTreatmentHistoryReport(petId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "Pet not found" });
    }

    [Fact]
    public async Task GetTreatmentHistoryReport_WhenUnauthorizedAccessException_ShouldReturnForbid()
    {
        // Arrange
        var petId = Guid.NewGuid();
        SetupUserClaims("owner-123", "Owner");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTreatmentHistoryReportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.GetTreatmentHistoryReport(petId);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion
}
