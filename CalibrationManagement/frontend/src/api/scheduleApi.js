import { del, get, post, put } from './http';

export const scheduleApi = {
  search: (params, signal) => get('/schedules', params, signal),
  create: (body) => post('/schedules', body),
  update: (id, body) => put(`/schedules/${id}`, body),
  remove: (id) => del(`/schedules/${id}`)
};
