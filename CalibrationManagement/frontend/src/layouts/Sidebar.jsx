import { AnimatePresence, motion } from 'framer-motion';
import { NavLink } from 'react-router-dom';
import { fadeVariants, spring } from '../utils/motion';
import { useAuth } from '../auth/useAuth';
import Icon from '../components/ui/Icon';

const NAV = [
  { to: '/', label: 'Dashboard', icon: 'dashboard', end: true },
  { to: '/equipment', label: 'Equipment', icon: 'equipment' },
  { to: '/schedules', label: 'Calibration plan', icon: 'calendar' },
  { to: '/results', label: 'Calibration results', icon: 'check' },
  { to: '/history', label: 'History cards', icon: 'history' },
  { to: '/certificates', label: 'Certificates', icon: 'certificate' },
  { to: '/maintenance', label: 'Maintenance', icon: 'wrench' },
  { to: '/reports', label: 'Reports', icon: 'report' },
  { to: '/notifications', label: 'Notifications', icon: 'bell' },
  { to: '/users', label: 'Users & roles', icon: 'users', permission: 'admin' }
];

export default function Sidebar({ collapsed, mobile, open, onClose, onToggleCollapse }) {
  const { can } = useAuth();
  const items = NAV.filter((item) => !item.permission || can(item.permission));
  const showLabels = mobile || !collapsed;

  const content = (
    <>
      <div className="sidebar-brand">
        <span className="brand-mark" aria-hidden="true"><Icon name="check" size={18} /></span>
        <AnimatePresence initial={false}>
          {showLabels && (
            <motion.span key="brand" className="brand-text" variants={fadeVariants} initial="initial" animate="animate" exit="exit">
              Calibration<br /><small>Management</small>
            </motion.span>
          )}
        </AnimatePresence>
      </div>

      <nav aria-label="Main navigation" className="sidebar-nav">
        <ul>
          {items.map((item) => (
            <li key={item.to}>
              <NavLink to={item.to} end={item.end} className="nav-link" onClick={mobile ? onClose : undefined} title={showLabels ? undefined : item.label}>
                {({ isActive }) => (
                  <>
                    {isActive && <motion.span layoutId={mobile ? 'nav-active-mobile' : 'nav-active'} className="nav-active-pill" transition={spring.snappy} />}
                    <Icon name={item.icon} className="nav-icon" />
                    <AnimatePresence initial={false}>
                      {showLabels && (
                        <motion.span key="label" className="nav-label" variants={fadeVariants} initial="initial" animate="animate" exit="exit">{item.label}</motion.span>
                      )}
                    </AnimatePresence>
                  </>
                )}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      {!mobile && (
        <button type="button" className="sidebar-collapse" onClick={onToggleCollapse} aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'} aria-expanded={!collapsed}>
          <Icon name={collapsed ? 'chevronRight' : 'chevronLeft'} />
          {showLabels && <span>Collapse</span>}
        </button>
      )}
    </>
  );

  if (mobile) {
    return (
      <AnimatePresence>
        {open && (
          <>
            <motion.div key="scrim" className="scrim" variants={fadeVariants} initial="initial" animate="animate" exit="exit" onClick={onClose} />
            <motion.aside key="drawer" className="sidebar sidebar-mobile" initial={{ x: '-100%' }} animate={{ x: 0 }} exit={{ x: '-100%' }} transition={spring.gentle}>
              {content}
            </motion.aside>
          </>
        )}
      </AnimatePresence>
    );
  }

  return (
    <motion.aside className="sidebar" initial={false} animate={{ width: collapsed ? 76 : 248 }} transition={spring.soft}>
      {content}
    </motion.aside>
  );
}
