import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Spinner, Alert } from 'react-bootstrap';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { donationsService } from '../services';
import './DonationHistoryDetailPage.css';

/* ============================================================
   DONATION HISTORY DETAIL PAGE
   Displays full details of a single donation.
   Protected — requires authentication (ProtectedRoute).
   ============================================================ */

const DonationHistoryDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [donation, setDonation] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const ac = new AbortController();
    const loadDonation = async () => {
      try {
        setLoading(true);
        const response = await donationsService.getById(id);

        if (response.success && response.data) {
          setDonation(response.data);
        } else {
          setError(response.message || 'Donation not found.');
        }
      } catch (err) {
        console.error('Error loading donation:', err);
        setError(
          err.response?.status === 401
            ? 'Please log in to view this donation.'
            : 'Failed to load donation details. Please try again.'
        );
      } finally {
        setLoading(false);
      }
    };

    loadDonation();
    return () => ac.abort();
  }, [id]);

  // ─── Formatters ─────────────────────────────────────────────────────────

  const formatCurrency = (amount) =>
    new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0,
    }).format(amount || 0);

  const formatDate = (dateStr) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatDateShort = (dateStr) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getStatusConfig = (status) => {
    const config = {
      Completed: {
        label: 'Completed',
        cls: 'dhdp-badge-success',
        icon: (
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        ),
      },
      Pending: {
        label: 'Pending',
        cls: 'dhdp-badge-warning',
        icon: (
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
          </svg>
        ),
      },
      Failed: {
        label: 'Failed',
        cls: 'dhdp-badge-error',
        icon: (
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <circle cx="12" cy="12" r="10" />
            <line x1="15" y1="9" x2="9" y2="15" />
            <line x1="9" y1="9" x2="15" y2="15" />
          </svg>
        ),
      },
      Refunded: {
        label: 'Refunded',
        cls: 'dhdp-badge-refund',
        icon: (
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <polyline points="1 4 1 10 7 10" />
            <path d="M3.51 15a9 9 0 1 0 .49-3.99" />
          </svg>
        ),
      },
    };
    return config[status] || { label: status, cls: 'dhdp-badge-neutral', icon: null };
  };

  const getPaymentMethodLabel = (method) => {
    const map = {
      BankTransfer: 'Bank Transfer',
      NetBanking: 'Net Banking',
      CreditCard: 'Credit Card',
      DebitCard: 'Debit Card',
      Cash: 'Cash',
      MoMo: 'MoMo',
      VNPay: 'VNPay',
      stripe: 'Stripe',
    };
    return map[method] || method || '—';
  };

  const getPaymentGatewayLabel = (gateway) => {
    const map = {
      stripe: 'Stripe',
      vnpay: 'VNPay',
      momo: 'MoMo',
      mock: 'Demo Mode',
    };
    return map[gateway] || gateway || null;
  };

  // ─── Loading ────────────────────────────────────────────────────────────

  if (loading) {
    return (
      <div className="dhdp-page">
        <div className="dhdp-loading">
          <Spinner animation="border" style={{ color: 'var(--c4k-teal)' }} />
          <p>Loading donation details...</p>
        </div>
      </div>
    );
  }

  // ─── Error ──────────────────────────────────────────────────────────────

  if (error) {
    return (
      <div className="dhdp-page">
        <section className="dhdp-hero dhdp-hero-sm">
          <Container>
            <p className="eyebrow">Donation Details</p>
            <h1 className="dhdp-hero-title">My Donation</h1>
          </Container>
        </section>
        <Container className="dhdp-content">
          <Alert variant="danger" className="dhdp-alert">
            <strong>Unable to load donation.</strong> {error}
          </Alert>
          <div className="dhdp-actions">
            <button className="btn-coral" onClick={() => navigate('/my-donations')}>
              Back to My Donations
            </button>
            {error.includes('log in') && (
              <Link to="/login" className="btn-outline">
                Log In
              </Link>
            )}
          </div>
        </Container>
      </div>
    );
  }

  const statusConfig = getStatusConfig(donation.paymentStatus);
  const gatewayLabel = getPaymentGatewayLabel(donation.paymentGateway);

  // ─── Detail ──────────────────────────────────────────────────────────────

  return (
    <div className="dhdp-page">

      {/* ─── HERO ─── */}
      <section className="dhdp-hero">
        <Container>
          <nav className="dhdp-breadcrumb" aria-label="Breadcrumb">
            <Link to="/my-donations" className="dhdp-breadcrumb-link">
              My Donations
            </Link>
            <span className="dhdp-breadcrumb-sep" aria-hidden="true">/</span>
            <span className="dhdp-breadcrumb-current">
              #{donation.donationId}
            </span>
          </nav>

          <p className="eyebrow">Donation Details</p>
          <h1 className="dhdp-hero-title">
            {donation.campaignName || donation.causeName || 'General Donation'}
          </h1>

          {/* Summary strip */}
          <div className="dhdp-summary-strip">
            <div className="dhdp-summary-item">
              <span className="dhdp-summary-label">Amount</span>
              <span className="dhdp-summary-amount">
                {formatCurrency(donation.amount)}
              </span>
            </div>
            <div className="dhdp-summary-divider" aria-hidden="true" />
            <div className="dhdp-summary-item">
              <span className="dhdp-summary-label">Date</span>
              <span className="dhdp-summary-value">
                {formatDateShort(donation.donationDate)}
              </span>
            </div>
            <div className="dhdp-summary-divider" aria-hidden="true" />
            <div className="dhdp-summary-item">
              <span className="dhdp-summary-label">Status</span>
              <span className={`dhdp-badge ${statusConfig.cls}`}>
                {statusConfig.icon}
                {statusConfig.label}
              </span>
            </div>
          </div>
        </Container>
      </section>

      {/* ─── DETAIL CARDS ─── */}
      <section className="dhdp-detail-section">
        <Container>
          <Row className="dhdp-detail-row">

            {/* ─── Left Column ─── */}
            <Col lg={8}>

              {/* Payment Details */}
              <div className="dhdp-card">
                <div className="dhdp-card-header">
                  <div className="dhdp-card-icon">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                      <rect x="1" y="4" width="22" height="16" rx="2" ry="2" />
                      <line x1="1" y1="10" x2="23" y2="10" />
                    </svg>
                  </div>
                  <h2 className="dhdp-card-title">Payment Details</h2>
                </div>
                <div className="dhdp-card-body">
                  <div className="dhdp-detail-grid">
                    <div className="dhdp-detail-item">
                      <span className="dhdp-detail-label">Donation ID</span>
                      <span className="dhdp-detail-value dhdp-mono">
                        #DRP-{String(donation.donationId).padStart(5, '0')}
                      </span>
                    </div>

                    {donation.transactionId && (
                      <div className="dhdp-detail-item">
                        <span className="dhdp-detail-label">Transaction ID</span>
                        <span className="dhdp-detail-value dhdp-mono dhdp-teal">
                          {donation.transactionId}
                        </span>
                      </div>
                    )}

                    <div className="dhdp-detail-item">
                      <span className="dhdp-detail-label">Donation Date</span>
                      <span className="dhdp-detail-value">{formatDate(donation.donationDate)}</span>
                    </div>

                    {donation.paymentConfirmedAt && (
                      <div className="dhdp-detail-item">
                        <span className="dhdp-detail-label">Payment Confirmed</span>
                        <span className="dhdp-detail-value">{formatDate(donation.paymentConfirmedAt)}</span>
                      </div>
                    )}

                    <div className="dhdp-detail-item">
                      <span className="dhdp-detail-label">Payment Method</span>
                      <span className="dhdp-detail-value">
                        {getPaymentMethodLabel(donation.paymentMethod)}
                        {donation.cardLastFour && (
                          <span className="dhdp-card-last4"> •••• {donation.cardLastFour}</span>
                        )}
                      </span>
                    </div>

                    {gatewayLabel && (
                      <div className="dhdp-detail-item">
                        <span className="dhdp-detail-label">Payment Gateway</span>
                        <span className="dhdp-detail-value">{gatewayLabel}</span>
                      </div>
                    )}

                    <div className="dhdp-detail-item">
                      <span className="dhdp-detail-label">Receipt Email</span>
                      <span className="dhdp-detail-value dhdp-flex-row">
                        {donation.receiptSent ? (
                          <>
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" style={{ color: 'var(--c4k-success)', flexShrink: 0 }}>
                              <polyline points="20 6 9 17 4 12" />
                            </svg>
                            Sent
                          </>
                        ) : (
                          <>
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" style={{ color: 'var(--c4k-gray-400)', flexShrink: 0 }}>
                              <circle cx="12" cy="12" r="10" />
                              <line x1="15" y1="9" x2="9" y2="15" />
                              <line x1="9" y1="9" x2="15" y2="15" />
                            </svg>
                            Not sent yet
                          </>
                        )}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Cause / Campaign */}
              {(donation.causeName || donation.campaignName) && (
                <div className="dhdp-card">
                  <div className="dhdp-card-header">
                    <div className="dhdp-card-icon dhdp-card-icon-teal">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
                      </svg>
                    </div>
                    <h2 className="dhdp-card-title">Cause &amp; Campaign</h2>
                  </div>
                  <div className="dhdp-card-body">
                    <div className="dhdp-detail-grid">
                      {donation.causeName && (
                        <div className="dhdp-detail-item">
                          <span className="dhdp-detail-label">Cause</span>
                          <span className="dhdp-detail-value">{donation.causeName}</span>
                        </div>
                      )}
                      {donation.campaignId && donation.campaignName && (
                        <div className="dhdp-detail-item">
                          <span className="dhdp-detail-label">Campaign</span>
                          <span className="dhdp-detail-value">
                            {donation.campaignName}
                            <Link
                              to={`/campaigns/${donation.campaignId}`}
                              className="dhdp-link"
                              aria-label={`View campaign: ${donation.campaignName}`}
                            >
                              View campaign →
                            </Link>
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* Donor Message */}
              {donation.message && (
                <div className="dhdp-card dhdp-card-message">
                  <div className="dhdp-card-header">
                    <div className="dhdp-card-icon dhdp-card-icon-coral">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
                      </svg>
                    </div>
                    <h2 className="dhdp-card-title">Your Message</h2>
                  </div>
                  <div className="dhdp-card-body">
                    <blockquote className="dhdp-message">
                      {donation.message}
                    </blockquote>
                    {donation.isAnonymous && (
                      <p className="dhdp-anon-note">
                        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                          <circle cx="12" cy="7" r="4" />
                        </svg>
                        This donation is displayed anonymously.
                      </p>
                    )}
                  </div>
                </div>
              )}

            </Col>

            {/* ─── Right Column ─── */}
            <Col lg={4}>

              {/* Actions */}
              <div className="dhdp-card dhdp-card-sticky">
                <div className="dhdp-card-header">
                  <div className="dhdp-card-icon dhdp-card-icon-teal">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                      <polyline points="6 9 6 2 18 2 18 9" />
                      <path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" />
                      <rect x="6" y="14" width="12" height="8" />
                    </svg>
                  </div>
                  <h2 className="dhdp-card-title">Actions</h2>
                </div>
                <div className="dhdp-card-body dhdp-card-body-tight">
                  <button
                    className="btn-coral dhdp-btn-full"
                    onClick={() => navigate(`/donation-receipt/${donation.donationId}`)}
                    aria-label="View donation receipt"
                  >
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                      <polyline points="14 2 14 8 20 8" />
                      <line x1="16" y1="13" x2="8" y2="13" />
                      <line x1="16" y1="17" x2="8" y2="17" />
                      <polyline points="10 9 9 9 8 9" />
                    </svg>
                    View Receipt
                  </button>

                  <button
                    className="btn-outline dhdp-btn-full"
                    onClick={() => window.print()}
                    aria-label="Print this page"
                  >
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <polyline points="6 9 6 2 18 2 18 9" />
                      <path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" />
                      <rect x="6" y="14" width="12" height="8" />
                    </svg>
                    Print
                  </button>

                  <Link to="/my-donations" className="btn-link dhdp-btn-full dhdp-link-btn">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <line x1="19" y1="12" x2="5" y2="12" />
                      <polyline points="12 19 5 12 12 5" />
                    </svg>
                    Back to All Donations
                  </Link>
                </div>
              </div>

              {/* Need Help */}
              <div className="dhdp-help-card">
                <div className="dhdp-help-icon">
                  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                    <circle cx="12" cy="12" r="10" />
                    <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" />
                    <line x1="12" y1="17" x2="12.01" y2="17" />
                  </svg>
                </div>
                <div className="dhdp-help-body">
                  <h4 className="dhdp-help-title">Need Help?</h4>
                  <p className="dhdp-help-text">
                    Have questions about this donation or want to request a refund?
                  </p>
                  <Link to="/raise-query" className="dhdp-help-link">
                    Contact Support →
                  </Link>
                </div>
              </div>

              {/* Related Campaigns */}
              {donation.campaignId && (
                <div className="dhdp-card">
                  <div className="dhdp-card-header">
                    <div className="dhdp-card-icon">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                        <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
                      </svg>
                    </div>
                    <h2 className="dhdp-card-title">Support More</h2>
                  </div>
                  <div className="dhdp-card-body dhdp-card-body-tight">
                    <Link to="/campaigns" className="btn-outline dhdp-btn-full">
                      Browse Campaigns
                    </Link>
                    <Link to="/donate" className="btn-link dhdp-btn-full dhdp-link-btn">
                      Make Another Donation →
                    </Link>
                  </div>
                </div>
              )}

            </Col>
          </Row>
        </Container>
      </section>

    </div>
  );
};

export default DonationHistoryDetailPage;
