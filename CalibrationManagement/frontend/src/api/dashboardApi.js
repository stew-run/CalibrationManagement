import { get } from './http';

export const dashboardApi = { get: (signal) => get('/dashboard', undefined, signal) };
