import { createContext, useContext, useMemo } from 'react';
import { lookupApi } from '../api/index';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../auth/useAuth';

const LookupsContext = createContext(null);

export function LookupsProvider({ children }) {
  const { isAuthenticated } = useAuth();
  const { data, loading, error, reload } = useApi((signal) => lookupApi.all(signal), [isAuthenticated], { enabled: isAuthenticated });

  const value = useMemo(() => ({
    departments: data?.departments ?? [],
    agencies: data?.agencies ?? [],
    frequencies: data?.frequencies ?? [],
    roles: data?.roles ?? [],
    loading,
    error,
    reload
  }), [data, loading, error, reload]);

  return <LookupsContext.Provider value={value}>{children}</LookupsContext.Provider>;
}

export function useLookups() {
  const ctx = useContext(LookupsContext);
  if (!ctx) throw new Error('useLookups must be used inside LookupsProvider');
  return ctx;
}
