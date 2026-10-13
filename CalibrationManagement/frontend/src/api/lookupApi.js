import { get, post } from './http';

export const lookupApi = {
  all: (signal) => get('/lookups', undefined, signal),
  createDepartment: (name) => post('/lookups/departments', { name }),
  createAgency: (name) => post('/lookups/agencies', { name })
};
