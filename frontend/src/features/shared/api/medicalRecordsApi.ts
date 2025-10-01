// API functions for medical records management

const BASE = import.meta.env.VITE_API_BASE_URL as string;
const TOKEN_KEY = "APP_AT";

if (!BASE) throw new Error("VITE_API_BASE_URL is not set");

import type {
  MedicalRecord,
  CreateMedicalRecordRequest,
  Vaccination,
  CreateVaccinationRequest,
  VaccinationReport,
  Treatment,
  CreateTreatmentRequest,
  TreatmentHistoryReport,
  Prescription,
  CreatePrescriptionRequest
} from '../types/medicalRecords';

// Generic API response types
type ApiResponse<T> = {
  ok: true;
  data: T;
} | {
  ok: false;
  message: string;
  status?: number;
};

// Helper function to get auth headers
function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem(TOKEN_KEY);
  return {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` })
  };
}

// Medical Records API
export const medicalRecordsApi = {
  // Get medical records for a pet
  async getMedicalRecordsByPet(petId: string): Promise<ApiResponse<MedicalRecord[]>> {
    try {
      const response = await fetch(`${BASE}/medicalrecords/pet/${petId}`, {
        method: 'GET',
        headers: getAuthHeaders()
      });

      if (!response.ok) {
        return {
          ok: false,
          message: `Failed to fetch medical records: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  },

  // Create a new medical record
  async createMedicalRecord(request: CreateMedicalRecordRequest): Promise<ApiResponse<MedicalRecord>> {
    try {
      const response = await fetch(`${BASE}/medicalrecords`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: response.statusText }));
        return {
          ok: false,
          message: errorData.message || `Failed to create medical record: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  }
};

// Vaccinations API
export const vaccinationsApi = {
  // Get vaccination report for a pet
  async getVaccinationReport(petId: string): Promise<ApiResponse<VaccinationReport>> {
    try {
      const response = await fetch(`${BASE}/vaccinations/pet/${petId}/report`, {
        method: 'GET',
        headers: getAuthHeaders()
      });

      if (!response.ok) {
        return {
          ok: false,
          message: `Failed to fetch vaccination report: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  },

  // Create a new vaccination record
  async createVaccination(request: CreateVaccinationRequest): Promise<ApiResponse<Vaccination>> {
    try {
      const response = await fetch(`${BASE}/vaccinations`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: response.statusText }));
        return {
          ok: false,
          message: errorData.message || `Failed to create vaccination record: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  }
};

// Treatments API
export const treatmentsApi = {
  // Get treatment history report for a pet
  async getTreatmentHistoryReport(petId: string): Promise<ApiResponse<TreatmentHistoryReport>> {
    try {
      const response = await fetch(`${BASE}/treatments/pet/${petId}/history`, {
        method: 'GET',
        headers: getAuthHeaders()
      });

      if (!response.ok) {
        return {
          ok: false,
          message: `Failed to fetch treatment history: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  },

  // Create a new treatment record
  async createTreatment(request: CreateTreatmentRequest): Promise<ApiResponse<Treatment>> {
    try {
      const response = await fetch(`${BASE}/treatments`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: response.statusText }));
        return {
          ok: false,
          message: errorData.message || `Failed to create treatment record: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  }
};

// Prescriptions API
export const prescriptionsApi = {
  // Create a new prescription
  async createPrescription(request: CreatePrescriptionRequest): Promise<ApiResponse<Prescription>> {
    try {
      const response = await fetch(`${BASE}/prescriptions`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: response.statusText }));
        return {
          ok: false,
          message: errorData.message || `Failed to create prescription: ${response.statusText}`,
          status: response.status
        };
      }

      const data = await response.json();
      return { ok: true, data };
    } catch (error) {
      return {
        ok: false,
        message: error instanceof Error ? error.message : 'Unknown error occurred'
      };
    }
  }
};