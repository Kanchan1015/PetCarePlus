// Medical Record Types
export const MedicalRecordType = {
  Vaccination: 1,
  Treatment: 2,
  Checkup: 3,
  Surgery: 4,
  Emergency: 5,
  Prescription: 6,
  Other: 99
} as const;

export type MedicalRecordType = typeof MedicalRecordType[keyof typeof MedicalRecordType];

export const MEDICAL_RECORD_TYPE_LABELS: Record<MedicalRecordType, string> = {
  [MedicalRecordType.Vaccination]: 'Vaccination',
  [MedicalRecordType.Treatment]: 'Treatment',
  [MedicalRecordType.Checkup]: 'Checkup',
  [MedicalRecordType.Surgery]: 'Surgery',
  [MedicalRecordType.Emergency]: 'Emergency',
  [MedicalRecordType.Prescription]: 'Prescription',
  [MedicalRecordType.Other]: 'Other'
};

// Vaccination Status
export const VaccinationStatus = {
  Current: 1,
  Overdue: 2,
  Upcoming: 3,
  Expired: 4
} as const;

export type VaccinationStatus = typeof VaccinationStatus[keyof typeof VaccinationStatus];

export const VACCINATION_STATUS_LABELS: Record<VaccinationStatus, string> = {
  [VaccinationStatus.Current]: 'Current',
  [VaccinationStatus.Overdue]: 'Overdue',
  [VaccinationStatus.Upcoming]: 'Upcoming',
  [VaccinationStatus.Expired]: 'Expired'
};

// Treatment Status
export const TreatmentStatus = {
  Planned: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4
} as const;

export type TreatmentStatus = typeof TreatmentStatus[keyof typeof TreatmentStatus];

export const TREATMENT_STATUS_LABELS: Record<TreatmentStatus, string> = {
  [TreatmentStatus.Planned]: 'Planned',
  [TreatmentStatus.InProgress]: 'In Progress',
  [TreatmentStatus.Completed]: 'Completed',
  [TreatmentStatus.Cancelled]: 'Cancelled'
};

// Prescription Status
export const PrescriptionStatus = {
  Active: 1,
  Completed: 2,
  Cancelled: 3,
  Expired: 4
} as const;

export type PrescriptionStatus = typeof PrescriptionStatus[keyof typeof PrescriptionStatus];

export const PRESCRIPTION_STATUS_LABELS: Record<PrescriptionStatus, string> = {
  [PrescriptionStatus.Active]: 'Active',
  [PrescriptionStatus.Completed]: 'Completed',
  [PrescriptionStatus.Cancelled]: 'Cancelled',
  [PrescriptionStatus.Expired]: 'Expired'
};

// Medical Record Interfaces
export interface MedicalRecord {
  id: string;
  petId: string;
  vetUserId: string;
  vetFullName?: string;
  recordType: MedicalRecordType;
  recordDate: string;
  title: string;
  description?: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateMedicalRecordRequest {
  petId: string;
  recordType: MedicalRecordType;
  recordDate: string;
  title: string;
  description?: string;
  notes?: string;
}

// Vaccination Interfaces
export interface Vaccination {
  id: string;
  petId: string;
  vetUserId: string;
  vetFullName?: string;
  vaccineName: string;
  vaccinationDate: string;
  nextDueDate?: string;
  batchNumber?: string;
  manufacturer?: string;
  status: VaccinationStatus;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateVaccinationRequest {
  petId: string;
  vaccineName: string;
  vaccinationDate: string;
  nextDueDate?: string;
  batchNumber?: string;
  manufacturer?: string;
  notes?: string;
}

export interface VaccinationReport {
  petId: string;
  petName: string;
  vaccinations: Vaccination[];
  overdueVaccinations: Vaccination[];
  upcomingVaccinations: Vaccination[];
  isUpToDate: boolean;
}

// Treatment Interfaces
export interface Treatment {
  id: string;
  petId: string;
  vetUserId: string;
  vetFullName?: string;
  treatmentType: string;
  diagnosis: string;
  treatmentDescription?: string;
  treatmentDate: string;
  followUpDate?: string;
  status: TreatmentStatus;
  medications?: string;
  instructions?: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTreatmentRequest {
  petId: string;
  treatmentType: string;
  diagnosis: string;
  treatmentDescription?: string;
  treatmentDate: string;
  followUpDate?: string;
  medications?: string;
  instructions?: string;
  notes?: string;
}

export interface TreatmentHistoryReport {
  petId: string;
  petName: string;
  treatments: Treatment[];
  lastTreatmentDate?: string;
  totalTreatments: number;
}

// Prescription Interfaces
export interface Prescription {
  id: string;
  petId: string;
  vetUserId: string;
  vetFullName?: string;
  medicationName: string;
  dosage: string;
  frequency: string;
  prescribedDate: string;
  startDate?: string;
  endDate?: string;
  durationDays?: number;
  instructions?: string;
  status: PrescriptionStatus;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePrescriptionRequest {
  petId: string;
  medicationName: string;
  dosage: string;
  frequency: string;
  prescribedDate: string;
  startDate?: string;
  endDate?: string;
  durationDays?: number;
  instructions?: string;
  notes?: string;
}