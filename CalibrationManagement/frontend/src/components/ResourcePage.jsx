import { useEffect, useMemo, useState } from 'react';
import { motion } from 'framer-motion';
import { useAuth } from '../auth/useAuth';
import { useLookups } from '../contexts/LookupsContext';
import { useToast } from '../contexts/ToastContext';
import { useListQuery } from '../hooks/useListQuery';
import Button from './ui/Button';
import Card from './ui/Card';
import ConfirmDialog from './ui/ConfirmDialog';
import DataTable from './ui/DataTable';
import FormModal from './ui/FormModal';
import Icon from './ui/Icon';
import PageHeader from './ui/PageHeader';
import Pagination from './ui/Pagination';

function FilterControl({ filter, value, onChange }) {
  const id = `filter-${filter.name}`;
  if (filter.type === 'select') {
    return (
      <div className="filter">
        <label htmlFor={id}>{filter.label}</label>
        <select id={id} className="input" value={value ?? ''} onChange={(e) => onChange(e.target.value)}>
          <option value="">{filter.allLabel || 'All'}</option>
          {filter.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>
      </div>
    );
  }
  if (filter.type === 'checkbox') {
    return (
      <div className="filter filter-check">
        <label className="checkbox"><input id={id} type="checkbox" checked={Boolean(value)} onChange={(e) => onChange(e.target.checked)} /><span>{filter.label}</span></label>
      </div>
    );
  }
  return (
    <div className={`filter ${filter.type === 'search' ? 'filter-grow' : ''}`}>
      <label htmlFor={id}>{filter.label}</label>
      <div className={filter.type === 'search' ? 'input-icon' : ''}>
        {filter.type === 'search' ? <Icon name="search" size={16} /> : null}
        <input id={id} className="input" type={filter.type === 'search' ? 'search' : filter.type} placeholder={filter.placeholder} value={value ?? ''} onChange={(e) => onChange(e.target.value)} />
      </div>
    </div>
  );
}

/**
 * Generic list + create/edit/delete page driven by configuration.
 * Every operation goes through the supplied API functions (real backend endpoints).
 */
export default function ResourcePage({
  title, subtitle, entityName, api, idKey, columns, filters = [], initialFilters = {}, fields, toForm, emptyForm = {},
  buildCreate, buildUpdate, rowActions, permissions = {}, headerActions, initialAction, onInitialActionHandled,
  deleteMessage, createLabel, canDeleteRow, canEditRow, emptyTitle, emptyMessage, pageSize = 25, formSize = 'lg', editNotice
}) {
  const { can } = useAuth();
  const toast = useToast();
  const lookups = useLookups();
  const perms = { create: 'write', edit: 'write', delete: 'manage', ...permissions };
  const list = useListQuery(api.search, initialFilters, { pageSize });
  const [form, setForm] = useState(null);
  const [toDelete, setToDelete] = useState(null);

  const resolvedFields = useMemo(() => (typeof fields === 'function' ? fields(lookups, form?.mode) : fields), [fields, lookups, form?.mode]);

  useEffect(() => {
    if (initialAction) {
      setForm({ mode: 'create', values: { ...emptyForm, ...initialAction } });
      onInitialActionHandled?.();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialAction]);

  const items = list.data?.items ?? [];
  const showCreate = perms.create && can(perms.create);
  const showEdit = (row) => perms.edit && can(perms.edit) && (!canEditRow || canEditRow(row));
  const showDelete = (row) => perms.delete && can(perms.delete) && (!canDeleteRow || canDeleteRow(row));

  const allColumns = [
    ...columns,
    {
      key: '__actions',
      header: <span className="sr-only">Actions</span>,
      className: 'col-actions',
      render: (row) => (
        <div className="row-actions">
          {rowActions?.(row, { reload: list.reload, openEdit: () => setForm({ mode: 'edit', row, values: toForm(row) }) }).map((a) => (
            <button key={a.label} type="button" className="icon-btn" title={a.label} aria-label={`${a.label}: ${row[idKey]}`} onClick={a.onClick}><Icon name={a.icon} /></button>
          ))}
          {showEdit(row) ? (
            <button type="button" className="icon-btn" title="Edit" aria-label="Edit" onClick={() => setForm({ mode: 'edit', row, values: toForm(row) })}><Icon name="edit" /></button>
          ) : null}
          {showDelete(row) ? (
            <button type="button" className="icon-btn danger" title="Delete" aria-label="Delete" onClick={() => setToDelete(row)}><Icon name="trash" /></button>
          ) : null}
        </div>
      )
    }
  ];

  const submit = async (payload, values) => {
    if (form.mode === 'create') {
      await api.create(buildCreate ? buildCreate(payload, values) : payload);
      toast.success(`${entityName} created.`);
    } else {
      await api.update(form.row[idKey], buildUpdate ? buildUpdate(payload, values, form.row) : payload);
      toast.success(`${entityName} updated.`);
    }
    list.reload();
  };

  return (
    <>
      <PageHeader
        title={title}
        subtitle={subtitle}
        actions={(
          <>
            {headerActions}
            {showCreate ? <Button variant="primary" icon="plus" onClick={() => setForm({ mode: 'create', values: emptyForm })}>{createLabel || `Add ${entityName.toLowerCase()}`}</Button> : null}
          </>
        )}
      />

      <Card className="filters-card">
        <form className="filters" onSubmit={(e) => e.preventDefault()} role="search" aria-label={`Filter ${title}`}>
          {filters.map((f) => (
            <FilterControl key={f.name} filter={f} value={list.filters[f.name]} onChange={(v) => list.setFilter(f.name, v)} />
          ))}
          <Button size="sm" icon="refresh" onClick={list.resetFilters}>Reset</Button>
        </form>
      </Card>

      <Card className="table-card">
        <DataTable
          columns={allColumns}
          rows={items}
          rowKey={(r) => r[idKey]}
          loading={list.loading}
          error={list.error}
          onRetry={list.reload}
          emptyTitle={emptyTitle || `No ${entityName.toLowerCase()} records found`}
          emptyMessage={emptyMessage || 'Adjust the filters or add a new record.'}
          caption={title}
        />
        {list.data ? (
          <Pagination page={list.page} pageSize={list.pageSize} totalCount={list.data.totalCount} totalPages={list.data.totalPages} onPageChange={list.setPage} />
        ) : null}
      </Card>

      <FormModal
        open={Boolean(form)}
        title={form?.mode === 'edit' ? `Edit ${entityName.toLowerCase()}` : `New ${entityName.toLowerCase()}`}
        fields={resolvedFields}
        initialValues={form?.values}
        onSubmit={submit}
        onClose={() => setForm(null)}
        submitLabel={form?.mode === 'edit' ? 'Save changes' : 'Create'}
        size={formSize}
        notice={form?.mode === 'edit' ? editNotice : null}
      />

      <ConfirmDialog
        open={Boolean(toDelete)}
        title={`Delete ${entityName.toLowerCase()}`}
        message={toDelete ? (deleteMessage ? deleteMessage(toDelete) : `Delete this ${entityName.toLowerCase()}? This cannot be undone.`) : ''}
        onClose={() => setToDelete(null)}
        onConfirm={async () => {
          await api.remove(toDelete[idKey]);
          toast.success(`${entityName} deleted.`);
          list.reload();
        }}
      />
    </>
  );
}

export function SectionNote({ children }) {
  return <motion.p className="muted small" initial={{ opacity: 0 }} animate={{ opacity: 1 }}>{children}</motion.p>;
}
