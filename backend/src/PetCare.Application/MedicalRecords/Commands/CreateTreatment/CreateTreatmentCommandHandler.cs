using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.Commands.CreateTreatment;

public class CreateTreatmentCommandHandler : IRequestHandler<CreateTreatmentCommand, TreatmentDto>
{
    private readonly ITreatmentRepository _treatmentRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public CreateTreatmentCommandHandler(
        ITreatmentRepository treatmentRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _treatmentRepository = treatmentRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<TreatmentDto> Handle(CreateTreatmentCommand request, CancellationToken cancellationToken)
    {
        // Validate pet exists
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.PetId} not found");
        }

        var treatment = new Treatment
        {
            PetId = request.PetId,
            VetUserId = request.VetUserId,
            TreatmentType = request.TreatmentType,
            Diagnosis = request.Diagnosis,
            Treatment_Description = request.TreatmentDescription,
            TreatmentDate = request.TreatmentDate,
            FollowUpDate = request.FollowUpDate,
            Status = TreatmentStatus.Completed, // Default to completed unless specified
            Medications = request.Medications,
            Instructions = request.Instructions,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var createdTreatment = await _treatmentRepository.AddAsync(treatment, cancellationToken);

        // Get vet full name
        var vetFullName = await _userService.GetUserFullNameAsync(request.VetUserId);

        return new TreatmentDto(
            createdTreatment.Id,
            createdTreatment.PetId,
            createdTreatment.VetUserId,
            vetFullName,
            createdTreatment.TreatmentType,
            createdTreatment.Diagnosis,
            createdTreatment.Treatment_Description,
            createdTreatment.TreatmentDate,
            createdTreatment.FollowUpDate,
            createdTreatment.Status,
            createdTreatment.Medications,
            createdTreatment.Instructions,
            createdTreatment.Notes,
            createdTreatment.CreatedAt,
            createdTreatment.UpdatedAt
        );
    }
}