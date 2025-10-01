using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.Commands.CreateMedicalRecord;

public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, MedicalRecordDto>
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public CreateMedicalRecordCommandHandler(
        IMedicalRecordRepository medicalRecordRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<MedicalRecordDto> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        // Validate pet exists
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.PetId} not found");
        }

        // Create medical record
        var medicalRecord = new MedicalRecord
        {
            PetId = request.PetId,
            VetUserId = request.VetUserId,
            RecordType = request.RecordType,
            RecordDate = request.RecordDate,
            Title = request.Title,
            Description = request.Description,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var createdRecord = await _medicalRecordRepository.AddAsync(medicalRecord, cancellationToken);

        // Get vet full name
        var vetFullName = await _userService.GetUserFullNameAsync(request.VetUserId);

        return new MedicalRecordDto(
            createdRecord.Id,
            createdRecord.PetId,
            createdRecord.VetUserId,
            vetFullName,
            createdRecord.RecordType,
            createdRecord.RecordDate,
            createdRecord.Title,
            createdRecord.Description,
            createdRecord.Notes,
            createdRecord.CreatedAt,
            createdRecord.UpdatedAt
        );
    }
}