using MediatR;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.MedicalRecords.DTOs;
using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.MedicalRecords.Queries.GetVaccinationReport;

public class GetVaccinationReportQueryHandler : IRequestHandler<GetVaccinationReportQuery, VaccinationReportDto>
{
    private readonly IVaccinationRepository _vaccinationRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserService _userService;

    public GetVaccinationReportQueryHandler(
        IVaccinationRepository vaccinationRepository,
        IPetRepository petRepository,
        IUserService userService)
    {
        _vaccinationRepository = vaccinationRepository;
        _petRepository = petRepository;
        _userService = userService;
    }

    public async Task<VaccinationReportDto> Handle(GetVaccinationReportQuery request, CancellationToken cancellationToken)
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
                throw new UnauthorizedAccessException("User does not have access to this pet's vaccination records");
            }
        }

        // Get all vaccinations
        var allVaccinations = await _vaccinationRepository.GetByPetIdAsync(request.PetId, cancellationToken);
        
        // Get overdue vaccinations
        var overdueVaccinations = await _vaccinationRepository.GetOverdueVaccinationsAsync(request.PetId, cancellationToken);
        
        // Get upcoming vaccinations (next 30 days)
        var upcomingVaccinations = await _vaccinationRepository.GetUpcomingVaccinationsAsync(request.PetId, DateTime.Now.AddDays(30), cancellationToken);

        // Convert to DTOs
        var allVaccinationDtos = await ConvertToVaccinationDtos(allVaccinations);
        var overdueVaccinationDtos = await ConvertToVaccinationDtos(overdueVaccinations);
        var upcomingVaccinationDtos = await ConvertToVaccinationDtos(upcomingVaccinations);

        // Determine if pet is up to date (no overdue vaccinations)
        var isUpToDate = !overdueVaccinations.Any();

        return new VaccinationReportDto(
            request.PetId,
            pet.Name,
            allVaccinationDtos,
            overdueVaccinationDtos,
            upcomingVaccinationDtos,
            isUpToDate
        );
    }

    private async Task<IReadOnlyList<VaccinationDto>> ConvertToVaccinationDtos(IReadOnlyList<Vaccination> vaccinations)
    {
        var dtos = new List<VaccinationDto>();
        foreach (var vaccination in vaccinations)
        {
            var vetFullName = await _userService.GetUserFullNameAsync(vaccination.VetUserId);
            dtos.Add(new VaccinationDto(
                vaccination.Id,
                vaccination.PetId,
                vaccination.VetUserId,
                vetFullName,
                vaccination.VaccineName,
                vaccination.VaccinationDate,
                vaccination.NextDueDate,
                vaccination.BatchNumber,
                vaccination.Manufacturer,
                vaccination.Status,
                vaccination.Notes,
                vaccination.CreatedAt,
                vaccination.UpdatedAt
            ));
        }
        return dtos.AsReadOnly();
    }
}