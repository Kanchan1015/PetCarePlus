using MediatR;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Queries.GetTreatmentHistoryReport;

public record GetTreatmentHistoryReportQuery(
    Guid PetId,
    string UserId,
    bool IsOwner
) : IRequest<TreatmentHistoryReportDto>;