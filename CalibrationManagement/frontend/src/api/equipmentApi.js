import { del, get, post, put } from './http';

export const equipmentApi = {
  search: (params, signal) => get('/equipment', params, signal),
  get: (id, signal) => get(`/equipment/${id}`, undefined, signal),
  create: (body) => post('/equipment', body),
  update: (id, body) => put(`/equipment/${id}`, body),
  remove: (id) => del(`/equipment/${id}`)
};
