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

  async login(credentials) {
    try {
      const response = await api.post('/auth/login', credentials);
      if (response.data?.success && response.data.data) {
        const { token, user } = response.data.data;
        localStorage.setItem(STORAGE_KEYS.TOKEN, token);
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
        return { success: true, data: response.data.data };
      }
      return { success: false, message: response.data?.message || 'Login failed' };
    } catch (error) {
      const message = error.friendlyMessage
        || error.response?.data?.message
        || error.message
        || 'Login failed. Please try again.';
      return { success: false, message };
    }
  }

  async register(userData) {
    try {
      const response = await api.post('/auth/register', userData);
      if (response.data?.success) {
        return { success: true, message: 'Registration successful! Please login.' };
      }
      return { success: false, message: response.data?.message || 'Registration failed' };
    } catch (error) {
      const message = error.friendlyMessage
        || error.response?.data?.message
        || error.message
        || 'Registration failed. Please try again.';
      return { success: false, message };
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

  async getCurrentUser() {
    try {
      const response = await api.get('/auth/me');
      if (response.data?.success && response.data.data) {
        const user = response.data.data;
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
        return user;
      }
      return null;
    } catch {
      return null;
    }
  }
}

export const authService = new AuthService();
export default authService;
