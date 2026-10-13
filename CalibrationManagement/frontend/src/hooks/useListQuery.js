import { useCallback, useState } from 'react';
import { useApi } from './useApi';
import { useDebounce } from './useDebounce';

/**
 * Shared list behaviour: filters + page state, debounced search, API loading/error state.
 * `fetcher(params, signal)` must return { items, page, pageSize, totalCount, totalPages }.
 */
export function useListQuery(fetcher, initialFilters = {}, { pageSize = 25 } = {}) {
  const [filters, setFilters] = useState(initialFilters);
  const [page, setPage] = useState(1);
  const debouncedFilters = useDebounce(filters, 350);

  const query = useApi(
    (signal) => fetcher({ ...debouncedFilters, page, pageSize }, signal),
    [JSON.stringify(debouncedFilters), page, pageSize]
  );

  const setFilter = useCallback((name, value) => {
    setFilters((f) => ({ ...f, [name]: value }));
    setPage(1);
  }, []);

  const resetFilters = useCallback(() => {
    setFilters(initialFilters);
    setPage(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return { ...query, filters, setFilter, resetFilters, page, setPage, pageSize };
}
