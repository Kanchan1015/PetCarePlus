using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.MedicalRecords.Commands.CreateVaccination;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Application.MedicalRecords.Queries.GetVaccinationReport;
using System.Security.Claims;
using MediatR;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaccinationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VaccinationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new vaccination record (Vets and Admins only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "VET,ADMIN")]
    [ProducesResponseType(typeof(VaccinationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VaccinationDto>> CreateVaccination([FromBody] CreateVaccinationRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var command = new CreateVaccinationCommand(
            request.PetId,
            request.VaccineName,
            request.VaccinationDate,
            request.NextDueDate,
            request.BatchNumber,
            request.Manufacturer,
            request.Notes,
            currentUserId
        );

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetVaccinationReport), new { petId = result.PetId }, result);
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
    /// Get vaccination report for a specific pet
    /// </summary>
    [HttpGet("pet/{petId:guid}/report")]
    [ProducesResponseType(typeof(VaccinationReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VaccinationReportDto>> GetVaccinationReport(Guid petId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var isOwner = User.IsInRole("OWNER") && !User.IsInRole("ADMIN") && !User.IsInRole("VET");

        var query = new GetVaccinationReportQuery(petId, currentUserId, isOwner);

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

    public class CreateVaccinationRequest
    {
        public Guid PetId { get; set; }
        public string VaccineName { get; set; } = default!;
        public DateTime VaccinationDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string? BatchNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? Notes { get; set; }
    }
}