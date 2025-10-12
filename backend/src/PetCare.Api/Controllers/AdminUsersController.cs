using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.Admin.Users.CreateVet;
using PetCare.Infrastructure.Persistence;
using PetCare.Domain.Users; // AccountStatus
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using PetCare.Infrastructure.Auth;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "ADMIN")]
public class AdminUsersController : ControllerBase
{
    // ---------- CREATE VET ----------
    [HttpPost("vets")]
    [ProducesResponseType(typeof(CreateVetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVet(
        [FromBody] CreateVetRequest request,
        [FromServices] CreateVetCommand handler,
        CancellationToken ct)
    {
        var (ok, error, data) = await handler.ExecuteAsync(request, ct);

        if (!ok)
        {
            return error switch
            {
                "email_in_use" => Conflict(new ProblemDetails
                {
                    Title = "Email already in use",
                    Detail = "An account with this email already exists.",
                    Status = StatusCodes.Status409Conflict
                }),
                "validation_failed" => BadRequest(new ProblemDetails
                {
                    Title = "Validation failed",
                    Detail = "Missing or invalid fields.",
                    Status = StatusCodes.Status400BadRequest
                }),
                _ when error?.StartsWith("identity_error") == true
                    => BadRequest(new ProblemDetails { Title = "Identity error", Detail = error, Status = 400 }),
                _ when error?.StartsWith("role_error") == true
                    => BadRequest(new ProblemDetails { Title = "Role assignment failed", Detail = error, Status = 400 }),
                _ => BadRequest(new ProblemDetails { Title = "Create vet failed", Detail = error ?? "Unknown error", Status = 400 })
            };
        }

        return Created(string.Empty, data);
    }

    // ---------- LIST VETS (paging + search) ----------
    // GET /api/admin/users/vets?search=&page=1&pageSize=10
    [HttpGet("vets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListVets(
        [FromServices] PetCareDbContext db,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // input safety
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        // users with VET role (use NormalizedName for casing-robust match)
        var query =
            from u in db.Users
            join ur in db.UserRoles on u.Id equals ur.UserId
            join r in db.Roles on ur.RoleId equals r.Id
            where r.NormalizedName == "VET"
            select new VetListItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email!
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                (x.FullName ?? "").ToLower().Contains(s) ||
                (x.Email ?? "").ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new VetListResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    // GET /api/admin/users/vets/{id}
    [HttpGet("vets/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVetById(
        [FromServices] PetCareDbContext db,
        [FromRoute] string id,
        CancellationToken ct = default)
    {
        var vet =
            await (from u in db.Users
                   join ur in db.UserRoles on u.Id equals ur.UserId
                   join r in db.Roles on ur.RoleId equals r.Id
                   where r.NormalizedName == "VET" && u.Id == id
                   select new { u.Id, u.FullName, u.Email })
                  .FirstOrDefaultAsync(ct);

        if (vet is null) return NotFound();
        return Ok(new { id = vet.Id, fullName = vet.FullName, email = vet.Email });
    }

    // ---------- LIST USERS (all roles) ----------
    // GET /api/admin/users?role=Admin|Vet|Owner&search=...&page=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers(
        [FromServices] PetCareDbContext db,
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = pageSize is <= 0 or > 100 ? 10 : pageSize;
        var q = db.Users.AsQueryable();

        // role filter (Admin/Vet/Owner)
        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalized = role.Trim().ToUpperInvariant(); // "ADMIN"|"VET"|"OWNER"
            q =
                from u in q
                join ur in db.UserRoles on u.Id equals ur.UserId
                join r in db.Roles on ur.RoleId equals r.Id
                where r.NormalizedName == normalized
                select u;
        }

        // search by name or email (case-insensitive)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(u =>
                (u.FullName != null && EF.Functions.Like(u.FullName, $"%{s}%")) ||
                (u.Email != null && EF.Functions.Like(u.Email, $"%{s}%")));
        }

        var total = await q.CountAsync(ct);

        // page the base user slice
        var pageItems = await q
            .OrderBy(u => u.FullName).ThenBy(u => u.Email)
            .Select(u => new { u.Id, u.FullName, u.Email, u.AccountStatus })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = pageItems.Select(x => x.Id).ToList();

        // fetch roles for this slice
        var rolesMap = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync(ct);

        var rolesByUser = rolesMap
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name).OrderBy(n => n).ToArray());

        var items = pageItems.Select(x => new
        {
            id = x.Id,
            fullName = x.FullName,
            email = x.Email,
            accountStatus = x.AccountStatus.ToString(),
            isActive = x.AccountStatus == AccountStatus.Active,
            roles = rolesByUser.TryGetValue(x.Id, out var rr) ? rr : Array.Empty<string>()
        });

        return Ok(new
        {
            items,
            total,
            page,
            pageSize
        });
    }

