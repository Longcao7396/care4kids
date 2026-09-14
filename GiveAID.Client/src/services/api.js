import axios from 'axios';
import { API_BASE_URL, BACKEND_CANDIDATES, STORAGE_KEYS } from '../config';

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Request interceptor - Add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle errors
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response) {
      // Server responded with error
      const { status } = error.response;

      if (status === 401) {
        // Unauthorized - clear token and dispatch a soft-redirect event.
        // We do NOT use window.location.href here because that wipes out
        // React Router state, including location.state.from which carries
        // the user's intended destination. The AuthBootstrap component
        // listens for this event and navigates with React Router, so the
        // `from` state survives a token-expired flow.
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER);
        window.dispatchEvent(new CustomEvent('giveaid:auth:expired', {
          detail: { from: window.location.pathname + window.location.search }
        }));
      } else if (status === 403) {
        // Forbidden - explicit friendly message so callers can show it.
        error.friendlyMessage = 'You do not have permission to perform this action.';
      } else if (status >= 500) {
        // Server error - generic friendly message.
        error.friendlyMessage = 'Server error. Please try again later or contact support.';
      }

      // Build a friendly message that includes the API-supplied message
      // (when present) for the caller to surface to the user.
      const apiMessage = error.response.data?.message
        || error.response.data?.Message
        || error.response.data?.title
        || error.response.statusText;
      if (apiMessage && !error.message?.includes(apiMessage)) {
        error.friendlyMessage = error.friendlyMessage
          ? `${error.friendlyMessage} (${apiMessage})`
          : `${status} ${apiMessage}`;
      }
    } else if (error.request) {
      // Try the next backend candidate if current one failed
      const currentBase = error.config?.baseURL || API_BASE_URL;
      const currentIdx = BACKEND_CANDIDATES.indexOf(currentBase);
      const nextBase = currentIdx >= 0 && currentIdx < BACKEND_CANDIDATES.length - 1
        ? BACKEND_CANDIDATES[currentIdx + 1]
        : null;

      if (nextBase && !error.config?._retried) {
        console.warn(`[api] ${currentBase} unreachable, retrying with ${nextBase}`);
        const retriedConfig = {
          ...error.config,
          baseURL: nextBase,
          _retried: true,
        };
        return axios.request(retriedConfig).catch((retryErr) => {
          retryErr.friendlyMessage = retryErr.friendlyMessage || '⚠️ Backend không phản hồi. Vui lòng khởi động backend.';
          return Promise.reject(retryErr);
        });
      }

      error.friendlyMessage = '⚠️ Backend không phản hồi. Vui lòng khởi động backend.';
      console.error(error.friendlyMessage);
    }
    return Promise.reject(error);
  }
);

export default api;
