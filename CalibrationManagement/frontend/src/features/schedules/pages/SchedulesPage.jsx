import { useNavigate } from 'react-router-dom';
import ResourcePage from '../../../components/ResourcePage';
import StatusBadge from '../../../components/ui/StatusBadge';
import { useAuth } from '../../../auth/useAuth';
import { scheduleApi } from '../../../api/index';
import { formatDate, toInputDate } from '../../../utils/format';

const STATUS_FILTER = ['Planned', 'DueSoon', 'Overdue', 'Completed', 'Cancelled'].map((v) => ({ value: v, label: v === 'DueSoon' ? 'Due soon' : v }));
const EDIT_STATUS = ['Planned', 'Cancelled', 'Completed'].map((v) => ({ value: v, label: v }));
const YEAR = new Date().getFullYear();

const fields = (lookups, mode) => [
  ...(mode === 'edit' ? [] : [{ name: 'equipmentId', label: 'Equipment', type: 'equipment', required: true, span: 2 }]),
  { name: 'planYear', label: 'Plan year', type: 'number', required: true, min: 2000, max: 2100, step: 1 },
  { name: 'dueDate', label: 'Due date', type: 'date', required: true },
  { name: 'assignedTo', label: 'Assigned to', maxLength: 100 },
  ...(mode === 'edit' ? [{ name: 'status', label: 'Status', type: 'select', allowEmpty: false, options: EDIT_STATUS, help: 'A plan entry becomes Completed by recording a calibration result for it.' }] : []),
  { name: 'notes', label: 'Notes', type: 'textarea', maxLength: 500, span: 2 }
];

export default function SchedulesPage() {
  const navigate = useNavigate();
  const { can } = useAuth();

  const columns = [
    { key: 'equipmentCode', header: 'Code', render: (r) => <strong>{r.equipmentCode}</strong> },
    { key: 'equipmentName', header: 'Instrument' },
    { key: 'departmentName', header: 'Department' },
    { key: 'dueDate', header: 'Due date', render: (r) => formatDate(r.dueDate) },
    {
      key: 'daysUntilDue',
      header: 'Days',
      render: (r) => (r.status !== 'Planned' ? '—' : r.daysUntilDue < 0 ? `${-r.daysUntilDue} overdue` : `${r.daysUntilDue} left`)
    },
    { key: 'assignedTo', header: 'Assigned to' },
    { key: 'displayStatus', header: 'Status', render: (r) => <StatusBadge value={r.displayStatus} /> },
    { key: 'planYear', header: 'Plan year' }
  ];

  return (
    <ResourcePage
      title="Calibration plan"
      subtitle="Planned calibrations with due and overdue tracking (QA.FMT-28)"
      entityName="Calibration plan entry"
      createLabel="Add plan entry"
      api={scheduleApi}
      idKey="scheduleId"
      columns={columns}
      filters={[
        { name: 'search', type: 'search', label: 'Search', placeholder: 'Code, name, assigned to…' },
        { name: 'status', type: 'select', label: 'Status', options: STATUS_FILTER },
        { name: 'planYear', type: 'number', label: 'Plan year', placeholder: String(YEAR) },
        { name: 'from', type: 'date', label: 'Due from' },
        { name: 'to', type: 'date', label: 'Due to' }
      ]}
      initialFilters={{ search: '', status: '', planYear: '', from: '', to: '' }}
      fields={fields}
      emptyForm={{ planYear: YEAR }}
      toForm={(r) => ({ planYear: r.planYear, dueDate: toInputDate(r.dueDate), assignedTo: r.assignedTo ?? '', status: r.status, notes: r.notes ?? '' })}
      canEditRow={(r) => r.status !== 'Completed'}
      deleteMessage={(r) => `Delete the plan entry for ${r.equipmentCode} due ${formatDate(r.dueDate)}? Entries with recorded results cannot be deleted – cancel them instead.`}
      rowActions={(r) => (can('write') && r.status === 'Planned'
        ? [{
            label: 'Record calibration result',
            icon: 'check',
            onClick: () => navigate('/results', { state: { schedule: { scheduleId: r.scheduleId, equipmentId: r.equipmentId, label: `${r.equipmentCode} – ${r.equipmentName}`, dueDate: r.dueDate } } })
          }]
        : [])}
    />
  );
}
