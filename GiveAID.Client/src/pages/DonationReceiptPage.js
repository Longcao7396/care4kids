import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Spinner, Alert } from 'react-bootstrap';
import { useParams, useLocation, useNavigate, Link } from 'react-router-dom';
import { donationsService } from '../services';
import './DonationReceiptPage.css';

/* ============================================================
   DONATION RECEIPT PAGE
   Shown after a successful donation — confirms, thanks, receipt.
   Receives donation data via location.state (from DonatePage) or
   fetches it via API if navigated directly.
   ============================================================ */

const DonationReceiptPage = () => {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();

  const [donation, setDonation] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Priority 1: data passed via navigate state (from DonatePage)
  // Priority 2: fetch from API using the id from the URL
  useEffect(() => {
    const ac = new AbortController();

    const loadDonation = async () => {
      // Priority 1: use passed state data
      if (location.state?.donationData) {
        setDonation(location.state.donationData);
        setLoading(false);
        return;
      }

      // Priority 2: fetch from API
      try {
        const response = await donationsService.getById(id);
        if (response.success && response.data) {
          setDonation(response.data);
        } else {
          setError(response.message || 'Donation not found.');
        }
      } catch (err) {
        console.error('Error loading donation receipt:', err);
        setError('Failed to load donation details. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    loadDonation();
    return () => ac.abort();
  }, [id, location.state]);

  // Auto-redirect to login if the receipt page was accessed without auth
  // (the ProtectedRoute wrapper should handle this, but as a belt-and-suspenders check)
  useEffect(() => {
    if (!loading && !donation && !error) {
      // Nothing to do — the protected route guard already handles auth
    }
  }, [loading, donation, error]);

  const formatCurrency = (amount) =>
    new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0,
    }).format(amount || 0);

  const formatDate = (dateStr) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusBadge = (status) => {
    const map = {
      Completed: 'drp-badge-success',
      Pending: 'drp-badge-warning',
      Failed: 'drp-badge-error',
      Refunded: 'drp-badge-neutral',
    };
    return map[status] || 'drp-badge-neutral';
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
    };
    return map[method] || method || '—';
  };

  // Confetti-style celebration for completed donations
  const isCompleted = donation?.paymentStatus === 'Completed';

  // ── Loading ──────────────────────────────────────────────────────────────
  if (loading) {
    return (
      <div className="drp-page">
        <div className="drp-loading">
          <Spinner animation="border" style={{ color: 'var(--c4k-teal)' }} />
          <p>Loading your receipt...</p>
        </div>
      </div>
    );
  }

  // ── Error ────────────────────────────────────────────────────────────────
  if (error) {
    return (
      <div className="drp-page">
        <section className="drp-hero drp-hero-sm">
          <Container>
            <p className="eyebrow">Receipt</p>
            <h1 className="drp-hero-title">Donation Receipt</h1>
          </Container>
        </section>
        <Container className="drp-content">
          <Alert variant="danger" className="drp-alert">
            <strong>Unable to load receipt.</strong> {error}
          </Alert>
          <div className="drp-actions">
            <button className="btn-coral" onClick={() => navigate('/my-donations')}>
              View My Donations
            </button>
            <Link to="/donate" className="btn-outline">
              Make Another Donation
            </Link>
          </div>
        </Container>
      </div>
    );
  }

  // ── Receipt ─────────────────────────────────────────────────────────────
  return (
    <div className="drp-page">

      {/* ─── HERO ─── */}
      <section className={`drp-hero ${isCompleted ? 'drp-hero-success' : 'drp-hero-pending'}`}>
        <div className="drp-hero-bg">
          <div className="drp-hero-pattern" />
        </div>
        <Container className="drp-hero-content">
          {isCompleted ? (
            <>
              <div className="drp-check-icon">
                <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
              </div>
              <p className="eyebrow" style={{ color: 'rgba(255,255,255,0.7)' }}>Payment Successful</p>
              <h1 className="drp-hero-title">Thank You!</h1>
              <p className="drp-hero-sub">
                Your generous donation of{' '}
                <strong style={{ color: '#FAE6DD' }}>{formatCurrency(donation.amount)}</strong>{' '}
                has been received. A receipt has been sent to your email.
              </p>
            </>
          ) : (
            <>
              <div className="drp-check-icon drp-check-icon-pending">
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                  <circle cx="12" cy="12" r="10" />
                  <polyline points="12 6 12 12 16 14" />
                </svg>
              </div>
              <p className="eyebrow" style={{ color: 'rgba(255,255,255,0.7)' }}>Processing</p>
              <h1 className="drp-hero-title">Payment {donation.paymentStatus}</h1>
              <p className="drp-hero-sub">
                Your donation of{' '}
                <strong style={{ color: '#FAE6DD' }}>{formatCurrency(donation.amount)}</strong>{' '}
                is currently being processed. You will receive a confirmation email once complete.
              </p>
            </>
          )}
        </Container>
      </section>

      {/* ─── RECEIPT CARD ─── */}
      <section className="drp-receipt-section">
        <Container>
          <Row className="justify-content-center">
            <Col lg={8}>

              <div className="drp-receipt-card">
                {/* Receipt Header */}
                <div className="drp-receipt-header">
                  <div className="drp-receipt-logo">
                    <img
                      src="/assets/logo-teal.svg"
                      alt="Care4Kids"
                      className="drp-logo-img"
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }}
                    />
                    <span className="drp-logo-text">Care4Kids</span>
                  </div>
                  <div className="drp-receipt-title-wrap">
                    <h2 className="drp-receipt-title">Donation Receipt</h2>
                    <span className={`drp-status-badge ${getStatusBadge(donation.paymentStatus)}`}>
                      {donation.paymentStatus || 'Unknown'}
                    </span>
                  </div>
                </div>

                {/* Receipt Details */}
                <div className="drp-receipt-body">
                  <div className="drp-receipt-id-row">
                    <div className="drp-receipt-id">
                      <span className="drp-receipt-id-label">Receipt No.</span>
                      <span className="drp-receipt-id-value">
                        {donation.donationId ? `#DRP-${String(donation.donationId).padStart(5, '0')}` : '—'}
                      </span>
                    </div>
                    <div className="drp-receipt-date">
                      <span className="drp-receipt-id-label">Date</span>
                      <span className="drp-receipt-id-value">{formatDate(donation.donationDate)}</span>
                    </div>
                  </div>

                  <hr className="drp-divider" />

                  {/* Amount */}
                  <div className="drp-amount-section">
                    <span className="drp-amount-label">Donation Amount</span>
                    <span className="drp-amount-value">{formatCurrency(donation.amount)}</span>
                  </div>

                  <hr className="drp-divider" />

                  {/* Info Grid */}
                  <div className="drp-info-grid">
                    <div className="drp-info-item">
                      <span className="drp-info-label">Cause</span>
                      <span className="drp-info-value">
                        {donation.causeName || 'General Fund'}
                      </span>
                    </div>

                    {donation.campaignName && (
                      <div className="drp-info-item">
                        <span className="drp-info-label">Campaign</span>
                        <span className="drp-info-value">{donation.campaignName}</span>
                      </div>
                    )}

                    <div className="drp-info-item">
                      <span className="drp-info-label">Payment Method</span>
                      <span className="drp-info-value">
                        {getPaymentMethodLabel(donation.paymentMethod)}
                        {donation.cardLastFour && (
                          <span className="drp-card-last4"> •••• {donation.cardLastFour}</span>
                        )}
                      </span>
                    </div>

                    {donation.transactionId && (
                      <div className="drp-info-item">
                        <span className="drp-info-label">Transaction ID</span>
                        <span className="drp-info-value drp-txn-id">{donation.transactionId}</span>
                      </div>
                    )}

                    {donation.receiptSent && (
                      <div className="drp-info-item">
                        <span className="drp-info-label">Receipt Email</span>
                        <span className="drp-info-value">
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <polyline points="20 6 9 17 4 12" />
                          </svg>
                          Sent to your email
                        </span>
                      </div>
                    )}
                  </div>

                  {/* Donor Message */}
                  {donation.message && (
                    <>
                      <hr className="drp-divider" />
                      <div className="drp-message-section">
                        <span className="drp-info-label">Your Message</span>
                        <blockquote className="drp-message">
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
                          </svg>
                          {donation.message}
                        </blockquote>
                      </div>
                    </>
                  )}

                  {/* Anonymous Notice */}
                  {donation.isAnonymous && (
                    <>
                      <hr className="drp-divider" />
                      <div className="drp-anon-notice">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                          <circle cx="12" cy="7" r="4" />
                        </svg>
                        This donation is displayed anonymously.
                      </div>
                    </>
                  )}
                </div>

                {/* Receipt Footer */}
                <div className="drp-receipt-footer">
                  <div className="drp-footer-note">
                    <p>
                      Care4Kids is a registered non-profit organization. Your donation is
                      tax-deductible to the extent permitted by law. For any questions about
                      this receipt, please contact us at{' '}
                      <a href="mailto:donations@care4kids.org">donations@care4kids.org</a>.
                    </p>
                  </div>
                </div>
              </div>

              {/* ─── ACTIONS ─── */}
              <div className="drp-actions">
                <button
                  className="btn-coral drp-btn-lg"
                  onClick={() => window.print()}
                  aria-label="Print or save receipt"
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <polyline points="6 9 6 2 18 2 18 9" />
                    <path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" />
                    <rect x="6" y="14" width="12" height="8" />
                  </svg>
                  Print Receipt
                </button>
                <Link to="/my-donations" className="btn-outline drp-btn-lg">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <line x1="19" y1="12" x2="5" y2="12" />
                    <polyline points="12 19 5 12 12 5" />
                  </svg>
                  View My Donations
                </Link>
                <Link to="/" className="btn-link">
                  Back to Home
                </Link>
              </div>

              {/* ─── SHARE ─── */}
              <div className="drp-share-section">
                <p className="drp-share-label">Share your impact</p>
                <div className="drp-share-btns">
                  <a
                    href={`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(window.location.origin)}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="drp-share-btn drp-share-fb"
                    aria-label="Share on Facebook"
                  >
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z" />
                    </svg>
                    Facebook
                  </a>
                  <a
                    href={`https://twitter.com/intent/tweet?text=${encodeURIComponent(`I just donated to Care4Kids! Every child deserves a chance. #Care4Kids #GiveBack`)}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="drp-share-btn drp-share-tw"
                    aria-label="Share on Twitter"
                  >
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M23 3a10.9 10.9 0 0 1-3.14 1.53 4.48 4.48 0 0 0-7.86 3v1A10.66 10.66 0 0 1 3 4s-4 9 5 13a11.64 11.64 0 0 1-7 2c9 5 20 0 20-11.5a4.5 4.5 0 0 0-.08-.83A7.72 7.72 0 0 0 23 3z" />
                    </svg>
                    Twitter / X
                  </a>
                  <button
                    className="drp-share-btn drp-share-copy"
                    onClick={() => {
                      navigator.clipboard.writeText(window.location.href).catch(() => {});
                    }}
                    aria-label="Copy link"
                  >
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71" />
                      <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71" />
                    </svg>
                    Copy Link
                  </button>
                </div>
              </div>

            </Col>
          </Row>
        </Container>
      </section>

      {/* ─── IMPACT SECTION ─── */}
      {isCompleted && donation.amount && (
        <section className="drp-impact-section">
          <Container>
            <Row className="justify-content-center">
              <Col lg={8}>
                <div className="drp-impact-card">
                  <h3 className="drp-impact-title">Your Donation at Work</h3>
                  <p className="drp-impact-desc">
                    Here's an estimate of the impact your donation will create:
                  </p>
                  <div className="drp-impact-grid">
                    {Math.floor(donation.amount / 25000) > 0 && (
                      <div className="drp-impact-item">
                        <div className="drp-impact-icon">
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                            <path d="M18 8h1a4 4 0 0 1 0 8h-1" />
                            <path d="M2 8h16v9a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8z" />
                            <line x1="6" y1="1" x2="6" y2="4" />
                            <line x1="10" y1="1" x2="10" y2="4" />
                            <line x1="14" y1="1" x2="14" y2="4" />
                          </svg>
                        </div>
                        <div className="drp-impact-num">
                          {Math.floor(donation.amount / 25000).toLocaleString('vi-VN')}
                        </div>
                        <div className="drp-impact-lbl">Nutritious meals provided</div>
                      </div>
                    )}
                    {Math.floor(donation.amount / 150000) > 0 && (
                      <div className="drp-impact-item">
                        <div className="drp-impact-icon">
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                            <path d="M20 7h-4l-2-3H10L8 7H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z" />
                          </svg>
                        </div>
                        <div className="drp-impact-num">
                          {Math.floor(donation.amount / 150000)}
                        </div>
                        <div className="drp-impact-lbl">Supply packs delivered</div>
                      </div>
                    )}
                    {Math.floor(donation.amount / 500000) > 0 && (
                      <div className="drp-impact-item">
                        <div className="drp-impact-icon">
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
                            <path d="M22 12h-4l-3 9L9 3l-3 9H2" />
                          </svg>
                        </div>
                        <div className="drp-impact-num">
                          {Math.floor(donation.amount / 500000)}
                        </div>
                        <div className="drp-impact-lbl">Health check-ups supported</div>
                      </div>
                    )}
                  </div>
                  <p className="drp-impact-note">
                    Estimates based on average programme costs. Actual impact varies.
                  </p>
                  <Link to="/campaigns" className="drp-explore-link">
                    Explore more campaigns →
                  </Link>
                </div>
              </Col>
            </Row>
          </Container>
        </section>
      )}

    </div>
  );
};

export default DonationReceiptPage;
