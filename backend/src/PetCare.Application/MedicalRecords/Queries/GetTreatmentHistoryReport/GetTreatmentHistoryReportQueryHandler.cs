using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Queries.GetTreatmentHistoryReport;

public class GetTreatmentHistoryReportQueryHandler : IRequestHandler<GetTreatmentHistoryReportQuery, TreatmentHistoryReportDto>
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public GetTreatmentHistoryReportQueryHandler(
        ITreatmentRepository treatmentRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _treatmentRepository = treatmentRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<TreatmentHistoryReportDto> Handle(GetTreatmentHistoryReportQuery request, CancellationToken cancellationToken)
    {
        // Validate pet exists and user has access
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.PetId} not found");
        }

        // Check if user is the owner or has access (vet/admin)
        if (request.IsOwner)
        {
            var isOwner = await _petRepository.IsOwnerAsync(request.PetId, request.UserId, cancellationToken);
            if (!isOwner)
            {
                throw new UnauthorizedAccessException("User does not have access to this pet's treatment history");
            }
        }

        // Get treatment history
        var treatments = await _treatmentRepository.GetTreatmentHistoryAsync(request.PetId, cancellationToken);

        // Convert to DTOs
        var treatmentDtos = new List<TreatmentDto>();
        foreach (var treatment in treatments)
        {
            var vetFullName = await _userService.GetUserFullNameAsync(treatment.VetUserId);
            treatmentDtos.Add(new TreatmentDto(
                treatment.Id,
                treatment.PetId,
                treatment.VetUserId,
                vetFullName,
                treatment.TreatmentType,
                treatment.Diagnosis,
                treatment.Treatment_Description,
                treatment.TreatmentDate,
                treatment.FollowUpDate,
                treatment.Status,
                treatment.Medications,
                treatment.Instructions,
                treatment.Notes,
                treatment.CreatedAt,
                treatment.UpdatedAt
            ));
        }

        // Calculate summary statistics
        var lastTreatmentDate = treatments.Any() ? treatments.Max(t => t.TreatmentDate) : (DateTime?)null;
        var totalTreatments = treatments.Count;

        return new TreatmentHistoryReportDto(
            request.PetId,
            pet.Name,
            treatmentDtos.AsReadOnly(),
            lastTreatmentDate,
            totalTreatments
        );
    }
}