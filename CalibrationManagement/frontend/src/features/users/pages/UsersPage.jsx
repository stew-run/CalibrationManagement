import { useState } from 'react';
import ResourcePage from '../../../components/ResourcePage';
import FormModal from '../../../components/ui/FormModal';
import StatusBadge from '../../../components/ui/StatusBadge';
import { useAuth } from '../../../auth/useAuth';
import { useToast } from '../../../contexts/ToastContext';
import { userApi } from '../../../api/index';
import { formatDateTime } from '../../../utils/format';

const PASSWORD_HELP = 'At least 12 characters with upper-case, lower-case and a digit.';
const api = { search: userApi.list, create: userApi.create, update: userApi.update, remove: () => Promise.reject(new Error('Users are deactivated, not deleted.')) };

const fields = (lookups, mode) => [
  ...(mode === 'edit' ? [] : [{ name: 'username', label: 'Username', required: true, maxLength: 50, autoComplete: 'off', help: 'Letters, digits, ".", "_" or "-" (3-50 characters).' }]),
  { name: 'fullName', label: 'Full name', required: true, maxLength: 150 },
  { name: 'email', label: 'Email', type: 'email', required: true, maxLength: 256 },
  { name: 'roleId', label: 'Role', type: 'select', valueType: 'int', allowEmpty: false, required: true, options: lookups.roles.map((r) => ({ value: r.id, label: r.name })) },
  ...(mode === 'edit'
    ? [{ name: 'isActive', label: 'Active', type: 'checkbox', checkboxLabel: 'Account is active (can sign in)', span: 2 }]
    : [{ name: 'password', label: 'Initial password', type: 'password', required: true, maxLength: 128, autoComplete: 'new-password', span: 2, help: PASSWORD_HELP }])
];

export default function UsersPage() {
  const toast = useToast();
  const { user: me } = useAuth();
  const [resetFor, setResetFor] = useState(null);

  const columns = [
    { key: 'username', header: 'Username', render: (r) => <strong>{r.username}</strong> },
    { key: 'fullName', header: 'Full name' },
    { key: 'email', header: 'Email' },
    { key: 'role', header: 'Role' },
    { key: 'isActive', header: 'Status', render: (r) => <StatusBadge value={r.isActive ? 'Active' : 'Inactive'} /> },
    { key: 'lastLoginAt', header: 'Last sign-in', render: (r) => formatDateTime(r.lastLoginAt) }
  ];

  return (
    <>
      <ResourcePage
        title="Users & roles"
        subtitle="Administrators create accounts and assign roles. There is no public registration."
        entityName="User"
        api={api}
        idKey="userId"
        columns={columns}
        filters={[{ name: 'search', type: 'search', label: 'Search', placeholder: 'Username, name or email…' }]}
        initialFilters={{ search: '' }}
        fields={fields}
        emptyForm={{ roleId: 4 }}
        permissions={{ create: 'admin', edit: 'admin', delete: null }}
        toForm={(r) => ({ fullName: r.fullName, email: r.email, roleId: r.roleId, isActive: r.isActive })}
        rowActions={(r) => [{ label: 'Reset password', icon: 'user', onClick: () => setResetFor(r) }]}
        formSize="md"
        editNotice={<p className="muted small">Role and status changes apply to new sign-ins; an active session keeps its role until its token expires (about 30 minutes).</p>}
      />
      <FormModal
        open={Boolean(resetFor)}
        title={resetFor ? `Reset password – ${resetFor.username}` : ''}
        fields={[{ name: 'newPassword', label: 'New password', type: 'password', required: true, maxLength: 128, autoComplete: 'new-password', help: PASSWORD_HELP }]}
        initialValues={{ newPassword: '' }}
        size="sm"
        submitLabel="Reset password"
        onClose={() => setResetFor(null)}
        onSubmit={async (payload) => {
          await userApi.resetPassword(resetFor.userId, payload.newPassword);
          toast.success(resetFor.userId === me?.userId ? 'Your password was changed.' : 'Password reset. Share it with the user securely.');
        }}
      />
    </>
  );
}
