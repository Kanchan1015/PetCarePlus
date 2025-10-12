// appointment.ts
// Purpose: TypeScript type definitions for appointment data structures
// Features: Complete appointment interface, summary views, request/update DTOs
// Benefits: Type safety, IntelliSense support, compile-time error checking for appointment operations

export interface Appointment {
  id: string;
  petId: string;
  petName: string;
  ownerUserId: string;
  ownerName: string;
  vetUserId?: string;
  vetName?: string;
  requestedDateTime: string;
  actualDateTime?: string;
  reasonForVisit: string;
  notes?: string;
  adminNotes?: string;
  status: 'Pending' | 'Approved' | 'Cancelled' | 'Completed' | 'NoShow';
  statusDisplayName: string;
  createdAt: string;
  updatedAt?: string;
  canBeCancelled: boolean;
  requiresAction: boolean;
}

export interface AppointmentSummary {
  id: string;
  petName: string;
  ownerName: string;
  requestedDateTime: string;
  status: string;
  reasonForVisit: string;
}

export interface CreateAppointmentRequest {
  petId: string;
  requestedDateTime: string;
  reasonForVisit: string;
  notes?: string;
}

export interface UpdateAppointmentStatusRequest {
  status: 'Approved' | 'Cancelled';
  adminNotes?: string;
  vetUserId?: string;
}

export interface AppointmentSummaryReport {
  generatedAt: string;
  reportPeriod: string;
  
  // Upcoming appointments statistics
  upcomingAppointmentsCount: number;
  upcomingAppointments: Appointment[];
  
  // Pending appointments statistics
  pendingAppointmentsCount: number;
  pendingAppointments: Appointment[];
  
  // Past appointments statistics
  pastAppointmentsCount: number;
  pastAppointments: Appointment[];
  
  // Overall statistics
  totalAppointmentsCount: number;
  completedAppointmentsCount: number;
  cancelledAppointmentsCount: number;
  noShowAppointmentsCount: number;
  
  // Workload metrics
  averageAppointmentsPerDay: number;
  busiestDayOfWeek: string;
  peakAppointmentHour: number;
}
