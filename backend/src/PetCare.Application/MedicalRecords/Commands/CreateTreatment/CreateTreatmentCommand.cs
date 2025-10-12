using MediatR;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Commands.CreateTreatment;

public record CreateTreatmentCommand(
    Guid PetId,
    string TreatmentType,
    string Diagnosis,
    string? TreatmentDescription,
    DateTime TreatmentDate,
    DateTime? FollowUpDate,
    string? Medications,
    string? Instructions,
    string? Notes,
    string VetUserId
) : IRequest<TreatmentDto>;