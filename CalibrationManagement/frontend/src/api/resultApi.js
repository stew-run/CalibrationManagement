import { del, get, post, put } from './http';

export const resultApi = {
  search: (params, signal) => get('/results', params, signal),
  create: (body) => post('/results', body),
  update: (id, body) => put(`/results/${id}`, body),
  remove: (id) => del(`/results/${id}`)
};
