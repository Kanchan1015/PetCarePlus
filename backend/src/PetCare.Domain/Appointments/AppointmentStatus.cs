// AppointmentStatus.cs
// Purpose: Enum for appointment status tracking throughout the workflow
// States: Pending (initial) -> Approved/Cancelled (admin decision) -> Completed/NoShow (final)

namespace PetCare.Domain.Appointments;

public enum AppointmentStatus
{
    Pending = 0,      // Initial state when owner submits request
    Approved = 1,     // Admin approved the request
    Cancelled = 2,    // Admin or owner cancelled
    Completed = 3,    // Appointment was completed
    NoShow = 4        // Pet owner didn't show up
}
