import { get, post, put } from './http';

export const authApi = {
  login: (username, password) => post('/auth/login', { username, password }),
  me: (signal) => get('/auth/me', undefined, signal),
  updateProfile: (body) => put('/auth/profile', body),
  changePassword: (body) => post('/auth/change-password', body)
};
