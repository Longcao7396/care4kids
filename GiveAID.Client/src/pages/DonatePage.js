import React, { useState, useEffect, useMemo, useRef } from 'react';
import { Container, Row, Col, Form, Alert } from 'react-bootstrap';
import { useNavigate, useLocation, useSearchParams } from 'react-router-dom';
import { causesService, donationsService } from '../services';
import { createCardElement, confirmCardPayment, destroyCardElement } from '../services/stripeService';
import api from '../services/api';
import './DonatePage.css';

/* ============================================================
// Card validation utilities (client-side only — PCI-DSS safe)
// ============================================================ */

/**
 * Luhn algorithm — validates card number structure.
 * Does NOT transmit card data; only validates locally.
 */
function validateLuhn(cardNumber) {
  const digits = cardNumber.replace(/\D/g, '');
  if (!digits) return false;
  let sum = 0;
  let isEven = false;
  for (let i = digits.length - 1; i >= 0; i--) {
    let d = parseInt(digits[i], 10);
    if (isEven) { d *= 2; if (d > 9) d -= 9; }
    sum += d;
    isEven = !isEven;
  }
  return sum % 10 === 0;
}

/**
 * Returns the card brand based on number prefix.
 */
function getCardBrand(cardNumber) {
  const digits = cardNumber.replace(/\D/g, '');
  if (/^4/.test(digits)) return 'Visa';
  if (/^5[1-5]/.test(digits) || /^2[2-7]/.test(digits)) return 'Mastercard';
  if (/^3[47]/.test(digits)) return 'Amex';
  if (/^6(?:011|5)/.test(digits)) return 'Discover';
  return null;
}

/**
 * Validates card expiry string (MM/YY).
 * Returns { valid, expired, month, year }.
 */
function validateExpiry(expiryStr) {
  const match = expiryStr.match(/^(\d{2})\/(\d{2})$/);
  if (!match) return { valid: false, expired: false, month: null, year: null };
  const month = parseInt(match[1], 10);
  const year = 2000 + parseInt(match[2], 10);
  if (month < 1 || month > 12) return { valid: false, expired: false, month, year };
  const expDate = new Date(year, month);
  return { valid: true, expired: expDate <= new Date(), month, year };
}

/**
 * Validates CVV — 3 digits for most cards, 4 for Amex.
 */
function validateCvv(cvv, isAmex) {
  const digits = cvv.replace(/\D/g, '');
  if (isAmex) return digits.length === 4;
  return digits.length >= 3 && digits.length <= 4;
}

/* ============================================================
// DonatePage
// ============================================================ */

