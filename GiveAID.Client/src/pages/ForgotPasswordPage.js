import React, { useState } from 'react';
import { Container, Row, Col, Card, Form, Button, Alert } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import api from '../services/api';
import './AuthPages.css';

function ForgotPasswordPage() {
    const [email, setEmail] = useState('');
    const [submitted, setSubmitted] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await api.post('/auth/forgot-password', { email });
            if (response.data?.success) {
                setSubmitted(true);
            } else {
                setError(response.data?.message || 'An error occurred. Please try again.');
            }
        } catch (err) {
            // Even on error from API, show success to prevent email enumeration
            setSubmitted(true);
        } finally {
            setLoading(false);
        }
    };

    if (submitted) {
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
                                        <div className="auth-logo-icon">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                                            </svg>
                                        </div>
                                    </div>
                                    <h3 className="mb-3">Check Your Email</h3>
                                    <p className="text-muted mb-4">
                                        If an account exists with that email address, we've sent password reset instructions.
                                    </p>
                                    <p className="text-muted small mb-4">
                                        Didn't receive the email? Check your spam folder or try again.
                                    </p>
                                    <Button
                                        variant="link"
                                        className="auth-footer-link-action"
                                        onClick={() => navigate('/login')}
                                    >
                                        Back to Login
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
                                    <h1 className="auth-title">Forgot Password?</h1>
                                    <p className="auth-subtitle">Enter your email and we'll send you reset instructions</p>
                                </div>

                                {error && (
                                    <Alert variant="danger" className="auth-alert" dismissible onClose={() => setError('')}>
                                        {error}
                                    </Alert>
                                )}

                                <Form onSubmit={handleSubmit} noValidate>
                                    <Form.Group className="auth-form-group">
                                        <Form.Label className="auth-label">Email Address</Form.Label>
                                        <div className="auth-input-wrap">
                                            <span className="auth-input-icon">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round">
                                                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                                                </svg>
                                            </span>
                                            <Form.Control
                                                type="email"
                                                value={email}
                                                onChange={(e) => setEmail(e.target.value)}
                                                placeholder="Enter your email"
                                                className="auth-input"
                                                required
                                            />
                                        </div>
                                    </Form.Group>

                                    <Button
                                        type="submit"
                                        className="auth-btn-submit w-100"
                                        disabled={loading}
                                    >
                                        {loading ? 'Sending...' : 'Send Reset Link'}
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

export default ForgotPasswordPage;
