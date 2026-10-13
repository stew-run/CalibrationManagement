import axios from 'axios';
import { tokenStore } from '../auth/tokenStorage';
import { lowerFirst } from '../utils/format';

export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001/api').replace(/\/+$/, '');

export class ApiError extends Error {
  constructor(message, { status = 0, fieldErrors = {}, traceId = null, network = false } = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.fieldErrors = fieldErrors;
    this.traceId = traceId;
    this.network = network;
  }
}

function toApiError(error) {
  if (axios.isCancel(error)) return error;
  if (error.response) {
    const { status, data } = error.response;
    const fieldErrors = {};
    if (data && typeof data === 'object' && data.errors && typeof data.errors === 'object') {
      Object.entries(data.errors).forEach(([key, messages]) => {
        fieldErrors[lowerFirst(key)] = Array.isArray(messages) ? messages.join(' ') : String(messages);
      });
    }
    let message = (data && typeof data === 'object' && (data.detail || data.title)) || `The server returned an error (HTTP ${status}).`;
    if (status === 400 && Object.keys(fieldErrors).length > 0 && (!data.detail || data.detail === 'One or more validation errors occurred.')) {
      message = 'Please correct the highlighted fields.';
    }
    if (status === 403) message = 'You do not have permission to perform this action.';
    if (status === 429) message = 'Too many attempts. Please wait a minute and try again.';
    return new ApiError(message, { status, fieldErrors, traceId: data?.traceId ?? null });
  }
  if (error.code === 'ECONNABORTED') {
    return new ApiError('The API did not respond in time. Check that the backend is running and SQL Server is reachable.', { network: true });
  }
  return new ApiError(
    `Cannot reach the API at ${API_BASE_URL}. Make sure the backend is running, that your browser trusts its HTTPS certificate ` +
      '(run "dotnet dev-certs https --trust") and that VITE_API_BASE_URL matches the backend address.',
    { network: true }
  );
}

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 20000,
  headers: { Accept: 'application/json' }
});

api.interceptors.request.use((config) => {
  const token = tokenStore.get();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const apiError = toApiError(error);
    const isLogin = error?.config?.url?.includes('/auth/login');
    if (apiError.status === 401 && !isLogin) {
      tokenStore.clear();
      window.dispatchEvent(new CustomEvent('cms:unauthorized'));
    }
    return Promise.reject(apiError);
  }
);

/** Drops empty values so optional filters are not sent as "undefined" / empty strings. */
export function clean(params = {}) {
  return Object.fromEntries(
    Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== '' && v !== false)
  );
}

export default api;
