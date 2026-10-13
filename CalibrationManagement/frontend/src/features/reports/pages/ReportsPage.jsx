import { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { spring } from '../../../utils/motion';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import DataTable from '../../../components/ui/DataTable';
import PageHeader from '../../../components/ui/PageHeader';
import Pagination from '../../../components/ui/Pagination';
import StatusBadge from '../../../components/ui/StatusBadge';
import { useAuth } from '../../../auth/useAuth';
import { useLookups } from '../../../contexts/LookupsContext';
import { useToast } from '../../../contexts/ToastContext';
import { useApi } from '../../../hooks/useApi';
import { useListQuery } from '../../../hooks/useListQuery';
import { reportApi } from '../../../api/index';
import { formatDate } from '../../../utils/format';

const TABS = [
  { value: 'all', label: 'All equipment' },
  { value: 'due', label: 'Due & overdue' },
  { value: 'overdue', label: 'Overdue' },
  { value: 'failed', label: 'Failed' },
  { value: 'notcalibrated', label: 'Not calibrated' }
];

function downloadBlob(blob, filename) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

function DataQualitySection() {
  const toast = useToast();
  const [onlyOpen, setOnlyOpen] = useState(true);
  const { data, loading, error, reload } = useApi((signal) => reportApi.dataQuality({ onlyOpen }, signal), [onlyOpen]);

  const toggle = async (issue) => {
    try {
      await reportApi.resolveIssue(issue.issueId, !issue.isResolved);
      reload();
    } catch (e) {
      toast.error(e.message);
    }
  };

  const columns = [
    { key: 'severity', header: 'Severity', render: (r) => <span className={`badge badge-${r.severity === 'Error' ? 'danger' : r.severity === 'Warning' ? 'warning' : 'info'}`}>{r.severity}</span> },
    { key: 'issueType', header: 'Type' },
    { key: 'equipmentCode', header: 'Code' },
    { key: 'sourceRow', header: 'Source row', render: (r) => r.sourceRow ?? '—' },
    { key: 'details', header: 'Details', className: 'wrap' },
    { key: 'isResolved', header: 'Resolved', render: (r) => <Button size="sm" onClick={() => toggle(r)}>{r.isResolved ? 'Reopen' : 'Mark resolved'}</Button> }
  ];

  return (
    <Card title="Data quality (Excel import findings)" actions={<label className="checkbox"><input type="checkbox" checked={onlyOpen} onChange={(e) => setOnlyOpen(e.target.checked)} /><span>Open only</span></label>}>
      <DataTable columns={columns} rows={data ?? []} rowKey={(r) => r.issueId} loading={loading} error={error} onRetry={reload} emptyTitle="No data quality findings" emptyMessage="Nothing to review." caption="Data quality findings" />
    </Card>
  );
}

export default function ReportsPage() {
  const { can } = useAuth();
  const toast = useToast();
  const lookups = useLookups();
  const [exporting, setExporting] = useState(false);
  const list = useListQuery(reportApi.calibration, { reportType: 'all', search: '', departmentId: '', agencyId: '', from: '', to: '' });
  const agencies = useApi((signal) => reportApi.agencySummary({ from: list.filters.from, to: list.filters.to }, signal), [list.filters.from, list.filters.to]);

  const exportCsv = async () => {
    setExporting(true);
    try {
      const { blob, filename } = await reportApi.exportCalibration(list.filters);
      downloadBlob(blob, filename);
      toast.success('Report exported.');
    } catch (e) {
      toast.error(e.message);
    } finally {
      setExporting(false);
    }
  };

  const columns = [
    { key: 'equipmentCode', header: 'Code', render: (r) => <strong>{r.equipmentCode}</strong> },
    { key: 'equipmentName', header: 'Instrument' },
    { key: 'departmentName', header: 'Department' },
    { key: 'agencyName', header: 'Agency' },
    { key: 'lastCalibrationDate', header: 'Last calibration', render: (r) => formatDate(r.lastCalibrationDate) },
    { key: 'nextDueDate', header: 'Next due', render: (r) => formatDate(r.nextDueDate) },
    { key: 'daysToDue', header: 'Days', align: 'right', render: (r) => (r.daysToDue === null || r.daysToDue === undefined ? '—' : r.daysToDue) },
    { key: 'calibrationStatus', header: 'Status', render: (r) => <StatusBadge value={r.calibrationStatus} /> }
  ];

  const agencyColumns = [
    { key: 'agencyName', header: 'Agency' },
    { key: 'totalCalibrations', header: 'Calibrations', align: 'right' },
    { key: 'okCount', header: 'OK', align: 'right' },
    { key: 'notOkCount', header: 'Not OK', align: 'right' },
    { key: 'unverifiedCount', header: 'Unverified', align: 'right' },
    { key: 'lastCalibration', header: 'Latest', render: (r) => formatDate(r.lastCalibration) }
  ];

  return (
    <>
      <PageHeader title="Reports" subtitle="Filter, review and export calibration status" actions={<Button icon="download" onClick={exportCsv} loading={exporting}>Export CSV</Button>} />

      <div className="tabs" role="tablist" aria-label="Report type">
        {TABS.map((t) => (
          <button key={t.value} type="button" role="tab" aria-selected={list.filters.reportType === t.value} className={`tab ${list.filters.reportType === t.value ? 'active' : ''}`} onClick={() => list.setFilter('reportType', t.value)}>
            {list.filters.reportType === t.value ? <motion.span layoutId="tab-pill" className="tab-pill" transition={spring.snappy} /> : null}
            <span className="tab-label">{t.label}</span>
          </button>
        ))}
      </div>

      <Card className="filters-card">
        <form className="filters" onSubmit={(e) => e.preventDefault()} role="search" aria-label="Report filters">
          <div className="filter filter-grow"><label htmlFor="r-search">Search</label><input id="r-search" className="input" type="search" placeholder="Code, name, responsible…" value={list.filters.search} onChange={(e) => list.setFilter('search', e.target.value)} /></div>
          <div className="filter"><label htmlFor="r-dept">Department</label>
            <select id="r-dept" className="input" value={list.filters.departmentId} onChange={(e) => list.setFilter('departmentId', e.target.value)}><option value="">All</option>{lookups.departments.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}</select></div>
          <div className="filter"><label htmlFor="r-agency">Agency</label>
            <select id="r-agency" className="input" value={list.filters.agencyId} onChange={(e) => list.setFilter('agencyId', e.target.value)}><option value="">All</option>{lookups.agencies.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}</select></div>
          <div className="filter"><label htmlFor="r-from">Due from</label><input id="r-from" className="input" type="date" value={list.filters.from} onChange={(e) => list.setFilter('from', e.target.value)} /></div>
          <div className="filter"><label htmlFor="r-to">Due to</label><input id="r-to" className="input" type="date" value={list.filters.to} onChange={(e) => list.setFilter('to', e.target.value)} /></div>
          <Button size="sm" icon="refresh" onClick={list.resetFilters}>Reset</Button>
        </form>
      </Card>

      <Card className="table-card">
        <DataTable columns={columns} rows={list.data?.items ?? []} rowKey={(r) => r.equipmentId} loading={list.loading} error={list.error} onRetry={list.reload} emptyTitle="No equipment matches this report" emptyMessage="Change the report type or filters." caption="Calibration report" />
        {list.data ? <Pagination page={list.page} pageSize={list.pageSize} totalCount={list.data.totalCount} totalPages={list.data.totalPages} onPageChange={list.setPage} /> : null}
      </Card>

      <Card title="Calibrations by agency">
        <DataTable columns={agencyColumns} rows={agencies.data ?? []} rowKey={(r) => r.agencyName} loading={agencies.loading} error={agencies.error} onRetry={agencies.reload} emptyTitle="No calibration results in this period" caption="Calibrations by agency" />
      </Card>

      <AnimatePresence>{can('manage') ? <DataQualitySection key="dq" /> : null}</AnimatePresence>
    </>
  );
}
