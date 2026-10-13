import { CALIBRATION_STATUS } from '../../utils/format';

export default function StatusBadge({ value, className = '' }) {
  const meta = CALIBRATION_STATUS[value] || { label: value || '—', tone: 'neutral' };
  return <span className={`badge badge-${meta.tone} ${className}`}>{meta.label}</span>;
}
