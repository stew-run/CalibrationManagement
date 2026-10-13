import { get, post } from './http';
import api, { clean } from './axiosClient';

export const reportApi = {
  calibration: (params, signal) => get('/reports/calibration', params, signal),
  agencySummary: (params, signal) => get('/reports/agency-summary', params, signal),
  dataQuality: (params, signal) => get('/reports/data-quality', params, signal),
  resolveIssue: (id, isResolved) => post(`/reports/data-quality/${id}/resolve`, { isResolved }),
  async exportCalibration(params) {
    const response = await api.get('/reports/calibration/export', { params: clean(params), responseType: 'blob' });
    const disposition = response.headers['content-disposition'] || '';
    const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposition);
    return { blob: response.data, filename: match ? decodeURIComponent(match[1]) : 'calibration-report.csv' };
  }
};
