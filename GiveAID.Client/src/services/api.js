import axios from 'axios';
import { API_BASE_URL, STORAGE_KEYS } from '../config';

// Create axios instance — baseURL matches v2.0 API contract
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' },
});

// ── Request interceptor ──────────────────────────────────────────────────────────
// Attaches JWT Bearer token from localStorage to every outgoing request.
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Response interceptor ─────────────────────────────────────────────────────────
// v2.0 API returns { success, message, data, errors? } envelope.
// We unwrap it here so all service functions get the raw data directly.
// Any non-2xx response is normalized into a plain Error with a user-friendly message.
api.interceptors.response.use(
  (response) => {
    const body = response.data;
    // Only unwrap the v2.0 envelope when present AND it carries actual data.
    // This preserves message strings from POST responses like contact-submit
    // (where data is null but message is the user's confirmation text).
    if (body && typeof body === 'object' && 'success' in body) {
      if (!body.success) {
        return Promise.reject(
          Object.assign(new Error(body.message || 'API call failed'), { _raw: body })
        );
      }
      // Return body.data when it exists and is not null; otherwise return the
      // full envelope so callers can read body.message (e.g. contact-submit
      // confirmation, register success).
      // The null check covers register (data=null) and forgot-password (data=null)
      // responses that need body.message.
      return body.data != null ? body.data : body;
    }
    // No envelope — return as-is (e.g. HTML from /health)
    return body;
  },
  (error) => {
    // Normalize the error message from v2.0 envelope or network failure
    const msg =
      error.response?.data?.message ||
      error.response?.data?.Message ||
      error.message ||
      'An unexpected error occurred.';

    if (error.response?.status === 401) {
      // Token expired or invalid — clear auth state and redirect to login.
      // We use window.location instead of React Router here because this
      // interceptor doesn't have access to the router context, and AuthBootstrap
      // / ProtectedRoute will not mount for a non-React navigation.
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    } else if (error.response?.status === 403) {
      error.friendlyMessage = 'You do not have permission to perform this action.';
    } else if (error.response?.status >= 500) {
      error.friendlyMessage = 'Server error. Please try again later or contact support.';
    }

    // Build a user-friendly error for callers
    const friendly = error.friendlyMessage
      ? `${error.friendlyMessage} (${msg})`
      : msg;

    return Promise.reject(Object.assign(new Error(friendly), { _raw: error.response?.data }));
  }
);

export default api;
