import { useCallback, useEffect, useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { Link, useNavigate } from 'react-router-dom';
import { spring } from '../utils/motion';
import { useAuth } from '../auth/useAuth';
import { useTheme } from '../contexts/ThemeContext';
import { notificationApi } from '../api/index';
import Dropdown from '../components/ui/Dropdown';
import Icon from '../components/ui/Icon';

export const NOTIFICATIONS_CHANGED = 'cms:notifications-changed';

function useUnreadCount() {
  const [count, setCount] = useState(0);
  const load = useCallback((signal) => {
    notificationApi.unreadCount(signal).then((r) => setCount(r.unreadCount)).catch(() => { /* badge is best-effort */ });
  }, []);
  useEffect(() => {
    const controller = new AbortController();
    load(controller.signal);
    const timer = setInterval(() => load(), 60000);
    const onChanged = () => load();
    window.addEventListener(NOTIFICATIONS_CHANGED, onChanged);
    return () => { controller.abort(); clearInterval(timer); window.removeEventListener(NOTIFICATIONS_CHANGED, onChanged); };
  }, [load]);
  return count;
}

export default function Topbar({ onMenu, showMenu }) {
  const { user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const unread = useUnreadCount();

  return (
    <header className="topbar">
      {showMenu ? <button type="button" className="icon-btn" onClick={onMenu} aria-label="Open navigation"><Icon name="menu" /></button> : <span />}
      <div className="topbar-right">
        <button type="button" className="icon-btn theme-toggle" onClick={toggleTheme} aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}>
          <AnimatePresence mode="wait" initial={false}>
            <motion.span key={theme} initial={{ rotate: -60, opacity: 0 }} animate={{ rotate: 0, opacity: 1 }} exit={{ rotate: 60, opacity: 0 }} transition={{ duration: 0.18 }} style={{ display: 'inline-flex' }}>
              <Icon name={theme === 'dark' ? 'sun' : 'moon'} />
            </motion.span>
          </AnimatePresence>
        </button>

        <Link to="/notifications" className="icon-btn bell" aria-label={`Notifications${unread ? `, ${unread} unread` : ''}`}>
          <Icon name="bell" />
          <AnimatePresence>
            {unread > 0 && (
              <motion.span key="badge" className="bell-badge" initial={{ scale: 0 }} animate={{ scale: 1 }} exit={{ scale: 0 }} transition={spring.snappy}>
                {unread > 99 ? '99+' : unread}
              </motion.span>
            )}
          </AnimatePresence>
        </Link>

        <Dropdown
          label="Account menu"
          trigger={(
            <span className="user-chip">
              <span className="avatar" aria-hidden="true">{(user?.fullName || '?').charAt(0).toUpperCase()}</span>
              <span className="user-meta"><strong>{user?.fullName}</strong><small>{user?.role}</small></span>
              <Icon name="chevronDown" size={14} />
            </span>
          )}
          items={[
            { label: 'Profile & password', onClick: () => navigate('/profile') },
            { label: theme === 'dark' ? 'Use light theme' : 'Use dark theme', onClick: toggleTheme },
            { label: 'Sign out', danger: true, onClick: () => { logout(); navigate('/login'); } }
          ]}
        />
      </div>
    </header>
  );
}
