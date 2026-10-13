import { useEffect, useState } from 'react';
import { AnimatePresence, motion, useAnimationControls } from 'framer-motion';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { fadeVariants, shakeVariants, spring } from '../utils/motion';
import Button from '../components/ui/Button';
import Icon from '../components/ui/Icon';
import { useAuth } from '../auth/useAuth';
import { useTheme } from '../contexts/ThemeContext';
import { API_BASE_URL } from '../api/axiosClient';
import { healthApi } from '../api/index';

function useApiHealth() {
  const [state, setState] = useState({ status: 'checking', message: '' });
  useEffect(() => {
    let cancelled = false;
    healthApi.check()
      .then(() => { if (!cancelled) setState({ status: 'ok', message: '' }); })
      .catch((error) => {
        if (cancelled) return;
        if (error.network) setState({ status: 'down', message: error.message });
        else if (error.status === 503) setState({ status: 'db', message: 'The API is running but cannot connect to SQL Server. Check that SQL Server is running, the database exists (scripts 01–05) and the connection string in appsettings.Local.json is correct.' });
        else setState({ status: 'down', message: error.message });
      });
    return () => { cancelled = true; };
  }, []);
  return state;
}

export default function LoginPage() {
  const { login, isAuthenticated, initializing } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const controls = useAnimationControls();
  const health = useApiHealth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  if (!initializing && isAuthenticated) return <Navigate to={location.state?.from || '/'} replace />;

  const submit = async (event) => {
    event.preventDefault();
    if (!username.trim() || !password) {
      setError('Enter your username and password.');
      controls.start('shake');
      return;
    }
    setLoading(true);
    setError(null);
    try {
      await login(username.trim(), password);
      navigate(location.state?.from || '/', { replace: true });
    } catch (e) {
      setError(e?.message || 'Sign-in failed.');
      controls.start('shake');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <button type="button" className="icon-btn login-theme" onClick={toggleTheme} aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}>
        <Icon name={theme === 'dark' ? 'sun' : 'moon'} />
      </button>
      <motion.div className="login-card" initial={{ opacity: 0, y: 24, scale: 0.98 }} animate={{ opacity: 1, y: 0, scale: 1 }} transition={spring.gentle}>
        <div className="login-brand">
          <span className="brand-mark brand-mark-lg" aria-hidden="true"><Icon name="check" size={26} /></span>
          <h1>Calibration Management</h1>
          <p className="muted">Sign in to manage equipment, plans and certificates.</p>
        </div>

        <AnimatePresence>
          {health.status === 'down' || health.status === 'db' ? (
            <motion.div key="health" className="alert alert-error" role="alert" variants={fadeVariants} initial="initial" animate="animate" exit="exit">
              <strong>{health.status === 'db' ? 'Database unavailable' : 'Cannot connect to the API'}</strong>
              <span>{health.message}</span>
              <small>API address: {API_BASE_URL}</small>
            </motion.div>
          ) : null}
        </AnimatePresence>

        <motion.form onSubmit={submit} noValidate variants={shakeVariants} initial="initial" animate={controls}>
          <div className="form-field">
            <label htmlFor="username">Username or email</label>
            <input id="username" className="input" autoComplete="username" autoFocus value={username} onChange={(e) => setUsername(e.target.value)} aria-invalid={error ? true : undefined} />
          </div>
          <div className="form-field">
            <label htmlFor="password">Password</label>
            <input id="password" className="input" type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} aria-invalid={error ? true : undefined} aria-describedby={error ? 'login-error' : undefined} />
          </div>
          <AnimatePresence>
            {error ? (
              <motion.p key="err" id="login-error" className="form-error" role="alert" variants={fadeVariants} initial="initial" animate="animate" exit="exit">{error}</motion.p>
            ) : null}
          </AnimatePresence>
          <Button variant="primary" type="submit" size="lg" loading={loading} className="btn-block">Sign in</Button>
        </motion.form>
        <p className="muted small login-help">Accounts are created by an administrator. Forgot your password? Ask an administrator to reset it.</p>
      </motion.div>
    </div>
  );
}
