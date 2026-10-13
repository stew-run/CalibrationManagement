import { useAuth } from './useAuth';

/** Renders children only when the signed-in user holds `permission` (read | write | manage | admin). */
export default function RoleGuard({ permission, children }) {
  const { can } = useAuth();
  if (permission && !can(permission)) {
    return (
      <div className="state state-error" role="alert">
        <h3>Access denied</h3>
        <p>Your role does not allow access to this page.</p>
      </div>
    );
  }
  return children;
}
