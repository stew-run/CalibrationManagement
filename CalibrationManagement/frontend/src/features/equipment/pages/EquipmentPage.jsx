import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ResourcePage from '../../../components/ResourcePage';
import Button from '../../../components/ui/Button';
import Dropdown from '../../../components/ui/Dropdown';
import FormModal from '../../../components/ui/FormModal';
import Icon from '../../../components/ui/Icon';
import Modal from '../../../components/ui/Modal';
import StatusBadge from '../../../components/ui/StatusBadge';
import { useAuth } from '../../../auth/useAuth';
import { useLookups } from '../../../contexts/LookupsContext';
import { useToast } from '../../../contexts/ToastContext';
import { equipmentApi, lookupApi } from '../../../api/index';
import { CALIBRATION_STATUS, formatDate } from '../../../utils/format';

const opt = (values) => values.map((v) => ({ value: v, label: CALIBRATION_STATUS[v]?.label ?? v }));
const STATUS = opt(['Active', 'Inactive', 'OutOfService', 'Retired']);
const CAL_STATUS = opt(['Valid', 'DueSoon', 'Overdue', 'Failed', 'NotCalibrated']);
const CAL_TYPES = [{ value: 'External', label: 'External' }, { value: 'Internal', label: 'Internal' }, { value: 'Customer', label: 'Customer' }];
const toOptions = (list) => list.map((x) => ({ value: x.id, label: x.name }));

const fields = (lookups) => [
  { name: 'equipmentCode', label: 'Instrument code no.', required: true, maxLength: 50 },
  { name: 'equipmentName', label: 'Instrument name', required: true, maxLength: 250 },
  { name: 'rangeLeastCount', label: 'Range & least count', maxLength: 150 },
  { name: 'make', label: 'Make', maxLength: 100 },
  { name: 'serialNo', label: 'Serial no.', maxLength: 100 },
  { name: 'acceptableErrorLimit', label: 'Acceptable error limit', maxLength: 80 },
  { name: 'departmentId', label: 'Issue department', type: 'select', valueType: 'int', options: toOptions(lookups.departments) },
  { name: 'location', label: 'Location', maxLength: 150 },
  { name: 'agencyId', label: 'Calibration agency', type: 'select', valueType: 'int', options: toOptions(lookups.agencies) },
  { name: 'frequencyId', label: 'Calibration frequency', type: 'select', valueType: 'int', options: toOptions(lookups.frequencies) },
  { name: 'calibrationType', label: 'Calibration at', type: 'select', allowEmpty: false, options: CAL_TYPES },
  { name: 'responsiblePerson', label: 'Responsible person', maxLength: 100 },
  { name: 'groupName', label: 'Group / section', maxLength: 150, help: 'e.g. a section heading such as "AIR GAUGE - MASTERS"' },
  { name: 'status', label: 'Status', type: 'select', allowEmpty: false, options: STATUS },
  { name: 'remarks', label: 'Remarks', type: 'textarea', maxLength: 500, span: 2 },
  { name: 'allowDuplicateCode', type: 'checkbox', label: 'Duplicate code', checkboxLabel: 'Allow a duplicate instrument code (legacy data only)', span: 2 }
];

const EMPTY = { calibrationType: 'External', status: 'Active', allowDuplicateCode: false };

const toForm = (e) => ({
  equipmentCode: e.equipmentCode, equipmentName: e.equipmentName, rangeLeastCount: e.rangeLeastCount ?? '', make: e.make ?? '', serialNo: e.serialNo ?? '',
  acceptableErrorLimit: e.acceptableErrorLimit ?? '', departmentId: e.departmentId ?? '', location: e.location ?? '', agencyId: e.agencyId ?? '',
  frequencyId: e.frequencyId ?? '', calibrationType: e.calibrationType, responsiblePerson: e.responsiblePerson ?? '', groupName: e.groupName ?? '',
  status: e.status, remarks: e.remarks ?? '', allowDuplicateCode: e.codeCount > 1
});

function Detail({ label, children }) {
  return <div className="detail"><dt>{label}</dt><dd>{children || '—'}</dd></div>;
}

