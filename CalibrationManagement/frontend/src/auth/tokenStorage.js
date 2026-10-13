const TOKEN_KEY = 'cms.token';

/** JWT access-token storage. sessionStorage: the token is dropped when the browser tab closes. */
export const tokenStore = {
  get() {
    try { return sessionStorage.getItem(TOKEN_KEY); } catch { return null; }
  },
  set(token) {
    try { sessionStorage.setItem(TOKEN_KEY, token); } catch { /* storage unavailable */ }
  },
  clear() {
    try { sessionStorage.removeItem(TOKEN_KEY); } catch { /* storage unavailable */ }
  }
};
