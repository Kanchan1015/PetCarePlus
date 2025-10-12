// notification.ts  
// Purpose: TypeScript type definitions for the in-app notification system
// Features: Notification interface with different types (appointment_approved, appointment_cancelled, etc.)
// Benefits: Type safety for notification operations, structured data for notification bell component

export interface Notification {
  id: string;
  userId: string;
  type: 'appointment_approved' | 'appointment_cancelled' | 'appointment_assigned' | 'general';
  title: string;
  message: string;
  data?: {
    appointmentId?: string;
    petId?: string;
    vetId?: string;
    [key: string]: any;
  };
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export interface CreateNotificationRequest {
  userId: string;
  type: Notification['type'];
  title: string;
  message: string;
  data?: Notification['data'];
}
