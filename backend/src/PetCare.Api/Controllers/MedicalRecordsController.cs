using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.MedicalRecords.Commands.CreateMedicalRecord;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Application.MedicalRecords.Queries.GetMedicalRecordsByPet;
using PetCare.Domain.MedicalRecords;
using System.Security.Claims;
using MediatR;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicalRecordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new medical record (Vets and Admins only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "VET,ADMIN")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MedicalRecordDto>> CreateMedicalRecord([FromBody] CreateMedicalRecordRequest request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var command = new CreateMedicalRecordCommand(
            request.PetId,
            request.RecordType,
            request.RecordDate,
            request.Title,
            request.Description,
            request.Notes,
            currentUserId
        );

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetMedicalRecordsByPet), new { petId = result.PetId }, result);
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
    /// Get medical records for a specific pet
    /// </summary>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<MedicalRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MedicalRecordDto>>> GetMedicalRecordsByPet(Guid petId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var isOwner = User.IsInRole("OWNER") && !User.IsInRole("ADMIN") && !User.IsInRole("VET");

        var query = new GetMedicalRecordsByPetQuery(petId, currentUserId, isOwner);

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

    public class CreateMedicalRecordRequest
    {
        public Guid PetId { get; set; }
        public MedicalRecordType RecordType { get; set; }
        public DateTime RecordDate { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}