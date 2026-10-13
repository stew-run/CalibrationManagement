import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { cardVariants, spring, staggerContainer } from '../utils/motion';
import Card from '../components/ui/Card';
import PageHeader from '../components/ui/PageHeader';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import StatusBadge from '../components/ui/StatusBadge';
import Button from '../components/ui/Button';
import { useApi } from '../hooks/useApi';
import { dashboardApi } from '../api/index';
import { formatDate, formatNumber } from '../utils/format';

function Metric({ label, value, tone = 'neutral', to, hint }) {
  const body = (
    <motion.div variants={cardVariants} className={`metric metric-${tone}`} whileHover={{ y: -3 }} transition={spring.snappy}>
      <span className="metric-label">{label}</span>
      <strong className="metric-value">{formatNumber(value)}</strong>
      {hint ? <span className="metric-hint">{hint}</span> : null}
    </motion.div>
  );
  return to ? <Link to={to} className="metric-link">{body}</Link> : body;
}

function monthLabel(key) {
  const [y, m] = key.split('-').map(Number);
  return new Date(y, m - 1, 1).toLocaleDateString(undefined, { month: 'short' });
}

export default function DashboardPage() {
  const { data, loading, error, reload } = useApi((signal) => dashboardApi.get(signal), []);

  if (loading && !data) return <LoadingState label="Loading dashboard…" />;
  if (error && !data) return <ErrorState error={error} onRetry={reload} />;
  const { metrics, byDepartment, dueByMonth, recentResults, upcoming } = data;
  const maxMonth = Math.max(1, ...dueByMonth.map((m) => m.dueCount));
  const maxDept = Math.max(1, ...byDepartment.map((d) => d.total));

  return (
    <>
      <PageHeader title="Dashboard" subtitle="Live status of the equipment register" actions={<Button icon="refresh" onClick={reload} loading={loading}>Refresh</Button>} />

      <motion.div className="metric-grid" variants={staggerContainer} initial="initial" animate="animate">
        <Metric label="Active equipment" value={metrics.totalEquipment} to="/equipment" />
        <Metric label="Calibration valid" value={metrics.validCount} tone="success" to="/equipment" />
        <Metric label="Due within 30 days" value={metrics.dueSoonCount} tone="warning" to="/reports" />
        <Metric label="Overdue" value={metrics.overdueCount} tone="danger" to="/reports" />
        <Metric label="Failed calibration" value={metrics.failedCount} tone="danger" to="/results" />
        <Metric label="Not calibrated" value={metrics.notCalibratedCount} to="/reports" />
        <Metric label="Planned this month" value={metrics.plannedThisMonth} tone="info" to="/schedules" />
        <Metric label="Open maintenance" value={metrics.openMaintenance} to="/maintenance" />
        <Metric label="Certificates expiring ≤30d" value={metrics.expiringCertificates} tone="warning" to="/certificates" />
        <Metric label="Unread notifications" value={metrics.unreadNotifications} tone="info" to="/notifications" />
      </motion.div>

      <div className="grid-2">
        <Card title="Calibrations falling due – next 12 months" animated>
          <div className="bar-chart" role="img" aria-label="Number of calibrations due per month for the next twelve months">
            {dueByMonth.map((m, i) => (
              <div key={m.monthKey} className="bar-col" title={`${m.monthKey}: ${m.dueCount}`}>
                <span className="bar-value">{m.dueCount}</span>
                <motion.div className="bar" initial={{ scaleY: 0 }} animate={{ scaleY: 1 }} transition={{ ...spring.gentle, delay: i * 0.03 }} style={{ height: `${(m.dueCount / maxMonth) * 100}%`, originY: 1 }} />
                <span className="bar-label">{monthLabel(m.monthKey)}</span>
              </div>
            ))}
          </div>
        </Card>

        <Card title="By department (most calibrations due)" animated>
          {byDepartment.length === 0 ? <EmptyState title="No equipment yet" /> : (
            <ul className="hbars">
              {byDepartment.map((d, i) => (
                <li key={d.departmentName}>
                  <div className="hbar-head"><span>{d.departmentName}</span><span className="muted small">{d.overdue} overdue · {d.dueSoon} due soon · {d.total} total</span></div>
                  <div className="hbar-track">
                    <motion.div className="hbar hbar-total" initial={{ width: 0 }} animate={{ width: `${(d.total / maxDept) * 100}%` }} transition={{ ...spring.gentle, delay: i * 0.04 }} />
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </div>

      <div className="grid-2">
        <Card title="Needs attention" animated actions={<Link to="/reports" className="link">Open reports</Link>}>
          {upcoming.length === 0 ? <EmptyState title="Nothing due" message="No equipment is due, overdue or failed." /> : (
            <ul className="list">
              {upcoming.map((u) => (
                <li key={u.equipmentId}>
                  <Link to={`/history/${u.equipmentId}`}><strong>{u.equipmentCode}</strong> – {u.equipmentName}</Link>
                  <span className="muted small">{u.departmentName || ''} · {formatDate(u.nextDueDate)}</span>
                  <StatusBadge value={u.calibrationStatus} />
                </li>
              ))}
            </ul>
          )}
        </Card>

        <Card title="Recent calibrations" animated actions={<Link to="/results" className="link">All results</Link>}>
          {recentResults.length === 0 ? <EmptyState title="No calibration results yet" /> : (
            <ul className="list">
              {recentResults.map((r) => (
                <li key={r.resultId}>
                  <Link to={`/history/${r.equipmentId}`}><strong>{r.equipmentCode}</strong> – {r.equipmentName}</Link>
                  <span className="muted small">{formatDate(r.calibrationDate)}</span>
                  <StatusBadge value={r.result} />
                </li>
              ))}
            </ul>
          )}
        </Card>
      </div>
    </>
  );
}
