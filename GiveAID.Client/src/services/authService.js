import axios from 'axios';
import api from './api';
import { API_BASE_URL, STORAGE_KEYS } from '../config';

// Raw axios instance for auth calls that need the full envelope.
// api instance uses interceptors that strip the { success, message, data }
// wrapper, but authService.register needs to read response.success.
const raw = axios.create({ baseURL: API_BASE_URL, timeout: 30000 });

class AuthService {
  getStoredUser() {
    try {
      const userStr = localStorage.getItem(STORAGE_KEYS.USER);
      return userStr ? JSON.parse(userStr) : null;
    } catch {
      return null;
    }
  }

  // L-07: Check both token existence AND expiry
  isAuthenticated() {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    if (!token) return false;

    // Check token expiry from stored user data
    const user = this.getStoredUser();
    if (user && user.expiresAt) {
      const expiryTime = new Date(user.expiresAt);
      if (expiryTime <= new Date()) {
        // Token expired - clear auth state
        this.logout();
        return false;
      }
    }

    return true;
  }

  // L-07: Helper to get remaining time until token expiry (in ms)
  getTokenExpiresIn() {
    const user = this.getStoredUser();
    if (user && user.expiresAt) {
      const expiryTime = new Date(user.expiresAt).getTime();
      const now = Date.now();
      return Math.max(0, expiryTime - now);
    }
    return 0;
  }

  // L-07: Check if token is about to expire (within 5 minutes)
  isTokenExpiringSoon() {
    const remaining = this.getTokenExpiresIn();
    return remaining > 0 && remaining < 5 * 60 * 1000; // 5 minutes
  }

  // ── v2.0 API contract ─────────────────────────────────────────────────────────
  // POST /auth/login  →  { success, message, data: { token, userId, email, username, role, expiresAt } }
  // On success: api.js interceptor unwraps { success, message, data } → returns body.data → response has token.
  // On failure: interceptor throws with err.response.data preserved (status, success, message, code).
  async login(credentials) {
    const response = await api.post('/auth/login', credentials);
    // Success path: response is { token, userId, email, username, role, expiresAt }
    if (response && response.token) {
      localStorage.setItem(STORAGE_KEYS.TOKEN, response.token);
      // Store user fields for quick access (without the token)
      const { token: _t, ...userInfo } = response;
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userInfo));
      return response; // AuthContext expects { token, ... }
    }
    // If we reach here, the interceptor gave us a non-token object — treat as error.
    // The error thrown from api.js will have .response.data.code and .response.data.message.
    throw new Error('Unexpected login response');
  }

  async register(userData) {
    try {
      // POST /auth/register → 200 OK with { success, message, data: { userId, username, email, fullName, role, isActive, isVerified } }
      // Use raw axios (bypass interceptor) to get the full envelope with success/message.
      const response = await raw.post('/auth/register', userData);
      const body = response.data;
      if (body && body.success) {
        return { success: true, message: body.message || 'Registration successful! Please login.' };
      }
      return { success: false, message: (body && body.message) || 'Registration failed.' };
    } catch (error) {
      const msg = error.response?.data?.message || error.message || 'Registration failed. Please try again.';
      return { success: false, message: msg };
    }
  }

  async logout() {
    try {
      await api.post('/auth/logout');
    } catch {
      // Ignore logout errors
    } finally {
      localStorage.removeItem(STORAGE_KEYS.TOKEN);
      localStorage.removeItem(STORAGE_KEYS.USER);
    }
  }

  // GET /auth/me → { success, message, data: { userId, email, username, role, ... } }
  // Interceptor unwraps → response is the user object directly.
  async getCurrentUser() {
    try {
      const response = await api.get('/auth/me');
      if (response && typeof response === 'object' && response.userId) {
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(response));
        return response;
      }
      return null;
    } catch {
      return null;
    }
  }
}

export const authService = new AuthService();
export { AuthService };
export default authService;
