import type { Pet, PetSummary, CreatePetRequest, UpdatePetRequest, AssignPetRequest, UserSelection } from '../types/pet';
import { getToken } from '../../auth/token';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;
const API_BASE = `${API_BASE_URL}/pets`;

// Get auth token from localStorage
const getAuthHeaders = () => {
  const token = getToken();
  console.log('Auth token:', token ? 'Present' : 'Missing'); // Debug log
  console.log('Token value:', token); // Debug: show actual token
  console.log('Full Authorization header:', token ? `Bearer ${token}` : 'No token'); // Debug log
  
  // Debug: decode token to see claims
  if (token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('Token payload:', payload);
      console.log('Token role:', payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']);
      console.log('Token expiration:', new Date(payload.exp * 1000));
    } catch (e) {
      console.error('Failed to decode token:', e);
    }
  }
  
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` })
  };
};

// Generic API response handler
const handleResponse = async <T>(response: Response): Promise<T> => {
  console.log('API Response:', response.status, response.statusText); // Debug log
  
  if (!response.ok) {
    const errorData = await response.text();
    console.error('API Error:', response.status, errorData); // Debug log
    throw new Error(`HTTP ${response.status}: ${errorData}`);
  }
  
  if (response.status === 204) {
    return {} as T; // No content response
  }
  
  return response.json();
};

// Admin Pet Management API
export const adminPetsApi = {
  // Get all pets (Admin only)
  getAll: async (): Promise<PetSummary[]> => {
    const response = await fetch(API_BASE, {
      headers: getAuthHeaders()
    });
    return handleResponse<PetSummary[]>(response);
  },

  // Get pet by ID (Admin only)
  getById: async (id: string): Promise<Pet> => {
    const response = await fetch(`${API_BASE}/${id}`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Pet>(response);
  },

  // Create new pet (Admin only)
  create: async (petData: CreatePetRequest): Promise<Pet> => {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(petData)
    });
    return handleResponse<Pet>(response);
  },

  // Update pet (Admin only)
  update: async (id: string, petData: UpdatePetRequest): Promise<Pet> => {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(petData)
    });
    return handleResponse<Pet>(response);
  },

  // Delete pet (Admin only)
  delete: async (id: string): Promise<void> => {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    return handleResponse<void>(response);
  },

  // Assign pet to owner (Admin only)
  assign: async (assignData: AssignPetRequest): Promise<Pet> => {
    const response = await fetch(`${API_BASE}/${assignData.petId}/assign`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(assignData)
    });
    return handleResponse<Pet>(response);
  },

  // Get users for pet assignment selection (Admin only)
  getUsersForSelection: async (): Promise<UserSelection[]> => {
    const response = await fetch(`${API_BASE}/users/selection`, {
      headers: getAuthHeaders()
    });
    return handleResponse<UserSelection[]>(response);
  }
};

// General Pet API (for both Admin and Owner)
export const petsApi = {
  // Get pet by ID
  getById: async (id: string): Promise<Pet | null> => {
    try {
      const response = await fetch(`${API_BASE}/${id}`, {
        headers: getAuthHeaders()
      });
      return handleResponse<Pet>(response);
    } catch (error) {
      if (error instanceof Error && error.message.includes('404')) {
        return null;
      }
      throw error;
    }
  },

  // Get current user's pets
  getMy: async (): Promise<Pet[]> => {
    const response = await fetch(`${API_BASE}/my-pets`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Pet[]>(response);
  },

  // Debug endpoint to check user claims
  debugMe: async (): Promise<unknown> => {
    const response = await fetch(`${API_BASE}/debug/me`, {
      headers: getAuthHeaders()
    });
    return handleResponse<unknown>(response);
  },

  // Get pets by owner ID
  getByOwner: async (ownerUserId: string): Promise<Pet[]> => {
    const response = await fetch(`${API_BASE}/owner/${ownerUserId}`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Pet[]>(response);
  }
};

// Combined export for convenience
export default {
  admin: adminPetsApi,
  pets: petsApi
};