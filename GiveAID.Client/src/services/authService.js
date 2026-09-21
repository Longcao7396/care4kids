import api from './api';
import { STORAGE_KEYS } from '../config';

class AuthService {
  getStoredUser() {
    try {
      const userStr = localStorage.getItem(STORAGE_KEYS.USER);
      return userStr ? JSON.parse(userStr) : null;
    } catch {
      return null;
    }
  }

  isAuthenticated() {
    const token = localStorage.getItem(STORAGE_KEYS.TOKEN);
    return !!token;
  }

  // ── v2.0 API contract ─────────────────────────────────────────────────────────
  // POST /auth/login  →  { success, message, data: { token, userId, email, username, role, expiresAt } }
  // The api.js interceptor unwraps { success, message, data } → returns body.data directly.
  // So `response` here is already { token, userId, email, username, role, expiresAt }.
  async login(credentials) {
    try {
      const response = await api.post('/auth/login', credentials);
      // response is already the unwrapped data: { token, userId, email, username, role, expiresAt }
      if (response && response.token) {
        localStorage.setItem(STORAGE_KEYS.TOKEN, response.token);
        // Store user fields for quick access (without the token)
        const { token: _t, ...userInfo } = response;
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userInfo));
        return { success: true, data: response };
      }
      return { success: false, message: 'Invalid response from server.' };
    } catch (error) {
      // Error is already normalized by api.js interceptor
      return { success: false, message: error.message || 'Login failed. Please try again.' };
    }
  }

  async register(userData) {
    try {
      // POST /auth/register → 200 OK with { success, message, data }
      // The api interceptor unwraps { success, message, data } → returns body.data directly.
      // So `response` here is already { userId, username, email, fullName, role, isActive, isVerified }.
      const response = await api.post('/auth/register', userData);
      // response is the unwrapped data: { userId, username, email, ... }
      if (response && response.userId) {
        return { success: true, data: response };
      }
      // Edge case: 200 but no userId in data (shouldn't happen)
      return { success: true, message: 'Registration successful! Please login.' };
    } catch (error) {
      return { success: false, message: error.message || 'Registration failed. Please try again.' };
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
