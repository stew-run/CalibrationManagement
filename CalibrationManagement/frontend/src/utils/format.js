// Dates from the API: SQL "date" columns arrive as "2026-10-12T00:00:00" (no timezone) and must be shown as-is.
// SQL "datetime2" audit columns are UTC without a "Z" suffix and are converted to local time.
export function formatDate(value) {
  if (!value) return '—';
  const [y, m, d] = String(value).slice(0, 10).split('-');
  if (!y || !m || !d) return String(value);
  return `${d}.${m}.${y}`; // dd.MM.yyyy, the format used in the Excel templates
}

export function toInputDate(value) {
  return value ? String(value).slice(0, 10) : '';
}

export function formatDateTime(value) {
  if (!value) return '—';
  const iso = /[zZ]|[+-]\d\d:\d\d$/.test(value) ? value : `${value}Z`;
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
}

export function formatNumber(value) {
  return value === null || value === undefined ? '—' : new Intl.NumberFormat().format(value);
}

export function formatBytes(bytes) {
  if (bytes === null || bytes === undefined) return '—';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export const CALIBRATION_STATUS = {
  Valid: { label: 'Valid', tone: 'success' },
  DueSoon: { label: 'Due soon', tone: 'warning' },
  Overdue: { label: 'Overdue', tone: 'danger' },
  Failed: { label: 'Failed', tone: 'danger' },
  NotCalibrated: { label: 'Not calibrated', tone: 'neutral' },
  Planned: { label: 'Planned', tone: 'info' },
  Completed: { label: 'Completed', tone: 'success' },
  Cancelled: { label: 'Cancelled', tone: 'neutral' },
  OK: { label: 'OK', tone: 'success' },
  NOTOK: { label: 'NOT OK', tone: 'danger' },
  Unverified: { label: 'Unverified', tone: 'warning' },
  Active: { label: 'Active', tone: 'success' },
  Inactive: { label: 'Inactive', tone: 'neutral' },
  OutOfService: { label: 'Out of service', tone: 'danger' },
  Retired: { label: 'Retired', tone: 'neutral' },
  Open: { label: 'Open', tone: 'warning' },
  InProgress: { label: 'In progress', tone: 'info' }
};

export const lowerFirst = (s) => (s ? s.charAt(0).toLowerCase() + s.slice(1) : s);
