using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.Commands.CreatePrescription;

public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, PrescriptionDto>
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public CreatePrescriptionCommandHandler(
        IPrescriptionRepository prescriptionRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _prescriptionRepository = prescriptionRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<PrescriptionDto> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        // Validate pet exists
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.PetId} not found");
        }

        // Determine prescription status
        var status = PrescriptionStatus.Active;
        if (request.EndDate.HasValue && request.EndDate.Value < DateTime.Now)
        {
            status = PrescriptionStatus.Completed;
        }

        var prescription = new Prescription
        {
            PetId = request.PetId,
            VetUserId = request.VetUserId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            PrescribedDate = request.PrescribedDate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DurationDays = request.DurationDays,
            Instructions = request.Instructions,
            Status = status,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var createdPrescription = await _prescriptionRepository.AddAsync(prescription, cancellationToken);

        // Get vet full name
        var vetFullName = await _userService.GetUserFullNameAsync(request.VetUserId);

        return new PrescriptionDto(
            createdPrescription.Id,
            createdPrescription.PetId,
            createdPrescription.VetUserId,
            vetFullName,
            createdPrescription.MedicationName,
            createdPrescription.Dosage,
            createdPrescription.Frequency,
            createdPrescription.PrescribedDate,
            createdPrescription.StartDate,
            createdPrescription.EndDate,
            createdPrescription.DurationDays,
            createdPrescription.Instructions,
            createdPrescription.Status,
            createdPrescription.Notes,
            createdPrescription.CreatedAt,
            createdPrescription.UpdatedAt
        );
    }
}