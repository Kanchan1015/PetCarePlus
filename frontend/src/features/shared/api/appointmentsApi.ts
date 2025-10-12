// appointmentsApi.ts
// Purpose: API interface for all appointment operations (CRUD) with proper error handling and authentication
// Features: Owner endpoints (create, getMy), Admin endpoints (getAll, getPending, updateStatus), Vet endpoints (getMyAssigned)
// Integration: Works with backend /api/appointments/* endpoints, includes TypeScript typing and token authentication

import { getToken } from '../../auth/token';
import type { Appointment, CreateAppointmentRequest, UpdateAppointmentStatusRequest, AppointmentSummaryReport } from '../types/appointment';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;
const API_BASE = `${API_BASE_URL}/appointments`;

// Get auth token from localStorage
const getAuthHeaders = () => {
  const token = getToken();
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` })
  };
};

// Generic API response handler
const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const errorData = await response.text();
    throw new Error(`HTTP ${response.status}: ${errorData}`);
  }
  
  if (response.status === 204) {
    return {} as T; // No content response
  }
  
  return response.json();
};

export const appointmentsApi = {
  // Sprint Feature: Owner endpoints for appointment management
  // Owner endpoints
  create: async (data: CreateAppointmentRequest): Promise<Appointment> => {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    return handleResponse<Appointment>(response);
  },

  getMy: async (): Promise<Appointment[]> => {
    const response = await fetch(`${API_BASE}/my`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Appointment[]>(response);
  },

  // Sprint Feature: Admin endpoints for appointment approval/rejection
  // Admin endpoints
  getAll: async (): Promise<Appointment[]> => {
    const response = await fetch(API_BASE, {
      headers: getAuthHeaders()
    });
    return handleResponse<Appointment[]>(response);
  },

  getPending: async (): Promise<Appointment[]> => {
    const response = await fetch(`${API_BASE}/pending`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Appointment[]>(response);
  },

  // Sprint Key Feature: Update appointment status (approve/reject) with admin notes
  updateStatus: async (appointmentId: string, data: UpdateAppointmentStatusRequest): Promise<Appointment> => {
    const response = await fetch(`${API_BASE}/${appointmentId}/status`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    return handleResponse<Appointment>(response);
  },

  // Future Enhancement: Vet endpoints for assigned appointments
  // Vet endpoints
  getMyAssigned: async (): Promise<Appointment[]> => {
    const response = await fetch(`${API_BASE}/assigned`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Appointment[]>(response);
  },

  // Vet endpoint: Get all approved appointments
  getApproved: async (): Promise<Appointment[]> => {
    const response = await fetch(`${API_BASE}/approved`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Appointment[]>(response);
  },

  // Vet endpoint: Get all approved appointments (smart method with fallback)
  getAllApprovedForVet: async (): Promise<Appointment[]> => {
    try {
      // Try the new approved endpoint first (VET,ADMIN access)
      return await appointmentsApi.getApproved();
    } catch (error) {
      console.log('Approved endpoint failed, trying assigned appointments fallback...', error);
      // Fallback to assigned appointments only
      const assignedAppointments = await appointmentsApi.getMyAssigned();
      return assignedAppointments.filter(apt => apt.status === 'Approved');
    }
  },

  // Admin endpoint: Get appointment summary report for workload tracking
  getSummaryReport: async (): Promise<AppointmentSummaryReport> => {
    const response = await fetch(`${API_BASE}/summary-report`, {
      headers: getAuthHeaders()
    });
    return handleResponse<AppointmentSummaryReport>(response);
  }
};
