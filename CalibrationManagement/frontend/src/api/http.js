import api, { clean } from './axiosClient';

// Small helpers shared by the *Api.js modules: each returns the response body.
export const get = (url, params, signal) => api.get(url, { params: clean(params), signal }).then((r) => r.data);
export const post = (url, body) => api.post(url, body).then((r) => r.data);
export const put = (url, body) => api.put(url, body).then((r) => r.data);
export const del = (url) => api.delete(url).then((r) => r.data);
