import { get, post, put } from './http';

export const userApi = {
  list: (params, signal) => get('/users', params, signal),
  roles: (signal) => get('/users/roles', undefined, signal),
  create: (body) => post('/users', body),
  update: (id, body) => put(`/users/${id}`, body),
  resetPassword: (id, newPassword) => post(`/users/${id}/reset-password`, { newPassword })
};
