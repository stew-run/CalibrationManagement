import { createContext, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { authApi } from '../api/index';
import { tokenStore } from './tokenStorage';
import { useToast } from '../contexts/ToastContext';

export const AuthContext = createContext(null);
const EXPIRY_KEY = 'cms.expires';

const PERMISSIONS = {
  read: ['Administrator', 'QualityManager', 'Technician', 'Viewer'],
  write: ['Administrator', 'QualityManager', 'Technician'],
  manage: ['Administrator', 'QualityManager'],
  admin: ['Administrator']
};

export function AuthProvider({ children }) {
  const toast = useToast();
  const [user, setUser] = useState(null);
  const [initializing, setInitializing] = useState(() => Boolean(tokenStore.get()));
  const expiryTimer = useRef(null);

  const clearSession = useCallback(() => {
    tokenStore.clear();
    try { sessionStorage.removeItem(EXPIRY_KEY); } catch { /* storage unavailable */ }
    if (expiryTimer.current) clearTimeout(expiryTimer.current);
    setUser(null);
  }, []);

  const scheduleExpiry = useCallback((expiresAt) => {
    if (expiryTimer.current) clearTimeout(expiryTimer.current);
    const ms = new Date(expiresAt).getTime() - Date.now();
    if (Number.isFinite(ms) && ms > 0) {
      // setTimeout cannot exceed ~24.8 days; tokens last minutes, so this is safe.
      expiryTimer.current = setTimeout(() => {
        clearSession();
        toast.info('Your session expired. Please sign in again.');
      }, ms);
    }
  }, [clearSession, toast]);

  // Restore the session after a page reload (token lives in sessionStorage for the tab only).
  useEffect(() => {
    if (!tokenStore.get()) return undefined;
    const controller = new AbortController();
    authApi.me(controller.signal)
      .then((profile) => {
        setUser(profile);
        const expiresAt = sessionStorage.getItem(EXPIRY_KEY);
        if (expiresAt) scheduleExpiry(expiresAt);
      })
      .catch((error) => {
        if (error?.name === 'CanceledError' || error?.code === 'ERR_CANCELED') return;
        clearSession();
        if (error?.network) toast.error(error.message);
      })
      .finally(() => { if (!controller.signal.aborted) setInitializing(false); });
    return () => controller.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // The API client dispatches this when any request returns 401 (expired or revoked token).
  useEffect(() => {
    const onUnauthorized = () => {
      clearSession();
      toast.info('Your session expired. Please sign in again.');
    };
    window.addEventListener('cms:unauthorized', onUnauthorized);
    return () => window.removeEventListener('cms:unauthorized', onUnauthorized);
  }, [clearSession, toast]);

  const login = useCallback(async (username, password) => {
    const response = await authApi.login(username, password);
    tokenStore.set(response.token);
    try { sessionStorage.setItem(EXPIRY_KEY, response.expiresAtUtc); } catch { /* storage unavailable */ }
    scheduleExpiry(response.expiresAtUtc);
    setUser(response.user);
    return response.user;
  }, [scheduleExpiry]);

  const can = useCallback((permission) => Boolean(user && PERMISSIONS[permission]?.includes(user.role)), [user]);

  const value = useMemo(
    () => ({ user, setUser, initializing, login, logout: clearSession, can, isAuthenticated: Boolean(user) }),
    [user, initializing, login, clearSession, can]
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
