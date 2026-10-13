import { get, post } from './http';

export const notificationApi = {
  list: (params, signal) => get('/notifications', params, signal),
  unreadCount: (signal) => get('/notifications/unread-count', undefined, signal),
  markRead: (id) => post(`/notifications/${id}/read`),
  markAllRead: () => post('/notifications/read-all'),
  refresh: () => post('/notifications/refresh')
};
