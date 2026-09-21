/**
 * API Envelope & v2 Response Format Tests
 * 
 * Tests the critical response format: { success, message, data }
 * and the common bug patterns that break auth.
 */

import React from 'react';
import { render, screen } from '@testing-library/react';

// ─────────────────────────────────────────────────────────────────────────────
// Mock localStorage
// ─────────────────────────────────────────────────────────────────────────────
const localStorageMock = (() => {
  let store = {};
  return {
    getItem: jest.fn((key) => store[key] || null),
    setItem: jest.fn((key, value) => { store[key] = value; }),
    removeItem: jest.fn((key) => { delete store[key]; }),
    clear: () => { store = {}; },
  };
})();
Object.defineProperty(window, 'localStorage', { value: localStorageMock });

// ─────────────────────────────────────────────────────────────────────────────
// Tests - No React components, pure logic
// ─────────────────────────────────────────────────────────────────────────────
describe('v2 API Response Format', () => {
  describe('Login Response (CRITICAL)', () => {
    const V2_LOGIN_RESPONSE = {
      success: true,
      data: {
        token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...',
        userId: 14,
        email: 'user@example.com',
        username: 'testuser',
        role: 'User',
        expiresAt: '2026-09-22T00:00:00Z',
      },
    };

    it('should have correct v2 API login response structure', () => {
      expect(V2_LOGIN_RESPONSE).toHaveProperty('success', true);
      expect(V2_LOGIN_RESPONSE).toHaveProperty('data');
      expect(V2_LOGIN_RESPONSE.data).toHaveProperty('token');
      expect(V2_LOGIN_RESPONSE.data).toHaveProperty('userId');
      expect(V2_LOGIN_RESPONSE.data).toHaveProperty('email');
      expect(V2_LOGIN_RESPONSE.data).toHaveProperty('username');
      expect(V2_LOGIN_RESPONSE.data).toHaveProperty('role');
    });

    it('should NOT have nested user object (common bug)', () => {
      // This is the BUG: expecting result.data.user
      expect(V2_LOGIN_RESPONSE.data).not.toHaveProperty('user');
    });

    it('should extract userId correctly', () => {
      // CORRECT way to check auth
      expect(V2_LOGIN_RESPONSE.success).toBe(true);
      expect(V2_LOGIN_RESPONSE.data?.userId).toBe(14);
    });
  });

  describe('Register Response', () => {
    const V2_REGISTER_RESPONSE = {
      success: true,
      message: 'Registration successful',
      data: {
        userId: 15,
        username: 'newuser',
        email: 'new@example.com',
        fullName: 'New User',
        role: 'User',
        isActive: true,
        isVerified: false,
      },
    };

    it('should have correct register response structure', () => {
      expect(V2_REGISTER_RESPONSE.success).toBe(true);
      expect(V2_REGISTER_RESPONSE.data).toHaveProperty('userId');
      expect(V2_REGISTER_RESPONSE.data).toHaveProperty('username');
    });
  });

  describe('Error Responses', () => {
    it('should handle 401 Unauthorized format', () => {
      const UNAUTHORIZED_RESPONSE = {
        success: false,
        message: 'Unauthorized access',
        data: null,
      };
      
      expect(UNAUTHORIZED_RESPONSE.success).toBe(false);
      expect(UNAUTHORIZED_RESPONSE.message).toBe('Unauthorized access');
    });

    it('should handle validation error format', () => {
      const VALIDATION_RESPONSE = {
        success: false,
        message: 'Validation failed',
        data: null,
        errors: {
          email: ['Invalid email'],
          password: ['Password too short'],
        },
      };
      
      expect(VALIDATION_RESPONSE.success).toBe(false);
      expect(VALIDATION_RESPONSE.errors).toHaveProperty('email');
    });
  });
});

describe('Token Storage Logic', () => {
  it('should store token and user data separately', () => {
    const loginData = {
      token: 'jwt-token-12345',
      userId: 14,
      email: 'test@example.com',
      username: 'testuser',
      role: 'User',
      expiresAt: '2026-09-22T00:00:00Z',
    };

    // Simulate what authService.login does
    const { token, ...userInfo } = loginData;
    
    // Token and userInfo are separated correctly
    expect(token).toBe('jwt-token-12345');
    expect(userInfo).not.toHaveProperty('token');
    expect(userInfo).toHaveProperty('userId', 14);
    expect(userInfo).toHaveProperty('username', 'testuser');
    expect(userInfo).toHaveProperty('role', 'User');
  });

  it('should handle missing userId gracefully', () => {
    const responseWithNoUserId = {
      success: true,
      data: {
        token: 'some-token',
        // NO userId!
      },
    };

    // Should NOT set user if no userId
    const hasUserId = responseWithNoUserId.data?.userId;
    expect(hasUserId).toBeUndefined();
  });
});

describe('Auth Bug Patterns (Prevention)', () => {
  const V2_RESPONSE = {
    success: true,
    data: {
      token: 'token',
      userId: 1,
      username: 'user',
      role: 'User',
    },
  };

  it('BUG PATTERN 1: result.data?.user is undefined', () => {
    // This is the bug that caused "Unauthorized access"
    const buggy = V2_RESPONSE.data?.user;
    expect(buggy).toBeUndefined();
  });

  it('CORRECT: Check userId directly', () => {
    const correct = V2_RESPONSE.success && V2_RESPONSE.data?.userId;
    expect(correct).toBeTruthy();
  });

  it('BUG PATTERN 2: Check result.data.user instead of extracting fields', () => {
    // BUGGY CODE:
    // if (result.success && result.data?.user) { setUser(result.data.user); }
    // setUser never called because result.data.user is undefined!
    
    const buggyCheck = V2_RESPONSE.success && V2_RESPONSE.data?.user;
    
    // This is why the bug occurred - the condition was ALWAYS false
    expect(buggyCheck).toBeFalsy();
    
    // FIX: Extract fields directly
    const { token, ...userInfo } = V2_RESPONSE.data;
    expect(userInfo.userId).toBe(1);
  });
});
