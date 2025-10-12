using MediatR;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.Commands.CreateMedicalRecord;

public record CreateMedicalRecordCommand(
    Guid PetId,
    MedicalRecordType RecordType,
    DateTime RecordDate,
    string Title,
    string? Description,
    string? Notes,
    string VetUserId
) : IRequest<MedicalRecordDto>;