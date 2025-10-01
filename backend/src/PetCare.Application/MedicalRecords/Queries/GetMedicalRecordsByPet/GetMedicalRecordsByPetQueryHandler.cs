using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;

namespace PetCare.Application.MedicalRecords.Queries.GetMedicalRecordsByPet;

public class GetMedicalRecordsByPetQueryHandler : IRequestHandler<GetMedicalRecordsByPetQuery, IReadOnlyList<MedicalRecordDto>>
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public GetMedicalRecordsByPetQueryHandler(
        IMedicalRecordRepository medicalRecordRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<IReadOnlyList<MedicalRecordDto>> Handle(GetMedicalRecordsByPetQuery request, CancellationToken cancellationToken)
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
                throw new UnauthorizedAccessException("User does not have access to this pet's medical records");
            }
        }

        var medicalRecords = await _medicalRecordRepository.GetByPetIdAsync(request.PetId, cancellationToken);

        var dtos = new List<MedicalRecordDto>();
        foreach (var record in medicalRecords)
        {
            var vetFullName = await _userService.GetUserFullNameAsync(record.VetUserId);
            dtos.Add(new MedicalRecordDto(
                record.Id,
                record.PetId,
                record.VetUserId,
                vetFullName,
                record.RecordType,
                record.RecordDate,
                record.Title,
                record.Description,
                record.Notes,
                record.CreatedAt,
                record.UpdatedAt
            ));
        }

        return dtos.AsReadOnly();
    }
}