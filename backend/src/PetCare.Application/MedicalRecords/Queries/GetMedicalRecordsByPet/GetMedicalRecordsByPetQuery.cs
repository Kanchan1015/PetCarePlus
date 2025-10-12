using MediatR;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Queries.GetMedicalRecordsByPet;

public record GetMedicalRecordsByPetQuery(
    Guid PetId,
    string UserId,
    bool IsOwner
) : IRequest<IReadOnlyList<MedicalRecordDto>>;