const DonatePage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const [causes, setCauses] = useState([]);
  const [causeTree, setCauseTree] = useState([]);
  const [campaigns, setCampaigns] = useState([]);
  const [subCauses, setSubCauses] = useState([]);
  
  // URL params campaign state
  const [urlCampaign, setUrlCampaign] = useState(null);
  const [isFromCampaignLink, setIsFromCampaignLink] = useState(false);
  
  const [formData, setFormData] = useState({
    causeId: location.state?.causeId || '',
    campaignId: location.state?.campaignId || '',
    amount: '500000',
    paymentMethod: 'BankTransfer',
    // Card fields — PCI-DSS: never sent to server
    cardNumber: '',
    cardHolderName: '',
    expiryDate: '',
    cvv: '',
    // ──
    message: '',
    isAnonymous: false
  });
  const [cardErrors, setCardErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // ── Stripe state ─────────────────────────────────────────────────────────────
  // Set when the backend returns a clientSecret (Stripe gateway is active).
  const [stripeState, setStripeState] = useState({
    clientSecret: null,           // pi_xxx_secret_xxx from backend
    stripePublishableKey: null,   // pk_test_... from backend
    stripe: null,                 // Stripe instance
    cardElement: null,            // Stripe card Element
    paymentProcessing: false,     // true while Stripe.confirmCardPayment runs
    paymentError: null,           // error message from Stripe
  });
  const [isMockGateway, setIsMockGateway] = useState(false); // true when using mock

  // Refs for Stripe card element mount point
  const stripeCardMountRef = useRef(null);

  // Card brand auto-detection
  const cardBrand = useMemo(() => getCardBrand(formData.cardNumber), [formData.cardNumber]);
  const showCardForm = formData.paymentMethod === 'CreditCard' || formData.paymentMethod === 'DebitCard';

  // Auto-format card number: 1234 5678 9012 3456
  const handleCardNumberChange = (e) => {
    const digits = e.target.value.replace(/\D/g, '').slice(0, 16);
    const formatted = digits.replace(/(.{4})/g, '$1 ').trim();
    setFormData((prev) => ({ ...prev, cardNumber: formatted }));
    if (cardErrors.cardNumber) setCardErrors((p) => ({ ...p, cardNumber: '' }));
  };

  // Auto-format expiry: MM/YY
  const handleExpiryChange = (e) => {
    let raw = e.target.value.replace(/\D/g, '').slice(0, 4);
    if (raw.length >= 3) raw = raw.slice(0, 2) + '/' + raw.slice(2);
    else if (raw.length === 2) raw = raw + '/';
    setFormData((prev) => ({ ...prev, expiryDate: raw }));
    if (cardErrors.expiryDate) setCardErrors((p) => ({ ...p, expiryDate: '' }));
  };

  /* ── Card field validation ────────────────────────────── */
  const validateCardFields = () => {
    const errs = {};
    const { cardNumber, cardHolderName, expiryDate, cvv, paymentMethod } = formData;

    if (paymentMethod === 'CreditCard' || paymentMethod === 'DebitCard') {
      if (!cardHolderName.trim() || cardHolderName.trim().length < 2) {
        errs.cardHolderName = 'Enter the name as shown on your card.';
      }
      const rawCard = cardNumber.replace(/\s/g, '');
      if (!rawCard) {
        errs.cardNumber = 'Card number is required.';
      } else if (rawCard.length < 13 || rawCard.length > 19) {
        errs.cardNumber = 'Enter a valid card number.';
      } else if (!validateLuhn(rawCard)) {
        errs.cardNumber = 'Card number is invalid (check for typos).';
      }
      if (!expiryDate) {
        errs.expiryDate = 'Expiry date is required.';
      } else {
        const exp = validateExpiry(expiryDate);
        if (!exp.valid) errs.expiryDate = 'Enter expiry as MM/YY.';
        else if (exp.expired) errs.expiryDate = 'Your card has expired.';
      }
      if (!cvv) {
        errs.cvv = 'CVV is required.';
      } else if (!validateCvv(cvv, cardBrand === 'Amex')) {
        errs.cvv = cardBrand === 'Amex' ? 'Amex CVV is 4 digits.' : 'CVV must be 3-4 digits.';
      }
    }
    return errs;
  };

  useEffect(() => {
    const ac = new AbortController();
    const loadTree = async () => {
      try {
        const treeData = await api.get('/causes/tree', {
          params: { activeOnly: true }, signal: ac.signal
        });
        // interceptor unwraps envelope → treeData is the array directly
        if (Array.isArray(treeData) && treeData.length > 0) {
          setCauseTree(treeData);
          const flat = [];
          treeData.forEach((node) => {
            flat.push(node.parent);
            (node.subCauses || []).forEach((s) => flat.push(s));
          });
          setCauses(flat);
        } else {
          const r = await causesService.getAll(true, { signal: ac.signal });
          if (Array.isArray(r) && r.length > 0) setCauses(r);
        }
      } catch (e) {
        if (e.name !== 'CanceledError') console.error(e);
      }
    };
    const loadCampaigns = async () => {
      try {
        const r = await api.get('/campaigns', {
          params: { status: 'Active' }, signal: ac.signal
        });
        // interceptor unwraps → r is array directly
        if (Array.isArray(r) && r.length > 0) setCampaigns(r);
      } catch (e) {
        if (e.name !== 'CanceledError') console.error(e);
      }
    };
    loadTree();
    loadCampaigns();
    return () => ac.abort();
  }, []);

  // Handle URL params for campaignId
  useEffect(() => {
    const campaignIdParam = searchParams.get('campaignId') || searchParams.get('campaign');
    
    if (campaignIdParam) {
      const loadCampaignFromUrl = async () => {
        try {
          const campaign = await api.get(`/campaigns/${campaignIdParam}`);
          // interceptor unwraps → campaign is the object directly
          if (campaign && typeof campaign === 'object' && campaign.campaignId) {
            setUrlCampaign(campaign);
            setIsFromCampaignLink(true);
            
            // Auto-select cause and campaign
            setFormData((prev) => ({
              ...prev,
              causeId: campaign.cause?.causeId || prev.causeId,
              campaignId: campaign.campaignId
            }));
          }
        } catch (err) {
          console.error('Failed to load campaign from URL:', err);
        }
      };
      loadCampaignFromUrl();
    }
  }, [searchParams]);

  useEffect(() => {
    if (formData.causeId) {
      loadCampaignsByCause(formData.causeId);
      const sel = causes.find((c) => String(c.causeId) === String(formData.causeId));
      const parentId = sel?.parentCauseId || sel?.causeId;
      const subs = causeTree.find((n) => n.parent.causeId === parentId)?.subCauses || [];
      setSubCauses(subs);
    } else {
      setSubCauses([]);
    }
  }, [formData.causeId, causeTree, causes]);

  // ── Destroy Stripe card element when payment method changes ─────────────────
  // This prevents stale iframes and race conditions when switching between
  // BankTransfer → CreditCard/DebitCard after a prior submission.
  useEffect(() => {
    if (stripeState.cardElement) {
      destroyCardElement(stripeState.cardElement);
      setStripeState((prev) => ({
        ...prev,
        stripe: null,
        cardElement: null,
        clientSecret: null,
        stripePublishableKey: null,
        paymentError: null,
      }));
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formData.paymentMethod]);

  // SECURITY/PCI-DSS: zero out sensitive form state (card number, CVV, expiry,
  // amount) when the component unmounts. Also destroy any Stripe card Element
  // to ensure the iframe is removed from the DOM.
  useEffect(() => {
    return () => {
      setFormData((prev) => ({
        ...prev,
        cardNumber: '',
        cardHolderName: '',
        expiryDate: '',
        cvv: '',
        amount: '',
      }));
      setCardErrors({});
      if (stripeState.cardElement) destroyCardElement(stripeState.cardElement);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadCampaignsByCause = async (causeId) => {
    try {
      const r = await api.get('/campaigns', { params: { status: 'Active', causeId } });
      // interceptor unwraps envelope → r is the array directly
      if (Array.isArray(r) && r.length > 0) setCampaigns(r);
    } catch (e) { console.error(e); }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
    if (cardErrors[name]) setCardErrors((p) => ({ ...p, [name]: '' }));
  };

  const handleQuickAmount = (amount) => {
    setFormData((prev) => ({ ...prev, amount: amount.toString() }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!formData.causeId) { setError('Please select a cause to support.'); return; }
    if (!formData.amount || parseFloat(formData.amount) <= 0) { setError('Please enter a valid donation amount.'); return; }

    const errs = validateCardFields();
    if (Object.keys(errs).length > 0) { setCardErrors(errs); setError('Please correct the payment details below.'); return; }

    setLoading(true);
    try {
      const idempotencyKey = `don-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;

      // ── Step 1: Create donation intent on backend ────────────────────────────
      const donationData = {
        causeId: parseInt(formData.causeId),
        campaignId: formData.campaignId ? parseInt(formData.campaignId) : null,
        amount: parseFloat(formData.amount),
        paymentMethod: formData.paymentMethod,
        message: formData.message || null,
        isAnonymous: formData.isAnonymous,
        idempotencyKey,
        // Optional: hint the gateway. Backend falls back to PaymentGateway__Default.
        paymentGateway: 'stripe',
      };

      const response = await donationsService.create(donationData);
      if (!response.success) {
        setError(response.message || 'Failed to create donation.');
        setLoading(false);
        return;
      }

      const { clientSecret, stripePublishableKey, paymentGateway } = response.data || {};

      // ── Step 2a: Mock gateway — skip Stripe.js ───────────────────────────────
      if (!clientSecret || paymentGateway === 'mock') {
        setIsMockGateway(true);
        const mockDonationId = response.data?.donationId || response.data?.donationId;
        setSuccess('Thank you for your generous donation (Demo Mode). You will receive a receipt via email.');
        setTimeout(() => navigate(`/donation-receipt/${mockDonationId}`, {
          state: { donationData: response.data }
        }), 2500);
        setLoading(false);
        return;
      }

      // ── Step 2b: Stripe gateway — use Stripe.js to confirm ──────────────────
      if (!stripePublishableKey) {
        setError('Stripe publishable key is missing. Please contact support.');
        setLoading(false);
        return;
      }

      // Mount Stripe card element dynamically
      const { stripe, cardElement } = await createCardElement(
        stripePublishableKey,
        stripeCardMountRef.current
      );

      setStripeState((prev) => ({
        ...prev,
        stripe,
        cardElement,
        clientSecret,
        stripePublishableKey,
        paymentProcessing: true,
      }));

      // Confirm the payment with Stripe
      const { error: stripeError, paymentIntent } = await confirmCardPayment(
        stripe,
        clientSecret,
        cardElement,
        {
          payment_method_data: {
            billing_details: {
              name: formData.cardHolderName || 'Anonymous Donor',
            },
          },
        }
      );

      setStripeState((prev) => ({ ...prev, paymentProcessing: false }));

      if (stripeError) {
        // Card was declined or user cancelled — show the error inline
        const friendly =
          stripeError.type === 'card_error'
            ? stripeError.message
            : 'Payment failed. Please check your card details and try again.';
        setCardErrors({ cvv: friendly }); // reuse existing error display
        setError(friendly);
        destroyCardElement(cardElement);
        setLoading(false);
        return;
      }

      if (paymentIntent && paymentIntent.status === 'succeeded') {
        // Payment confirmed by Stripe — webhook will update the donation to Completed.
        // Show success immediately so the user doesn't have to wait for the webhook.
        setSuccess(
          `Payment confirmed! Transaction ID: ${paymentIntent.id}. ` +
          'Thank you for your generous donation. You will receive a receipt via email.'
        );
        const donationId = response.data?.donationId;
        setTimeout(() => navigate(donationId ? `/donation-receipt/${donationId}` : '/my-donations', {
          state: { donationData: response.data }
        }), 3500);
      } else {
        // Payment is in a processing state (e.g. 3D Secure required)
        setSuccess(
          `Payment is being processed (${paymentIntent?.status}). ` +
          'You will receive a confirmation email once complete.'
        );
        const donationId = response.data?.donationId;
        setTimeout(() => navigate(donationId ? `/donation-receipt/${donationId}` : '/my-donations', {
          state: { donationData: response.data }
        }), 4000);
      }

      destroyCardElement(cardElement);
    } catch (err) {
      // Clean up Stripe element on any unexpected error
      if (stripeState.cardElement) destroyCardElement(stripeState.cardElement);
      setError(err.response?.data?.message || err.message || 'Donation failed. Please try again.');
    } finally {
      setLoading(false);
      setStripeState((prev) => ({ ...prev, paymentProcessing: false }));
    }
  };

  const formatCurrency = (amount) => new Intl.NumberFormat('vi-VN', {
    style: 'currency', currency: 'VND', maximumFractionDigits: 0,
  }).format(amount || 0);

  const quickAmounts = [100000, 250000, 500000, 1000000, 2500000, 5000000];
  const impactPreview = useMemo(() => {
    const a = parseFloat(formData.amount) || 0;
    if (a <= 0) return null;
    return { meals: Math.floor(a / 25000), supplies: Math.floor(a / 150000), healthcare: Math.floor(a / 500000) };
  }, [formData.amount]);

  return (
    <div className="dp-page">

      {/* ─── HERO ─── */}
      <section className="dp-hero">
        <div className="dp-hero-bg">
          <img
            src="https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=1600&q=80"
            alt="Helping hands"
          />
          <div className="dp-hero-overlay" />
        </div>
        <Container className="dp-hero-content">
          <p className="eyebrow" style={{ color: '#FAE6DD' }}>Make a Difference</p>
          <h1 className="dp-hero-title">Your Donation<br />Changes Lives</h1>
          <p className="dp-hero-sub">
            100% of your donation goes directly to programmes supporting children's welfare — food, education, healthcare and safe homes.
          </p>
        </Container>
      </section>

      {/* ─── FORM ─── */}
      <section className="dp-form-section">
        <Container>
          {/* Campaign indicator when coming from campaign link */}
          {isFromCampaignLink && urlCampaign && (
            <div className="dp-campaign-indicator mb-4">
              <div className="dp-campaign-indicator-inner">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>
                </svg>
                <span>Donating to: <strong>{urlCampaign.campaignName}</strong></span>
                <button 
                  type="button" 
                  className="dp-campaign-indicator-close"
                  onClick={() => {
                    setIsFromCampaignLink(false);
                    setUrlCampaign(null);
                    setFormData((prev) => ({ ...prev, campaignId: '' }));
                  }}
                  title="Choose a different campaign"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <line x1="18" y1="6" x2="6" y2="18"/>
                    <line x1="6" y1="6" x2="18" y2="18"/>
                  </svg>
                </button>
              </div>
            </div>
          )}

          <Row className="dp-form-row">

            {/* Left — Form */}
            <Col lg={7} className="dp-form-col">
              <div className="dp-form-card">
                {error && <Alert variant="danger" className="dp-alert">{error}</Alert>}
                {success && <Alert variant="success" className="dp-alert">{success}</Alert>}

                <Form onSubmit={handleSubmit}>

                  {/* Step 1 — Cause */}
                  <div className="dp-step">
                    <p className="dp-step-num">Step 1</p>
                    <h3 className="dp-step-title">Choose a Cause</h3>
                    <p className="dp-step-desc">Select the area you'd like your donation to support.</p>
                    <Form.Select
                      name="parentCauseId"
                      value={
                        causeTree.find((n) =>
                          n.parent.causeId === formData.causeId ||
                          (n.subCauses || []).some((s) => s.causeId === formData.causeId)
                        )?.parent.causeId || ''
                      }
                      onChange={(e) => {
                        const parent = causeTree.find((n) => n.parent.causeId === Number(e.target.value))?.parent;
                        setFormData((p) => ({ ...p, causeId: parent?.causeId || '', campaignId: '' }));
                      }}
                      required
                      disabled={isFromCampaignLink}
                      className="dp-select"
                    >
                      <option value="">Choose a cause...</option>
                      {causeTree.map((node) => (
                        <option key={node.parent.causeId} value={node.parent.causeId}>
                          {node.parent.causeName}
                        </option>
                      ))}
                    </Form.Select>
                  </div>

                  {/* Step 1b — Sub-cause */}
                  {subCauses.length > 0 && (
                    <div className="dp-step dp-step-sub">
                      <p className="dp-step-num">Step 1b · Optional</p>
                      <h3 className="dp-step-title">Specific Need</h3>
                      <p className="dp-step-desc">Pick a specific area, or leave blank to support the whole cause.</p>
                      <Form.Select name="causeId" value={formData.causeId} onChange={handleChange} className="dp-select">
                        <option value="">Support the whole cause</option>
                        {subCauses.map((sub) => (
                          <option key={sub.causeId} value={sub.causeId}>{sub.causeName}</option>
                        ))}
                      </Form.Select>
                    </div>
                  )}

                  {/* Step 2 — Campaign */}
                  {campaigns.length > 0 && formData.causeId && (
                    <div className="dp-step">
                      <p className="dp-step-num">Step 2 · Optional</p>
                      <h3 className="dp-step-title">Select a Campaign</h3>
                      <p className="dp-step-desc">Choose a specific campaign, or leave blank for a general donation.</p>
                      <Form.Select name="campaignId" value={formData.campaignId} onChange={handleChange} disabled={isFromCampaignLink} className="dp-select">
                        <option value="">General donation to this cause</option>
                        {campaigns.map((c) => (
                          <option key={c.campaignId} value={c.campaignId}>
                            {c.campaignName} — {c.percentageReached?.toFixed(0) || 0}% funded
                          </option>
                        ))}
                      </Form.Select>
                    </div>
                  )}

                  {/* Step 3 — Amount */}
                  <div className="dp-step">
                    <p className="dp-step-num">{campaigns.length > 0 && formData.causeId ? 'Step 3' : 'Step 2'}</p>
                    <h3 className="dp-step-title">Donation Amount</h3>
                    <p className="dp-step-desc">Choose a preset amount or enter your own.</p>
                    <div className="dp-quick-amounts">
                      {quickAmounts.map((a) => (
                        <button
                          type="button" key={a}
                          className={`dp-quick-btn ${formData.amount === a.toString() ? 'is-active' : ''}`}
                          onClick={() => handleQuickAmount(a)}
                        >
                          {formatCurrency(a)}
                        </button>
                      ))}
                    </div>
                    <Form.Group className="mt-3">
                      <Form.Label className="dp-label">Custom amount (VND)</Form.Label>
                      <Form.Control
                        type="number" name="amount"
                        placeholder="Enter any amount"
                        value={formData.amount}
                        onChange={handleChange}
                        min="1000" step="1000"
                        required className="dp-input dp-input-large"
                      />
                    </Form.Group>
                  </div>

                  {/* Step 4 — Payment Method + Card */}
                  <div className="dp-step">
                    <p className="dp-step-num">Payment Information</p>
                    <h3 className="dp-step-title">Choose Payment Method</h3>
                    <Form.Select name="paymentMethod" value={formData.paymentMethod} onChange={handleChange} className="dp-select">
                      <option value="BankTransfer">Bank Transfer (Recommended)</option>
                      <option value="NetBanking">Net Banking</option>
                      <option value="CreditCard">Credit Card</option>
                      <option value="DebitCard">Debit Card</option>
                    </Form.Select>
                    <Form.Text className="text-muted">
                      Card details are validated locally and never transmitted or stored.
                    </Form.Text>

                    {/* ── Card form (only for Credit/Debit) ── */}
                    {showCardForm && (
                      <div className="dp-card-form">
                        <div className="dp-card-form-header">
                          <span className="dp-card-form-title">Card Details</span>
                          {stripeState.stripe ? (
                            // ── Stripe Elements mode ──────────────────────────────────
                            <div className="dp-secured-stripe">
                              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                                <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                              </svg>
                              <span>Secured by</span>
                              <strong>Stripe</strong>
                            </div>
                          ) : (
                            <div className="dp-card-logos">
                              <span className="dp-card-logo">Visa</span>
                              <span className="dp-card-logo">MC</span>
                              <span className="dp-card-logo">Amex</span>
                              <span className="dp-card-logo">Disc</span>
                            </div>
                          )}
                        </div>

                        {/* ── Mock gateway banner ── */}
                        {isMockGateway && (
                          <div className="dp-demo-banner">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                              <circle cx="12" cy="12" r="10"/>
                              <line x1="12" y1="8" x2="12" y2="12"/>
                              <line x1="12" y1="16" x2="12.01" y2="16"/>
                            </svg>
                            <span>Demo Mode — no real payment is processed. Set <code>PaymentGateway__Default=stripe</code> in Web.config for live payments.</span>
                          </div>
                        )}

                        {/* ── Stripe Elements card input (replaces raw card fields) ── */}
                        {stripeState.stripe ? (
                          <>
                            <div className="dp-stripe-card-wrap">
                              {/* Stripe mounts the card iframe here */}
                              <div
                                id="stripe-card-element"
                                ref={stripeCardMountRef}
                                className="dp-stripe-card-element"
                              />
                            </div>
                            {/* Cardholder name (still needed for billing_details) */}
                            <Form.Group className="mb-3 mt-3">
                              <Form.Label className="dp-label">Cardholder Name</Form.Label>
                              <Form.Control
                                type="text"
                                placeholder="NGUYEN VAN A"
                                value={formData.cardHolderName}
                                onChange={handleChange}
                                name="cardHolderName"
                                className="dp-input"
                                autoComplete="cc-name"
                              />
                            </Form.Group>
                            {/* Stripe inline error */}
                            {stripeState.paymentError && (
                              <div className="dp-field-error">{stripeState.paymentError}</div>
                            )}
                            {stripeState.paymentProcessing && (
                              <div className="dp-stripe-processing">
                                <span className="spinner-border spinner-border-sm me-2"></span>
                                Confirming payment with Stripe...
                              </div>
                            )}
                          </>
                        ) : (
                          <>
                            {/* ── Raw card fields (non-Stripe gateways / fallback) ── */}

                            {/* Card number */}
                            <Form.Group className="mb-3">
                              <Form.Label className="dp-label">Card Number</Form.Label>
                              <div className="dp-card-input-wrap">
                                <Form.Control
                                  type="text" inputMode="numeric"
                                  placeholder="1234 5678 9012 3456"
                                  value={formData.cardNumber}
                                  onChange={handleCardNumberChange}
                                  className={`dp-input ${cardErrors.cardNumber ? 'is-invalid' : ''}`}
                                  maxLength={19} autoComplete="cc-number"
                                />
                                {cardBrand && <span className="dp-card-brand-tag">{cardBrand}</span>}
                              </div>
                              {cardErrors.cardNumber && <div className="dp-field-error">{cardErrors.cardNumber}</div>}
                              {formData.cardNumber && !cardErrors.cardNumber && (
                                <div className="dp-field-ok">Valid format</div>
                              )}
                            </Form.Group>

                            {/* Cardholder name */}
                            <Form.Group className="mb-3">
                              <Form.Label className="dp-label">Cardholder Name</Form.Label>
                              <Form.Control
                                type="text" placeholder="NGUYEN VAN A"
                                value={formData.cardHolderName}
                                onChange={handleChange}
                                name="cardHolderName"
                                className={`dp-input ${cardErrors.cardHolderName ? 'is-invalid' : ''}`}
                                autoComplete="cc-name"
                              />
                              {cardErrors.cardHolderName && <div className="dp-field-error">{cardErrors.cardHolderName}</div>}
                            </Form.Group>

                            <Row>
                              {/* Expiry */}
                              <Col md={6}>
                                <Form.Group className="mb-3">
                                  <Form.Label className="dp-label">Expiry Date (MM/YY)</Form.Label>
                                  <Form.Control
                                    type="text" inputMode="numeric" placeholder="MM/YY"
                                    value={formData.expiryDate}
                                    onChange={handleExpiryChange}
                                    className={`dp-input ${cardErrors.expiryDate ? 'is-invalid' : ''}`}
                                    maxLength={5} autoComplete="cc-exp"
                                  />
                                  {cardErrors.expiryDate && <div className="dp-field-error">{cardErrors.expiryDate}</div>}
                                </Form.Group>
                              </Col>
                              {/* CVV */}
                              <Col md={6}>
                                <Form.Group className="mb-3">
                                  <Form.Label className="dp-label">
                                    CVV {cardBrand === 'Amex' ? '(4 digits)' : '(3 digits)'}
                                  </Form.Label>
                                  <Form.Control
                                    type="password" inputMode="numeric"
                                    placeholder={cardBrand === 'Amex' ? '1234' : '123'}
                                    value={formData.cvv}
                                    onChange={handleChange}
                                    name="cvv"
                                    className={`dp-input ${cardErrors.cvv ? 'is-invalid' : ''}`}
                                    maxLength={4} autoComplete="cc-csc"
                                  />
                                  {cardErrors.cvv && <div className="dp-field-error">{cardErrors.cvv}</div>}
                                </Form.Group>
                              </Col>
                            </Row>
                          </>
                        )}

                        <div className="dp-card-security-note">
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                            <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                          </svg>
                          {stripeState.stripe
                            ? 'Your card details are handled securely by Stripe and never touch our servers.'
                            : 'Your card details are encrypted and never stored on our servers.'}
                        </div>
                      </div>
                    )}
                  </div>

                  {/* Step 5 — Message */}
                  <div className="dp-step">
                    <p className="dp-step-num">Final Touch</p>
                    <h3 className="dp-step-title">Add a Message</h3>
                    <Form.Group className="mb-3">
                      <Form.Control
                        as="textarea" rows={3} name="message"
                        placeholder="Share why you're supporting this cause..."
                        value={formData.message}
                        onChange={handleChange} className="dp-input"
                      />
                    </Form.Group>
                    <label className="dp-checkbox dp-checkbox-small">
                      <input type="checkbox" name="isAnonymous" checked={formData.isAnonymous} onChange={handleChange} />
                      <span className="dp-checkbox-mark"></span>
                      <span className="dp-checkbox-label">Display my donation anonymously</span>
                    </label>
                  </div>

                  <button
                    type="submit"
                    className="btn-coral btn-lg dp-submit"
                    disabled={loading || stripeState.paymentProcessing}
                  >
                    {loading || stripeState.paymentProcessing ? (
                      <><span className="spinner-border spinner-border-sm me-2"></span>
                        {stripeState.paymentProcessing ? 'Confirming with Stripe...' : 'Processing...'}
                      </>
                    ) : (
                      <>Complete Donation — {formatCurrency(parseFloat(formData.amount) || 0)}</>
                    )}
                  </button>
                  <p className="dp-secure-note">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                      <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                    </svg>
                    Your payment is secure and encrypted. We never store card details.
                  </p>
                </Form>
              </div>
            </Col>

            {/* Right — Summary */}
            <Col lg={5} className="dp-summary-col">
              <div className="dp-summary-card">
                <div className="dp-summary-header">
                  <p className="eyebrow">Your Donation</p>
                  <div className="dp-summary-amount">
                    {formatCurrency(parseFloat(formData.amount) || 0)}
                  </div>
                </div>
                <div className="dp-summary-detail">
                  <div className="dp-summary-row">
                    <span className="dp-summary-label">Cause</span>
                    <span className="dp-summary-value">
                      {causes.find((c) => c.causeId === parseInt(formData.causeId))?.causeName || 'Not selected'}
                    </span>
                  </div>
                  {formData.campaignId && (
                    <div className="dp-summary-row">
                      <span className="dp-summary-label">Campaign</span>
                      <span className="dp-summary-value">
                        {campaigns.find((c) => c.campaignId === parseInt(formData.campaignId))?.campaignName || '—'}
                      </span>
                    </div>
                  )}
                </div>

                {impactPreview && impactPreview.meals > 0 && (
                  <div className="dp-impact-preview">
                    <p className="dp-impact-title">Your Impact</p>
                    <div className="dp-impact-items">
                      {impactPreview.meals > 0 && (
                        <div className="dp-impact-item">
                          <div className="dp-impact-num">{impactPreview.meals.toLocaleString('vi-VN')}</div>
                          <div className="dp-impact-lbl">Nutritious meals</div>
                        </div>
                      )}
                      {impactPreview.supplies > 0 && (
                        <div className="dp-impact-item">
                          <div className="dp-impact-num">{impactPreview.supplies}</div>
                          <div className="dp-impact-lbl">Supply packs</div>
                        </div>
                      )}
                      {impactPreview.healthcare > 0 && (
                        <div className="dp-impact-item">
                          <div className="dp-impact-num">{impactPreview.healthcare}</div>
                          <div className="dp-impact-lbl">Health check-ups</div>
                        </div>
                      )}
                    </div>
                    <p className="dp-impact-note">
                      Estimated impact based on average programme costs.
                    </p>
                  </div>
                )}

                <div className="dp-trust-mini">
                  <div className="dp-trust-mini-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
                    <span>Secure encrypted payment</span>
                  </div>
                  <div className="dp-trust-mini-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                    <span>Tax deductible receipt</span>
                  </div>
                  <div className="dp-trust-mini-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
                    <span>Transparent reporting</span>
                  </div>
                </div>
              </div>
            </Col>

          </Row>
        </Container>
      </section>

    </div>
  );
};

export default DonatePage;
