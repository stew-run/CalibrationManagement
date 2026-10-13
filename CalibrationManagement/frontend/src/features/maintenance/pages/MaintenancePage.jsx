import ResourcePage from '../../../components/ResourcePage';
import StatusBadge from '../../../components/ui/StatusBadge';
import { maintenanceApi } from '../../../api/index';
import { formatDate, toInputDate } from '../../../utils/format';

const TYPES = ['Preventive', 'Corrective', 'Repair', 'Inspection'].map((v) => ({ value: v, label: v }));
const STATUSES = [{ value: 'Open', label: 'Open' }, { value: 'InProgress', label: 'In progress' }, { value: 'Completed', label: 'Completed' }];

const fields = (lookups, mode) => [
  ...(mode === 'edit' ? [] : [{ name: 'equipmentId', label: 'Equipment', type: 'equipment', required: true, span: 2 }]),
  { name: 'maintenanceType', label: 'Type', type: 'select', allowEmpty: false, options: TYPES },
  { name: 'status', label: 'Status', type: 'select', allowEmpty: false, options: STATUSES },
  { name: 'maintenanceDate', label: 'Maintenance date', type: 'date', required: true },
  { name: 'nextMaintenanceDate', label: 'Next maintenance date', type: 'date' },
  { name: 'performedBy', label: 'Performed by', maxLength: 100 },
  { name: 'cost', label: 'Cost', type: 'number', min: 0, step: '0.01' },
  { name: 'description', label: 'Description', type: 'textarea', required: true, maxLength: 1000, span: 2 }
];

export default function MaintenancePage() {
  const columns = [
    { key: 'equipmentCode', header: 'Equipment', render: (r) => <><strong>{r.equipmentCode}</strong> – {r.equipmentName}</> },
    { key: 'maintenanceType', header: 'Type' },
    { key: 'maintenanceDate', header: 'Date', render: (r) => formatDate(r.maintenanceDate) },
    { key: 'description', header: 'Description' },
    { key: 'performedBy', header: 'Performed by' },
    { key: 'cost', header: 'Cost', align: 'right', render: (r) => (r.cost === null || r.cost === undefined ? '—' : Number(r.cost).toFixed(2)) },
    { key: 'nextMaintenanceDate', header: 'Next', render: (r) => formatDate(r.nextMaintenanceDate) },
    { key: 'status', header: 'Status', render: (r) => <StatusBadge value={r.status} /> }
  ];

  return (
    <ResourcePage
      title="Maintenance"
      subtitle="Preventive and corrective maintenance records"
      entityName="Maintenance record"
      api={maintenanceApi}
      idKey="maintenanceId"
      columns={columns}
      filters={[
        { name: 'search', type: 'search', label: 'Search', placeholder: 'Equipment, description, technician…' },
        { name: 'maintenanceType', type: 'select', label: 'Type', options: TYPES },
        { name: 'status', type: 'select', label: 'Status', options: STATUSES }
      ]}
      initialFilters={{ search: '', maintenanceType: '', status: '' }}
      fields={fields}
      emptyForm={{ maintenanceType: 'Preventive', status: 'Open' }}
      toForm={(r) => ({
        maintenanceType: r.maintenanceType, status: r.status, maintenanceDate: toInputDate(r.maintenanceDate), nextMaintenanceDate: toInputDate(r.nextMaintenanceDate),
        performedBy: r.performedBy ?? '', cost: r.cost ?? '', description: r.description
      })}
      deleteMessage={(r) => `Delete the ${r.maintenanceType.toLowerCase()} maintenance record of ${r.equipmentCode}?`}
    />
  );
}
