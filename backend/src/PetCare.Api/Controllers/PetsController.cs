using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PetCare.Application.Pets.Commands.AssignPetToOwner;
using PetCare.Application.Pets.Commands.CreatePet;
using PetCare.Application.Pets.Commands.DeletePet;
using PetCare.Application.Pets.Commands.UpdatePet;
using PetCare.Application.Pets.DTOs;
using PetCare.Application.Pets.Queries.GetPetById;
using PetCare.Application.Pets.Queries.GetPets;
using PetCare.Application.Pets.Queries.GetPetsByOwner;
using Microsoft.AspNetCore.Identity;
using PetCare.Infrastructure.Auth;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public PetsController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    /// <summary>
    /// Get all pets (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<PetSummaryDto>>> GetAllPets()
    {
        var query = new GetPetsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get pet by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PetDto>> GetPetById(Guid id)
    {
        var query = new GetPetByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound($"Pet with ID {id} not found");
        }

        // Users can only access their own pets, Admins can access any pet
        if (!User.IsInRole("Admin") && result.OwnerUserId != (User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value))
        {
            return Forbid("You can only access your own pets");
        }

        return Ok(result);
    }

    /// <summary>
    /// Get pets by owner (Owners can get their own, Admins can get any)
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> GetPetsByOwner(string ownerId)
    {
        // Users can only access their own pets, Admins can access any owner's pets
        if (!User.IsInRole("Admin") && ownerId != (User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value))
        {
            return Forbid("You can only access your own pets");
        }

        var query = new GetPetsByOwnerQuery(ownerId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get users for pet assignment selection (Admin only)
    /// </summary>
    [HttpGet("users/selection")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<UserSelectionDto>>> GetUsersForSelection()
    {
        try
        {
            var users = _userManager.Users.ToList();
            var userSelection = new List<UserSelectionDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Only include users with Owner role
                if (roles.Contains("Owner"))
                {
                    userSelection.Add(new UserSelectionDto(
                        user.Id,
                        user.FullName ?? "Unknown",
                        user.Email ?? "No Email",
                        "Owner"
                    ));
                }
            }

            return Ok(userSelection.OrderBy(u => u.FullName).ToList());
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to load users: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new pet (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PetDto>> CreatePet([FromBody] CreatePetDto createPetDto)
    {
        var command = new CreatePetCommand(
            createPetDto.Name,
            createPetDto.Species,
            createPetDto.Breed,
            createPetDto.DateOfBirth,
            createPetDto.Color,
            createPetDto.Weight,
            createPetDto.MedicalNotes,
            createPetDto.OwnerUserId
        );

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing pet (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PetDto>> UpdatePet(Guid id, [FromBody] UpdatePetDto updatePetDto)
    {
        var command = new UpdatePetCommand(
            id,
            updatePetDto.Name,
            updatePetDto.Species,
            updatePetDto.Breed,
            updatePetDto.DateOfBirth,
            updatePetDto.Color,
            updatePetDto.Weight,
            updatePetDto.MedicalNotes,
            updatePetDto.IsActive
        );

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a pet (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletePet(Guid id)
    {
        var command = new DeletePetCommand(id);

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Assign pet to a new owner (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PetDto>> AssignPetToOwner(Guid id, [FromBody] AssignPetDto assignPetDto)
    {
        if (id != assignPetDto.PetId)
        {
            return BadRequest("Pet ID in URL does not match Pet ID in request body");
        }

        var command = new AssignPetToOwnerCommand(
            assignPetDto.PetId,
            assignPetDto.NewOwnerUserId
        );

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get current user's pets
    /// </summary>
    [HttpGet("my-pets")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> GetMyPets()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var query = new GetPetsByOwnerQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Debug endpoint to check current user claims
    /// </summary>
    [HttpGet("debug/me")]
    public ActionResult GetCurrentUserInfo()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? User.FindFirst("sub")?.Value;
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        return Ok(new
        {
            UserId = userId,
            Roles = roles,
            AllClaims = claims,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            AuthenticationType = User.Identity?.AuthenticationType
        });
    }
}