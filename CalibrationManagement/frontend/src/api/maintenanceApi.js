import { del, get, post, put } from './http';

export const maintenanceApi = {
  search: (params, signal) => get('/maintenance', params, signal),
  create: (body) => post('/maintenance', body),
  update: (id, body) => put(`/maintenance/${id}`, body),
  remove: (id) => del(`/maintenance/${id}`)
};
