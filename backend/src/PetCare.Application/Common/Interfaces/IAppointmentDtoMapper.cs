// IAppointmentDtoMapper.cs
// Purpose: Mapper interface following Single Responsibility Principle  
// Separates DTO mapping concerns from controller logic

using System.Threading.Tasks;
using PetCare.Application.Appointments.DTOs;
using PetCare.Domain.Appointments;

namespace PetCare.Application.Common.Interfaces;

public interface IAppointmentDtoMapper
{
    Task<AppointmentDto> MapToDto(Appointment appointment);
}
