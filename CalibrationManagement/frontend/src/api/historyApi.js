import { get } from './http';

export const historyApi = { get: (equipmentId, signal) => get(`/history/${equipmentId}`, undefined, signal) };
