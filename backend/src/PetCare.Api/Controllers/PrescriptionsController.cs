using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.MedicalRecords.Commands.CreatePrescription;
using PetCare.Application.MedicalRecords.DTOs;
using System.Security.Claims;
using MediatR;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrescriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new prescription (Vets and Admins only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "VET,ADMIN")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PrescriptionDto>> CreatePrescription([FromBody] CreatePrescriptionRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var command = new CreatePrescriptionCommand(
            request.PetId,
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.PrescribedDate,
            request.StartDate,
            request.EndDate,
            request.DurationDays,
            request.Instructions,
            request.Notes,
            currentUserId
        );

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPrescription), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get prescription by ID (placeholder for future implementation)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PrescriptionDto> GetPrescription(Guid id)
    {
        // This would be implemented later with a GetPrescriptionByIdQuery
        return NotFound("Prescription retrieval not yet implemented");
    }

    public class CreatePrescriptionRequest
    {
        public Guid PetId { get; set; }
        public string MedicationName { get; set; } = default!;
        public string Dosage { get; set; } = default!;
        public string Frequency { get; set; } = default!;
        public DateTime PrescribedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DurationDays { get; set; }
        public string? Instructions { get; set; }
        public string? Notes { get; set; }
    }
}