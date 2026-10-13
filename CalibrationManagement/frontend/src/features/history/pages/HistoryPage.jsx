import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import EquipmentPicker from '../../../components/ui/EquipmentPicker';
import PageHeader from '../../../components/ui/PageHeader';
import { EmptyState, ErrorState, LoadingState } from '../../../components/ui/States';
import StatusBadge from '../../../components/ui/StatusBadge';
import { useAuth } from '../../../auth/useAuth';
import { useApi } from '../../../hooks/useApi';
import { historyApi } from '../../../api/index';
import { formatDate } from '../../../utils/format';

function Field({ label, children }) {
  return <div className="card-field"><span>{label}</span><strong>{children || '—'}</strong></div>;
}

export default function HistoryPage() {
  const { equipmentId } = useParams();
  const navigate = useNavigate();
  const { can } = useAuth();
  const [pickerLabel, setPickerLabel] = useState('');
  const { data, loading, error, reload } = useApi((signal) => historyApi.get(equipmentId, signal), [equipmentId], { enabled: Boolean(equipmentId) });

  const eq = data?.equipment;

  return (
    <>
      <PageHeader
        title="Equipment history card"
        subtitle="Calibration history per instrument (QA.FMT-29)"
        actions={eq ? (
          <>
            <Button icon="print" onClick={() => window.print()}>Print</Button>
            {can('write') ? (
              <Button variant="primary" icon="plus" onClick={() => navigate('/results', { state: { schedule: { equipmentId: eq.equipmentId, label: `${eq.equipmentCode} – ${eq.equipmentName}` } } })}>Record calibration</Button>
            ) : null}
          </>
        ) : null}
      />

      <Card className="no-print">
        <div className="form-field">
          <label htmlFor="history-picker">Select equipment</label>
          <EquipmentPicker
            id="history-picker"
            value={equipmentId ? Number(equipmentId) : ''}
            label={pickerLabel || (eq ? `${eq.equipmentCode} – ${eq.equipmentName}` : '')}
            onChange={(id, label) => { setPickerLabel(label); if (id) navigate(`/history/${id}`); }}
          />
        </div>
      </Card>

      {!equipmentId ? <Card><EmptyState title="Choose an instrument" message="Search for an instrument above to open its history card." /></Card> : null}
      {equipmentId && loading && !data ? <LoadingState label="Loading history card…" /> : null}
      {equipmentId && error && !data ? <ErrorState error={error} onRetry={reload} /> : null}

      {eq ? (
        <Card className="history-card" aria-label="Equipment history card">
          <div className="history-title">
            <h2>EQUIPMENT HISTORY CARD</h2>
            <span className="muted small">QA.FMT-29</span>
          </div>
          <div className="card-fields">
            <Field label="Equipment name">{eq.equipmentName}</Field>
            <Field label="Code no.">{eq.equipmentCode}</Field>
            <Field label="Make">{eq.make}</Field>
            <Field label="Serial no.">{eq.serialNo}</Field>
            <Field label="Range & least count">{eq.rangeLeastCount}</Field>
            <Field label="Deviation allowed">{eq.acceptableErrorLimit}</Field>
            <Field label="Calibration at">{eq.calibrationType}{eq.agencyName ? ` – ${eq.agencyName}` : ''}</Field>
            <Field label="Frequency">{eq.frequencyLabel}</Field>
            <Field label="Location">{eq.location || eq.departmentName}</Field>
            <Field label="Responsible">{eq.responsiblePerson}</Field>
            <Field label="Status"><StatusBadge value={eq.calibrationStatus} /></Field>
            <Field label="Next due">{formatDate(eq.nextDueDate)}</Field>
          </div>

          <div className="table-wrap">
            <table className="table history-table">
              <caption className="sr-only">Calibration history</caption>
              <thead>
                <tr>
                  <th rowSpan={2}>Sl. no.</th><th rowSpan={2}>Calibration date</th><th rowSpan={2}>Certificate ref.</th><th rowSpan={2}>Agency</th>
                  <th rowSpan={2}>Error details</th><th colSpan={2}>Result</th><th rowSpan={2}>If not OK, action taken</th><th rowSpan={2}>Due date</th><th rowSpan={2}>Remarks</th><th rowSpan={2}>Sign</th>
                </tr>
                <tr><th>OK</th><th>Not OK</th></tr>
              </thead>
              <tbody>
                {data.rows.length === 0 ? (
                  <tr><td colSpan={11} className="muted center">No calibrations have been recorded for this instrument yet.</td></tr>
                ) : data.rows.map((r) => (
                  <tr key={r.resultId}>
                    <td>{r.slNo}</td><td>{formatDate(r.calibrationDate)}</td><td>{r.certificateReference || '—'}</td><td>{r.agencyName || '—'}</td>
                    <td>{r.errorDetails || '—'}</td>
                    <td className="center">{r.result === 'OK' ? '✓' : r.result === 'Unverified' ? '?' : ''}</td>
                    <td className="center">{r.result === 'NOTOK' ? '✓' : ''}</td>
                    <td>{r.actionTaken || '—'}</td><td>{formatDate(r.dueDate)}</td><td>{r.remarks || '—'}</td><td>{r.signedBy || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <p className="muted small">"?" marks an imported calibration whose pass/fail result was not recorded in the source Excel plan.</p>
        </Card>
      ) : null}
    </>
  );
}
