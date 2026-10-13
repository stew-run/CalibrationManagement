import { del, get, post, put } from './http';

export const certificateApi = {
  search: (params, signal) => get('/certificates', params, signal),
  create: (body) => post('/certificates', body),
  update: (id, body) => put(`/certificates/${id}`, body),
  remove: (id) => del(`/certificates/${id}`)
};