    // ---------- UPDATE USER (profile + status only) ----------
    // PUT /api/admin/users/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] string id,
        [FromBody] UpdateUserRequest body,
        [FromServices] UserManager<ApplicationUser> users,
        CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();

        // FullName (optional)
        if (!string.IsNullOrWhiteSpace(body.FullName))
        {
            var fn = body.FullName.Trim();
            if (fn.Length < 2 || fn.Length > 100)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation failed",
                    Detail = "Full name must be between 2 and 100 characters.",
                    Status = 400
                });
            }
            user.FullName = fn;
        }

        // Email (optional) - conflict check
        if (!string.IsNullOrWhiteSpace(body.Email))
        {
            var newEmail = body.Email.Trim().ToLowerInvariant();
            if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await users.FindByEmailAsync(newEmail);
                if (exists is not null && exists.Id != user.Id)
                {
                    return Conflict(new ProblemDetails
                    {
                        Title = "Email already in use",
                        Detail = "Another account already uses this email.",
                        Status = 409
                    });
                }

                // Update via Identity helpers to keep normalized fields in sync
                var setEmailRes = await users.SetEmailAsync(user, newEmail);
                if (!setEmailRes.Succeeded)
                {
                    var msg = string.Join("; ", setEmailRes.Errors.Select(e => e.Description));
                    return BadRequest(new ProblemDetails { Title = "Email update failed", Detail = msg, Status = 400 });
                }

                // If you use Email as username, keep them in sync (optional)
                var setUserNameRes = await users.SetUserNameAsync(user, newEmail);
                if (!setUserNameRes.Succeeded)
                {
                    var msg = string.Join("; ", setUserNameRes.Errors.Select(e => e.Description));
                    return BadRequest(new ProblemDetails { Title = "Username update failed", Detail = msg, Status = 400 });
                }
            }
        }

        // AccountStatus (optional) - prevent self-deactivation
        if (!string.IsNullOrWhiteSpace(body.AccountStatus))
        {
            if (!Enum.TryParse<AccountStatus>(body.AccountStatus, true, out var parsed))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation failed",
                    Detail = "AccountStatus must be 'Active' or 'Inactive'.",
                    Status = 400
                });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub");

            if (currentUserId == user.Id && parsed != AccountStatus.Active)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Cannot deactivate yourself",
                    Detail = "An admin cannot set their own account to Inactive.",
                    Status = 400
                });
            }

            user.AccountStatus = parsed;
        }

        // Persist remaining fields
        var updateRes = await users.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            var msg = string.Join("; ", updateRes.Errors.Select(e => e.Description));
            return BadRequest(new ProblemDetails { Title = "Update failed", Detail = msg, Status = 400 });
        }

        // Roles for response
        var roles = (await users.GetRolesAsync(user)).OrderBy(r => r).ToArray();

        return Ok(new UpdateUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AccountStatus = user.AccountStatus.ToString(),
            IsActive = user.AccountStatus == AccountStatus.Active,
            Roles = roles
        });
    }

    // Local DTOs (keep it here for now; can move to Application later)
    private sealed class VetListItem
    {
        public string Id { get; set; } = default!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    private sealed class VetListResponse
    {
        public List<VetListItem> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public sealed class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AccountStatus { get; set; } // "Active" | "Inactive"
    }

    public sealed class UpdateUserResponse
    {
        public string Id { get; set; } = default!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string AccountStatus { get; set; } = default!;
        public bool IsActive { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
