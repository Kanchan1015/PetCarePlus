using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.Commands.CreateVaccination;

public class CreateVaccinationCommandHandler : IRequestHandler<CreateVaccinationCommand, VaccinationDto>
{
    private readonly IVaccinationRepository _vaccinationRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public CreateVaccinationCommandHandler(
        IVaccinationRepository vaccinationRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _vaccinationRepository = vaccinationRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<VaccinationDto> Handle(CreateVaccinationCommand request, CancellationToken cancellationToken)
    {
        // Validate pet exists
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet == null)
        {
            throw new ArgumentException($"Pet with ID {request.PetId} not found");
        }

        // Determine vaccination status based on dates
        var status = VaccinationStatus.Current;
        if (request.NextDueDate.HasValue)
        {
            if (request.NextDueDate.Value < DateTime.Now)
            {
                status = VaccinationStatus.Overdue;
            }
            else if (request.NextDueDate.Value <= DateTime.Now.AddDays(30))
            {
                status = VaccinationStatus.Upcoming;
            }
        }

        var vaccination = new Vaccination
        {
            PetId = request.PetId,
            VetUserId = request.VetUserId,
            VaccineName = request.VaccineName,
            VaccinationDate = request.VaccinationDate,
            NextDueDate = request.NextDueDate,
            BatchNumber = request.BatchNumber,
            Manufacturer = request.Manufacturer,
            Status = status,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var createdVaccination = await _vaccinationRepository.AddAsync(vaccination, cancellationToken);

        // Get vet full name
        var vetFullName = await _userService.GetUserFullNameAsync(request.VetUserId);

        return new VaccinationDto(
            createdVaccination.Id,
            createdVaccination.PetId,
            createdVaccination.VetUserId,
            vetFullName,
            createdVaccination.VaccineName,
            createdVaccination.VaccinationDate,
            createdVaccination.NextDueDate,
            createdVaccination.BatchNumber,
            createdVaccination.Manufacturer,
            createdVaccination.Status,
            createdVaccination.Notes,
            createdVaccination.CreatedAt,
            createdVaccination.UpdatedAt
        );
    }
}