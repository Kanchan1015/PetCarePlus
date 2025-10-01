using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.MedicalRecords.Commands.CreateTreatment;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Application.MedicalRecords.Queries.GetTreatmentHistoryReport;
using System.Security.Claims;
using MediatR;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TreatmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TreatmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new treatment record (Vets and Admins only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "VET,ADMIN")]
    [ProducesResponseType(typeof(TreatmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TreatmentDto>> CreateTreatment([FromBody] CreateTreatmentRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var command = new CreateTreatmentCommand(
            request.PetId,
            request.TreatmentType,
            request.Diagnosis,
            request.TreatmentDescription,
            request.TreatmentDate,
            request.FollowUpDate,
            request.Medications,
            request.Instructions,
            request.Notes,
            currentUserId
        );

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTreatmentHistoryReport), new { petId = result.PetId }, result);
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
    /// Get treatment history report for a specific pet
    /// </summary>
    [HttpGet("pet/{petId:guid}/history")]
    [ProducesResponseType(typeof(TreatmentHistoryReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TreatmentHistoryReportDto>> GetTreatmentHistoryReport(Guid petId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var isOwner = User.IsInRole("OWNER") && !User.IsInRole("ADMIN") && !User.IsInRole("VET");

        var query = new GetTreatmentHistoryReportQuery(petId, currentUserId, isOwner);

        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    public class CreateTreatmentRequest
    {
        public Guid PetId { get; set; }
        public string TreatmentType { get; set; } = default!;
        public string Diagnosis { get; set; } = default!;
        public string? TreatmentDescription { get; set; }
        public DateTime TreatmentDate { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string? Medications { get; set; }
        public string? Instructions { get; set; }
        public string? Notes { get; set; }
    }
}