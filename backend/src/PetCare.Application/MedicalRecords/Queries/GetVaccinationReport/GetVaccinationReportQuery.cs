using MediatR;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Queries.GetVaccinationReport;

public record GetVaccinationReportQuery(
    Guid PetId,
    string UserId,
    bool IsOwner
) : IRequest<VaccinationReportDto>;