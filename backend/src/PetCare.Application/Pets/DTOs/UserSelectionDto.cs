namespace PetCare.Application.Pets.DTOs;

public record UserSelectionDto(
    string Id,
    string FullName,
    string Email,
    string Role
);