export default function EquipmentPage() {
  const navigate = useNavigate();
  const toast = useToast();
  const { can } = useAuth();
  const lookups = useLookups();
  const [viewing, setViewing] = useState(null);
  const [addList, setAddList] = useState(null); // 'department' | 'agency'

  const filters = [
    { name: 'search', type: 'search', label: 'Search', placeholder: 'Code, name, make, department…' },
    { name: 'departmentId', type: 'select', label: 'Department', options: toOptions(lookups.departments) },
    { name: 'agencyId', type: 'select', label: 'Agency', options: toOptions(lookups.agencies) },
    { name: 'status', type: 'select', label: 'Status', options: STATUS },
    { name: 'calibrationStatus', type: 'select', label: 'Calibration', options: CAL_STATUS },
    { name: 'duplicatesOnly', type: 'checkbox', label: 'Duplicate codes only' }
  ];

  const columns = [
    { key: 'equipmentCode', header: 'Code', render: (r) => (<><strong>{r.equipmentCode}</strong>{r.codeCount > 1 ? <span className="badge badge-warning ml" title="This code is used by more than one record">Duplicate ×{r.codeCount}</span> : null}</>) },
    { key: 'equipmentName', header: 'Instrument' },
    { key: 'rangeLeastCount', header: 'Range / least count' },
    { key: 'departmentName', header: 'Department' },
    { key: 'agencyName', header: 'Agency' },
    { key: 'nextDueDate', header: 'Next due', render: (r) => formatDate(r.nextDueDate) },
    { key: 'calibrationStatus', header: 'Calibration', render: (r) => <StatusBadge value={r.calibrationStatus} /> },
    { key: 'status', header: 'Status', render: (r) => <StatusBadge value={r.status} /> }
  ];

  const addListFields = [{ name: 'name', label: addList === 'agency' ? 'Agency name' : 'Department name', required: true, maxLength: addList === 'agency' ? 200 : 150 }];

  return (
    <>
      <ResourcePage
        title="Equipment register"
        subtitle="Monitoring & measuring equipment (QA.FMT-27)"
        entityName="Equipment"
        api={equipmentApi}
        idKey="equipmentId"
        columns={columns}
        filters={filters}
        initialFilters={{ search: '', departmentId: '', agencyId: '', status: '', calibrationStatus: '', duplicatesOnly: false }}
        fields={fields}
        toForm={toForm}
        emptyForm={EMPTY}
        deleteMessage={(r) => `Delete ${r.equipmentCode} – ${r.equipmentName}? It is removed from the register and its open plan entries are cancelled; calibration history is kept in the database.`}
        rowActions={(r) => [
          { label: 'View', icon: 'eye', onClick: () => setViewing(r) },
          { label: 'History card', icon: 'history', onClick: () => navigate(`/history/${r.equipmentId}`) }
        ]}
        headerActions={can('write') ? (
          <Dropdown
            label="Manage lists"
            trigger={<span className="btn btn-secondary btn-md"><span>Manage lists</span><Icon name="chevronDown" size={14} /></span>}
            items={[
              { label: 'Add department', onClick: () => setAddList('department') },
              { label: 'Add calibration agency', onClick: () => setAddList('agency') }
            ]}
          />
        ) : null}
      />

      <FormModal
        open={Boolean(addList)}
        title={addList === 'agency' ? 'Add calibration agency' : 'Add department'}
        fields={addListFields}
        initialValues={{ name: '' }}
        size="sm"
        submitLabel="Add"
        onClose={() => setAddList(null)}
        onSubmit={async (payload) => {
          if (addList === 'agency') await lookupApi.createAgency(payload.name);
          else await lookupApi.createDepartment(payload.name);
          lookups.reload();
          toast.success('Added. It is now available in the equipment form.');
        }}
      />

      <Modal open={Boolean(viewing)} title={viewing ? `${viewing.equipmentCode} – ${viewing.equipmentName}` : ''} onClose={() => setViewing(null)} size="lg"
        footer={viewing ? (<><Button onClick={() => setViewing(null)}>Close</Button><Button variant="primary" icon="history" onClick={() => navigate(`/history/${viewing.equipmentId}`)}>Open history card</Button></>) : null}>
        {viewing ? (
          <dl className="details">
            <Detail label="Range & least count">{viewing.rangeLeastCount}</Detail>
            <Detail label="Make">{viewing.make}</Detail>
            <Detail label="Serial no.">{viewing.serialNo}</Detail>
            <Detail label="Acceptable error limit">{viewing.acceptableErrorLimit}</Detail>
            <Detail label="Issue department">{viewing.departmentName}</Detail>
            <Detail label="Location">{viewing.location}</Detail>
            <Detail label="Calibration agency">{viewing.agencyName}</Detail>
            <Detail label="Calibration at">{viewing.calibrationType}</Detail>
            <Detail label="Frequency">{viewing.frequencyLabel}</Detail>
            <Detail label="Responsible person">{viewing.responsiblePerson}</Detail>
            <Detail label="Group / section">{viewing.groupName}</Detail>
            <Detail label="Status"><StatusBadge value={viewing.status} /></Detail>
            <Detail label="Last calibration">{formatDate(viewing.lastCalibrationDate)}</Detail>
            <Detail label="Next due date">{formatDate(viewing.nextDueDate)}</Detail>
            <Detail label="Calibration status"><StatusBadge value={viewing.calibrationStatus} /></Detail>
            <Detail label="Remarks">{viewing.remarks}</Detail>
          </dl>
        ) : null}
      </Modal>
    </>
  );
}
