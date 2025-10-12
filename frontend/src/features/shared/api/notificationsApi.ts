// notificationsApi.ts
// Purpose: API interface for in-app notification system with read/unread state management
// Features: Get notifications, mark as read (single/bulk), unread count tracking
// Integration: Works with backend /api/notifications/* endpoints for real-time notification updates

import { getToken } from '../../auth/token';
import type { Notification } from '../types/notification';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;
const API_BASE = `${API_BASE_URL}/notifications`;

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

export const notificationsApi = {
  // Sprint Feature: Get user's notifications for the notification bell dropdown
  // Get user's notifications
  getMy: async (): Promise<Notification[]> => {
    const response = await fetch(`${API_BASE}/my`, {
      headers: getAuthHeaders()
    });
    return handleResponse<Notification[]>(response);
  },

  // Sprint Feature: Mark single notification as read when user clicks on it
  // Mark notification as read
  markAsRead: async (notificationId: string): Promise<void> => {
    const response = await fetch(`${API_BASE}/${notificationId}/read`, {
      method: 'PUT',
      headers: getAuthHeaders()
    });
    return handleResponse<void>(response);
  },

  // Sprint Feature: Mark all notifications as read (bulk operation)
  // Mark all notifications as read
  markAllAsRead: async (): Promise<void> => {
    const response = await fetch(`${API_BASE}/mark-all-read`, {
      method: 'PUT',
      headers: getAuthHeaders()
    });
    return handleResponse<void>(response);
  },

  // Sprint Feature: Get unread count for notification bell badge
  // Get unread count
  getUnreadCount: async (): Promise<{ count: number }> => {
    const response = await fetch(`${API_BASE}/unread-count`, {
      headers: getAuthHeaders()
    });
    return handleResponse<{ count: number }>(response);
  }
};
