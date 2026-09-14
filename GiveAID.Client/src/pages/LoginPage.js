import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Spinner } from 'react-bootstrap';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { STORAGE_KEYS } from '../config';
import './AuthPages.css';

function LoginPage() {
  const [form, setForm] = useState({ email: '', password: '', rememberMe: false });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  // Honor the URL the user originally tried to reach (set by ProtectedRoute
  // or AuthBootstrap on 401). Fall back to /dashboard.
  const intendedFrom = location.state?.from || '/dashboard';

  /* Redirect if already logged in — only fires once on mount. */
  useEffect(() => {
    if (localStorage.getItem(STORAGE_KEYS.TOKEN)) {
      navigate(intendedFrom, { replace: true });
    }
  }, [navigate, intendedFrom]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    if (error) setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.email || !form.password) {
      setError('Please enter your email and password.');
      return;
    }
    setError('');
    setLoading(true);

    try {
      const result = await login({ email: form.email, password: form.password });
      // Navigation happens via the effect below once user state updates.
      // The previous bug was calling navigate() before AuthContext's setUser
      // had propagated, which made ProtectedRoute bounce the user back to /login.
      if (!result?.success) {
        setError(result?.message || 'Login failed. Please try again.');
      }
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  /* Watch the auth context — once the user is populated after login,
   * navigate to the originally-requested URL (or dashboard). This
   * guarantees navigation happens AFTER AuthContext's setUser has
   * propagated, so ProtectedRoute sees user set. */
  const { user } = useAuth();
  useEffect(() => {
    if (user && !loading) {
      navigate(intendedFrom, { replace: true });
    }
  }, [user, loading, navigate, intendedFrom]);

  return (
    <div className="auth-page">
      {/* Decorative background blobs */}
      <div className="auth-bg-blob auth-bg-blob-1" />
      <div className="auth-bg-blob auth-bg-blob-2" />

      <Container>
        <Row className="justify-content-center align-items-center min-vh-75">
          <Col md={6} lg={5} xl={4}>
            {/* Card */}
            <div className="auth-card">
              {/* Top accent bar */}
              <div className="auth-card-bar" />

              <div className="auth-card-body">
                {/* Logo + title */}
                <div className="auth-header text-center">
                  <div className="auth-logo-wrap mb-3">
                    <div className="auth-logo-icon">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
                      </svg>
                    </div>
                    <div className="auth-logo-text">
                      <span className="auth-logo-brand">Care</span>
                      <span className="auth-logo-accent">4</span>
                      <span className="auth-logo-brand">Kids</span>
                    </div>
                  </div>
                  <h1 className="auth-title">Welcome back</h1>
                  <p className="auth-subtitle">Sign in to continue supporting children's welfare</p>
                </div>

                {/* Error */}
                {error && (
                  <Alert
                    variant="danger"
                    className="auth-alert"
                    dismissible
                    onClose={() => setError('')}
                    role="alert"
                  >
                    <span className="auth-alert-icon" aria-hidden="true">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                        <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                      </svg>
                    </span>
                    <span className="auth-alert-content">{error}</span>
                  </Alert>
                )}

                {/* Form */}
                <Form onSubmit={handleSubmit} noValidate>
                  <Form.Group className="auth-form-group">
                    <Form.Label className="auth-label" htmlFor="login-email">Email Address</Form.Label>
                    <div className="auth-input-wrap">
                      <span className="auth-input-icon" aria-hidden="true">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                          <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                        </svg>
                      </span>
                      <Form.Control
                        id="login-email"
                        type="email"
                        name="email"
                        placeholder="you@example.com"
                        value={form.email}
                        onChange={handleChange}
                        className="auth-input"
                        autoComplete="email"
                        required
                      />
                    </div>
                  </Form.Group>

                  <Form.Group className="auth-form-group">
                    <div className="d-flex justify-content-between align-items-center mb-2">
                      <Form.Label className="auth-label mb-0" htmlFor="login-password">Password</Form.Label>
                      <Link to="/forgot-password" className="auth-forgot-link">Forgot password?</Link>
                    </div>
                    <div className="auth-input-wrap">
                      <span className="auth-input-icon" aria-hidden="true">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                          <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                        </svg>
                      </span>
                      <Form.Control
                        id="login-password"
                        type={showPassword ? 'text' : 'password'}
                        name="password"
                        placeholder="Enter your password"
                        value={form.password}
                        onChange={handleChange}
                        className="auth-input"
                        autoComplete="current-password"
                        required
                      />
                      <span className="auth-password-toggle-col">
                        <button
                          type="button"
                          className="auth-password-toggle"
                          onClick={() => setShowPassword((v) => !v)}
                          aria-label={showPassword ? 'Hide password' : 'Show password'}
                        >
                          {showPassword ? (
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                              <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                              <line x1="1" y1="1" x2="23" y2="23"/>
                            </svg>
                          ) : (
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>
                            </svg>
                          )}
                        </button>
                      </span>
                    </div>
                  </Form.Group>

                  <div className="auth-remember-row">
                    <Form.Check
                      type="checkbox"
                      name="rememberMe"
                      id="rememberMe"
                      label="Remember me"
                      checked={form.rememberMe}
                      onChange={handleChange}
                      className="auth-checkbox"
                    />
                  </div>

                  <Button
                    type="submit"
                    className="auth-btn-submit"
                    disabled={loading}
                  >
                    {loading ? (
                      <>
                        <Spinner as="span" animation="border" size="sm" aria-hidden="true" />
                        <span>Signing in…</span>
                      </>
                    ) : (
                      <>
                        <span>Sign In</span>
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                          <line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/>
                        </svg>
                      </>
                    )}
                  </Button>
                </Form>

                {/* Footer link */}
                <div className="auth-footer-link text-center">
                  <span className="auth-footer-text">Don't have an account? </span>
                  <Link to="/register" className="auth-footer-link-action">Create one free</Link>
                </div>

                {/* Trust footer — humanitarian microcopy */}
                <p className="auth-trust-footer">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                  </svg>
                  Your data is encrypted and never shared. We protect children's privacy.
                </p>
              </div>
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

export default LoginPage;
