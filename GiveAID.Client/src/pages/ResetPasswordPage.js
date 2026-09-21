import React, { useState } from 'react';
import { Container, Row, Col, Card, Form, Button, Alert } from 'react-bootstrap';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import api from '../services/api';
import './AuthPages.css';

function ResetPasswordPage() {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (password !== confirmPassword) {
            setError('Passwords do not match.');
            return;
        }

        if (password.length < 8) {
            setError('Password must be at least 8 characters.');
            return;
        }

        setLoading(true);

        try {
            const response = await api.post('/auth/reset-password', { token, newPassword: password });
            if (response.data?.success) {
                setSuccess(true);
                setTimeout(() => navigate('/login'), 3000);
            } else {
                setError(response.data?.message || 'Failed to reset password.');
            }
        } catch (err) {
            setError(
                err.response?.data?.message ||
                'Failed to reset password. The token may be expired. Please request a new password reset.'
            );
        } finally {
            setLoading(false);
        }
    };

    if (!token) {
        return (
            <div className="auth-page">
                <div className="auth-bg-blob auth-bg-blob-1" />
                <div className="auth-bg-blob auth-bg-blob-2" />
                <Container>
                    <Row className="justify-content-center align-items-center min-vh-75">
                        <Col md={6} lg={5} xl={4}>
                            <Card className="auth-card">
                                <div className="auth-card-bar" />
                                <Card.Body className="auth-card-body text-center p-5">
                                    <div className="auth-logo-wrap mb-3 mx-auto">
                                        <div className="auth-logo-icon text-danger">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                                            </svg>
                                        </div>
                                    </div>
                                    <h3 className="mb-3">Invalid Reset Link</h3>
                                    <p className="text-muted mb-4">
                                        This password reset link is invalid or has expired.
                                    </p>
                                    <Button
                                        variant="link"
                                        className="auth-footer-link-action"
                                        onClick={() => navigate('/forgot-password')}
                                    >
                                        Request New Reset Link
                                    </Button>
                                    <div className="mt-3">
                                        <Link to="/login" className="auth-footer-link-action">
                                            Back to Login
                                        </Link>
                                    </div>
                                </Card.Body>
                            </Card>
                        </Col>
                    </Row>
                </Container>
            </div>
        );
    }

    if (success) {
        return (
            <div className="auth-page">
                <div className="auth-bg-blob auth-bg-blob-1" />
                <div className="auth-bg-blob auth-bg-blob-2" />
                <Container>
                    <Row className="justify-content-center align-items-center min-vh-75">
                        <Col md={6} lg={5} xl={4}>
                            <Card className="auth-card">
                                <div className="auth-card-bar" style={{ background: '#22c55e' }} />
                                <Card.Body className="auth-card-body text-center p-5">
                                    <div className="auth-logo-wrap mb-3 mx-auto">
                                        <div className="auth-logo-icon" style={{ color: '#22c55e' }}>
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
                                            </svg>
                                        </div>
                                    </div>
                                    <h3 className="mb-3">Password Reset!</h3>
                                    <p className="text-muted mb-4">
                                        Your password has been successfully reset.
                                    </p>
                                    <p className="text-muted small mb-4">
                                        Redirecting you to login page...
                                    </p>
                                    <Button
                                        variant="link"
                                        className="auth-footer-link-action"
                                        onClick={() => navigate('/login')}
                                    >
                                        Go to Login Now
                                    </Button>
                                </Card.Body>
                            </Card>
                        </Col>
                    </Row>
                </Container>
            </div>
        );
    }

    return (
        <div className="auth-page">
            <div className="auth-bg-blob auth-bg-blob-1" />
            <div className="auth-bg-blob auth-bg-blob-2" />
            <Container>
                <Row className="justify-content-center align-items-center min-vh-75">
                    <Col md={6} lg={5} xl={4}>
                        <Card className="auth-card">
                            <div className="auth-card-bar" />
                            <Card.Body className="auth-card-body p-4">
                                <div className="auth-header text-center mb-4">
                                    <div className="auth-logo-wrap mb-3 mx-auto">
                                        <div className="auth-logo-icon">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                                            </svg>
                                        </div>
                                        <div className="auth-logo-text">
                                            <span className="auth-logo-brand">Care</span>
                                            <span className="auth-logo-accent">4</span>
                                            <span className="auth-logo-brand">Kids</span>
                                        </div>
                                    </div>
                                    <h1 className="auth-title">Set New Password</h1>
                                    <p className="auth-subtitle">Create a strong password for your account</p>
                                </div>

                                {error && (
                                    <Alert variant="danger" className="auth-alert" dismissible onClose={() => setError('')}>
                                        {error}
                                    </Alert>
                                )}

                                <Form onSubmit={handleSubmit} noValidate>
                                    <Form.Group className="auth-form-group">
                                        <Form.Label className="auth-label">New Password</Form.Label>
                                        <div className="auth-input-wrap">
                                            <span className="auth-input-icon">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                                                </svg>
                                            </span>
                                            <Form.Control
                                                type="password"
                                                value={password}
                                                onChange={(e) => setPassword(e.target.value)}
                                                placeholder="At least 8 characters"
                                                className="auth-input"
                                                required
                                                minLength={8}
                                            />
                                        </div>
                                    </Form.Group>

                                    <Form.Group className="auth-form-group">
                                        <Form.Label className="auth-label">Confirm Password</Form.Label>
                                        <div className="auth-input-wrap">
                                            <span className="auth-input-icon">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                                                </svg>
                                            </span>
                                            <Form.Control
                                                type="password"
                                                value={confirmPassword}
                                                onChange={(e) => setConfirmPassword(e.target.value)}
                                                placeholder="Confirm your password"
                                                className="auth-input"
                                                required
                                            />
                                        </div>
                                    </Form.Group>

                                    <div className="password-strength mb-3">
                                        <div className="strength-bar">
                                            <div className={`strength-segment ${password.length >= 8 ? 'active' : ''}`} />
                                            <div className={`strength-segment ${password.length >= 12 ? 'active' : ''}`} />
                                            <div className={`strength-segment ${/[A-Z]/.test(password) && password.length >= 8 ? 'active' : ''}`} />
                                        </div>
                                        <small className="text-muted">Use 8+ characters with uppercase, numbers, and symbols</small>
                                    </div>

                                    <Button
                                        type="submit"
                                        className="auth-btn-submit w-100"
                                        disabled={loading}
                                    >
                                        {loading ? 'Resetting...' : 'Reset Password'}
                                    </Button>
                                </Form>

                                <div className="auth-footer-link text-center mt-4">
                                    <span className="auth-footer-text">Remember your password? </span>
                                    <Link to="/login" className="auth-footer-link-action">Sign in</Link>
                                </div>
                            </Card.Body>
                        </Card>
                    </Col>
                </Row>
            </Container>
        </div>
    );
}

export default ResetPasswordPage;
