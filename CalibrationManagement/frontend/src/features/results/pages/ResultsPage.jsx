import { useMemo } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import ResourcePage from '../../../components/ResourcePage';
import StatusBadge from '../../../components/ui/StatusBadge';
import { useLookups } from '../../../contexts/LookupsContext';
import { resultApi } from '../../../api/index';
import { formatDate, toInputDate } from '../../../utils/format';

const RESULTS = [{ value: 'OK', label: 'OK' }, { value: 'NOTOK', label: 'NOT OK' }, { value: 'Unverified', label: 'Unverified' }];
const toOptions = (list) => list.map((x) => ({ value: x.id, label: x.name }));

const fields = (lookups, mode) => [
  ...(mode === 'edit' ? [] : [{ name: 'equipmentId', label: 'Equipment', type: 'equipment', required: true, span: 2 }]),
  { name: 'calibrationDate', label: 'Calibration date', type: 'date', required: true },
  { name: 'dueDate', label: 'Next due date', type: 'date', help: 'Leave empty to calculate it from the equipment frequency (1 year → one day before the anniversary).' },
  { name: 'certificateReference', label: 'Calibration certificate ref.', maxLength: 100 },
  { name: 'agencyId', label: 'Calibration agency', type: 'select', valueType: 'int', options: toOptions(lookups.agencies) },
  { name: 'result', label: 'Result', type: 'select', allowEmpty: false, options: RESULTS },
  { name: 'signedBy', label: 'Signed by', maxLength: 100 },
  { name: 'errorDetails', label: 'Error details', type: 'textarea', maxLength: 500, span: 2 },
  { name: 'actionTaken', label: 'If not OK, action taken', type: 'textarea', maxLength: 500, span: 2, help: 'Required when the result is NOT OK.' },
  { name: 'remarks', label: 'Remarks', type: 'textarea', maxLength: 500, span: 2 }
];

export default function ResultsPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const lookups = useLookups();
  const schedule = location.state?.schedule;

  // Arriving from "Record calibration result" on the plan page opens the form pre-filled.
  const initialAction = useMemo(() => (schedule
    ? {
        equipmentId: schedule.equipmentId,
        equipmentIdLabel: schedule.label,
        scheduleId: schedule.scheduleId ?? '',
        dueDate: '',
        result: 'OK'
      }
    : null), [schedule]);

  const columns = [
    { key: 'equipmentCode', header: 'Code', render: (r) => <strong>{r.equipmentCode}</strong> },
    { key: 'equipmentName', header: 'Instrument' },
    { key: 'calibrationDate', header: 'Calibrated', render: (r) => formatDate(r.calibrationDate) },
    { key: 'certificateReference', header: 'Certificate ref.' },
    { key: 'agencyName', header: 'Agency' },
    { key: 'result', header: 'Result', render: (r) => <StatusBadge value={r.result} /> },
    { key: 'dueDate', header: 'Next due', render: (r) => formatDate(r.dueDate) },
    { key: 'actionTaken', header: 'Action taken' }
  ];

  return (
    <ResourcePage
      title="Calibration results"
      subtitle="Record outcomes; the next calibration is planned automatically when a result is OK"
      entityName="Calibration result"
      createLabel="Record calibration"
      api={resultApi}
      idKey="resultId"
      columns={columns}
      filters={[
        { name: 'search', type: 'search', label: 'Search', placeholder: 'Code, name, certificate…' },
        { name: 'result', type: 'select', label: 'Result', options: RESULTS },
        { name: 'agencyId', type: 'select', label: 'Agency', options: toOptions(lookups.agencies) },
        { name: 'from', type: 'date', label: 'Calibrated from' },
        { name: 'to', type: 'date', label: 'Calibrated to' }
      ]}
      initialFilters={{ search: '', result: '', agencyId: '', from: '', to: '' }}
      fields={fields}
      emptyForm={{ result: 'OK' }}
      initialAction={initialAction}
      onInitialActionHandled={() => navigate(location.pathname, { replace: true, state: null })}
      buildCreate={(payload, values) => ({ ...payload, scheduleId: values.scheduleId ? Number(values.scheduleId) : null })}
      toForm={(r) => ({
        calibrationDate: toInputDate(r.calibrationDate), dueDate: toInputDate(r.dueDate), certificateReference: r.certificateReference ?? '', agencyId: r.agencyId ?? '',
        result: r.result, signedBy: r.signedBy ?? '', errorDetails: r.errorDetails ?? '', actionTaken: r.actionTaken ?? '', remarks: r.remarks ?? ''
      })}
      deleteMessage={(r) => `Delete the ${formatDate(r.calibrationDate)} calibration of ${r.equipmentCode}? If it completed a plan entry, that entry is re-opened.`}
    />
  );
}
