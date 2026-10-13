import { AnimatePresence, motion } from 'framer-motion';
import { rowVariants } from '../../utils/motion';
import { EmptyState, ErrorState, LoadingState } from './States';

/** columns: [{ key, header, render?(row), className?, align? }] */
export default function DataTable({ columns, rows, rowKey, loading, error, onRetry, emptyTitle, emptyMessage, emptyAction, caption }) {
  if (error && (!rows || rows.length === 0)) return <ErrorState error={error} onRetry={onRetry} />;
  if (loading && (!rows || rows.length === 0)) return <LoadingState />;
  if (!rows || rows.length === 0) return <EmptyState title={emptyTitle} message={emptyMessage} action={emptyAction} />;

  return (
    <div className={`table-wrap ${loading ? 'is-refreshing' : ''}`} aria-busy={loading || undefined}>
      {error ? <p className="form-error" role="alert">{error.message}</p> : null}
      <table className="table">
        {caption ? <caption className="sr-only">{caption}</caption> : null}
        <thead>
          <tr>
            {columns.map((c) => (
              <th key={c.key} scope="col" className={c.className} style={{ textAlign: c.align }}>{c.header}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          <AnimatePresence initial={false}>
            {rows.map((row) => (
              <motion.tr key={rowKey(row)} variants={rowVariants} initial="initial" animate="animate" exit="exit">
                {columns.map((c) => (
                  <td key={c.key} className={c.className} style={{ textAlign: c.align }} data-label={typeof c.header === 'string' ? c.header : undefined}>
                    {c.render ? c.render(row) : row[c.key] ?? '—'}
                  </td>
                ))}
              </motion.tr>
            ))}
          </AnimatePresence>
        </tbody>
      </table>
    </div>
  );
}
