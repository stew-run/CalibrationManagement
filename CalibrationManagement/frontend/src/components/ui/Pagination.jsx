import Button from './Button';

export default function Pagination({ page, pageSize, totalCount, totalPages, onPageChange }) {
  if (!totalCount) return null;
  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);
  return (
    <nav className="pagination" aria-label="Pagination">
      <span className="muted small">Showing {from}–{to} of {totalCount}</span>
      <div className="pagination-buttons">
        <Button size="sm" icon="chevronLeft" onClick={() => onPageChange(page - 1)} disabled={page <= 1} aria-label="Previous page" />
        <span className="pagination-page" aria-current="page">Page {page} of {Math.max(totalPages, 1)}</span>
        <Button size="sm" icon="chevronRight" onClick={() => onPageChange(page + 1)} disabled={page >= totalPages} aria-label="Next page" />
      </div>
    </nav>
  );
}
