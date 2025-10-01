using MediatR;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Commands.CreatePrescription;

public record CreatePrescriptionCommand(
    Guid PetId,
    string MedicationName,
    string Dosage,
    string Frequency,
    DateTime PrescribedDate,
    DateTime? StartDate,
    DateTime? EndDate,
    int? DurationDays,
    string? Instructions,
    string? Notes,
    string VetUserId
) : IRequest<PrescriptionDto>;