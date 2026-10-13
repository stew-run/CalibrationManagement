import api from './axiosClient';

export const healthApi = {
  check: () => api.get('/health', { timeout: 6000 }).then((r) => r.data)
};
