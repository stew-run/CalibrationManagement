import { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import { listItemVariants } from '../../../utils/motion';
import { NOTIFICATIONS_CHANGED } from '../../../layouts/Topbar';
import Button from '../../../components/ui/Button';
import Card from '../../../components/ui/Card';
import PageHeader from '../../../components/ui/PageHeader';
import Pagination from '../../../components/ui/Pagination';
import { EmptyState, ErrorState, LoadingState } from '../../../components/ui/States';
import { useToast } from '../../../contexts/ToastContext';
import { useListQuery } from '../../../hooks/useListQuery';
import { notificationApi } from '../../../api/index';
import { formatDateTime } from '../../../utils/format';

const TONE = { Overdue: 'danger', Failed: 'danger', DueSoon: 'warning', Maintenance: 'info', System: 'info' };

export default function NotificationsPage() {
  const toast = useToast();
  const list = useListQuery(notificationApi.list, { unreadOnly: false }, { pageSize: 20 });
  const [busy, setBusy] = useState(false);
  const items = list.data?.items ?? [];

  const changed = () => { window.dispatchEvent(new Event(NOTIFICATIONS_CHANGED)); list.reload(); };

  const run = async (fn, success) => {
    setBusy(true);
    try {
      await fn();
      if (success) toast.success(success);
      changed();
    } catch (e) {
      toast.error(e.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <>
      <PageHeader
        title="Notifications"
        subtitle="Calibrations that are due soon, overdue or failed"
        actions={(
          <>
            <Button icon="refresh" loading={busy} onClick={() => run(async () => { const r = await notificationApi.refresh(); toast.info(r.created ? `${r.created} new notification(s).` : 'Already up to date.'); })}>Check now</Button>
            <Button icon="check" disabled={busy} onClick={() => run(() => notificationApi.markAllRead(), 'All notifications marked as read.')}>Mark all read</Button>
          </>
        )}
      />
      <Card>
        <div className="filters">
          <label className="checkbox"><input type="checkbox" checked={list.filters.unreadOnly} onChange={(e) => list.setFilter('unreadOnly', e.target.checked)} /><span>Unread only</span></label>
        </div>

        {list.loading && items.length === 0 ? <LoadingState /> : null}
        {list.error && items.length === 0 ? <ErrorState error={list.error} onRetry={list.reload} /> : null}
        {!list.loading && !list.error && items.length === 0 ? <EmptyState title="No notifications" message="You are all caught up." /> : null}

        <ul className="notice-list">
          <AnimatePresence initial={false}>
            {items.map((n) => (
              <motion.li key={n.notificationId} layout variants={listItemVariants} initial="initial" animate="animate" exit="exit" className={`notice ${n.isRead ? '' : 'unread'}`}>
                <span className={`dot dot-${TONE[n.notificationType] || 'info'}`} aria-hidden="true" />
                <div className="notice-body">
                  <strong>{n.title}</strong>
                  <p>{n.message}</p>
                  <small className="muted">{formatDateTime(n.createdAt)}{n.equipmentId ? <> · <Link to={`/history/${n.equipmentId}`} className="link">Open history card</Link></> : null}</small>
                </div>
                {!n.isRead ? <Button size="sm" onClick={() => run(() => notificationApi.markRead(n.notificationId))}>Mark read</Button> : <span className="muted small">Read</span>}
              </motion.li>
            ))}
          </AnimatePresence>
        </ul>
        {list.data ? <Pagination page={list.page} pageSize={list.pageSize} totalCount={list.data.totalCount} totalPages={list.data.totalPages} onPageChange={list.setPage} /> : null}
      </Card>
    </>
  );
}
