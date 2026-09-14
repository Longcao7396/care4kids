import React, { useState, useEffect, useMemo } from 'react';
import { Container, Row, Col, Form, Button, Alert } from 'react-bootstrap';
import { useNavigate, useLocation } from 'react-router-dom';
import { causesService, donationsService } from '../services';
import api from '../services/api';
import './DonatePage.css';

const DonatePage = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const [causes, setCauses] = useState([]);
  const [campaigns, setCampaigns] = useState([]);
  const [formData, setFormData] = useState({
    causeId: location.state?.causeId || '',
    campaignId: location.state?.campaignId || '',
    amount: '500000',
    isRecurring: false,
    paymentMethod: 'CreditCard',
    cardHolderName: '',
    cardNumber: '',
    expiryDate: '',
    cvv: '',
    message: '',
    isAnonymous: false
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    loadCauses();
    loadCampaigns();
  }, []);

  useEffect(() => {
    if (formData.causeId) {
      loadCampaignsByCause(formData.causeId);
    }
  }, [formData.causeId]);

  const loadCauses = async () => {
    try {
      const response = await causesService.getAll(true);
      if (response.success) setCauses(response.data);
    } catch (error) {
      console.error('Error loading causes:', error);
    }
  };

  const loadCampaigns = async () => {
    try {
      const response = await api.get('/campaigns', { params: { status: 'Active' } });
      if (response.data.success) setCampaigns(response.data.data);
    } catch (error) {
      console.error('Error loading campaigns:', error);
    }
  };

  const loadCampaignsByCause = async (causeId) => {
    try {
      const response = await api.get('/campaigns', {
        params: { status: 'Active', causeId: causeId }
      });
      if (response.data.success) setCampaigns(response.data.data);
    } catch (error) {
      console.error('Error loading campaigns:', error);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleQuickAmount = (amount) => {
    setFormData(prev => ({ ...prev, amount: amount.toString() }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!formData.causeId) {
      setError('Please select a cause to support');
      return;
    }
    if (!formData.amount || parseFloat(formData.amount) <= 0) {
      setError('Please enter a valid donation amount');
      return;
    }

    setLoading(true);
    try {
      const donationData = {
        causeId: parseInt(formData.causeId),
        campaignId: formData.campaignId ? parseInt(formData.campaignId) : null,
        amount: parseFloat(formData.amount),
        isRecurring: formData.isRecurring,
        paymentMethod: formData.paymentMethod,
        cardHolderName: formData.cardHolderName,
        cardNumber: formData.cardNumber,
        expiryDate: formData.expiryDate,
        cvv: formData.cvv,
        message: formData.message || null,
        isAnonymous: formData.isAnonymous
      };

      const response = await donationsService.create(donationData);

      if (response.success) {
        setSuccess('Thank you for your generous donation. You will receive a receipt via email.');
        setTimeout(() => navigate('/my-donations'), 2000);
      }
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Donation failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount || 0);

  const quickAmounts = [100000, 250000, 500000, 1000000, 2500000, 5000000];

  // Impact preview — calculated dynamically based on amount
  const impactPreview = useMemo(() => {
    const amount = parseFloat(formData.amount) || 0;
    if (amount <= 0) return null;
    const meals = Math.floor(amount / 25000);
    const supplies = Math.floor(amount / 150000);
    const healthcare = Math.floor(amount / 500000);
    return { meals, supplies, healthcare };
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

      {/* ─── DONATION FORM ─── */}
      <section className="dp-form-section">
        <Container>
          <Row className="dp-form-row">

            {/* Left column — Form */}
            <Col lg={7} className="dp-form-col">
              <div className="dp-form-card">
                {error && (
                  <Alert variant="danger" className="dp-alert">{error}</Alert>
                )}
                {success && (
                  <Alert variant="success" className="dp-alert">{success}</Alert>
                )}

                <Form onSubmit={handleSubmit}>

                  {/* Step 1 — Choose cause */}
                  <div className="dp-step">
                    <p className="dp-step-num">Step 1</p>
                    <h3 className="dp-step-title">Choose a Cause</h3>
                    <p className="dp-step-desc">Select the area you'd like your donation to support.</p>

                    <Form.Select
                      name="causeId"
                      value={formData.causeId}
                      onChange={handleChange}
                      required
                      className="dp-select"
                    >
                      <option value="">Choose a cause...</option>
                      {causes.map((cause) => (
                        <option key={cause.causeId} value={cause.causeId}>
                          {cause.causeName}
                        </option>
                      ))}
                    </Form.Select>
                  </div>

                  {/* Step 2 — Optional Campaign */}
                  {campaigns.length > 0 && formData.causeId && (
                    <div className="dp-step">
                      <p className="dp-step-num">Step 2 · Optional</p>
                      <h3 className="dp-step-title">Select a Campaign</h3>
                      <p className="dp-step-desc">Choose a specific campaign, or leave blank for a general donation to this cause.</p>

                      <Form.Select
                        name="campaignId"
                        value={formData.campaignId}
                        onChange={handleChange}
                        className="dp-select"
                      >
                        <option value="">General donation to this cause</option>
                        {campaigns.map((campaign) => (
                          <option key={campaign.campaignId} value={campaign.campaignId}>
                            {campaign.campaignName} — {campaign.percentageReached?.toFixed(0) || 0}% funded
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
                      {quickAmounts.map((amount) => (
                        <button
                          type="button"
                          key={amount}
                          className={`dp-quick-btn ${formData.amount === amount.toString() ? 'is-active' : ''}`}
                          onClick={() => handleQuickAmount(amount)}
                        >
                          {formatCurrency(amount)}
                        </button>
                      ))}
                    </div>

                    <Form.Group className="mt-3">
                      <Form.Label className="dp-label">Custom amount (VND)</Form.Label>
                      <Form.Control
                        type="number"
                        name="amount"
                        placeholder="Enter any amount"
                        value={formData.amount}
                        onChange={handleChange}
                        min="1000"
                        step="1000"
                        required
                        className="dp-input dp-input-large"
                      />
                    </Form.Group>

                    <div className="dp-recurring">
                      <label className="dp-checkbox">
                        <input
                          type="checkbox"
                          name="isRecurring"
                          checked={formData.isRecurring}
                          onChange={handleChange}
                        />
                        <span className="dp-checkbox-mark"></span>
                        <span className="dp-checkbox-label">
                          Make this a <strong>monthly donation</strong>
                        </span>
                      </label>
                      <p className="dp-checkbox-help">
                        Monthly donations provide steady support that helps us plan long-term programmes.
                      </p>
                    </div>
                  </div>

                  {/* Step 4 — Donor Info */}
                  <div className="dp-step">
                    <p className="dp-step-num">Donor Information</p>
                    <h3 className="dp-step-title">Your Details</h3>

                    <Form.Group className="mb-3">
                      <Form.Label className="dp-label">Payment Method</Form.Label>
                      <Form.Select
                        name="paymentMethod"
                        value={formData.paymentMethod}
                        onChange={handleChange}
                        className="dp-select"
                      >
                        <option value="CreditCard">Credit Card</option>
                        <option value="DebitCard">Debit Card</option>
                        <option value="NetBanking">Net Banking</option>
                      </Form.Select>
                    </Form.Group>

                    <Form.Group className="mb-3">
                      <Form.Label className="dp-label">Cardholder Name</Form.Label>
                      <Form.Control
                        type="text"
                        name="cardHolderName"
                        placeholder="Name as it appears on card"
                        value={formData.cardHolderName}
                        onChange={handleChange}
                        required
                        className="dp-input"
                      />
                    </Form.Group>

                    <Form.Group className="mb-3">
                      <Form.Label className="dp-label">Card Number</Form.Label>
                      <Form.Control
                        type="text"
                        name="cardNumber"
                        placeholder="1234 5678 9012 3456"
                        value={formData.cardNumber}
                        onChange={handleChange}
                        maxLength="19"
                        required
                        className="dp-input"
                      />
                    </Form.Group>

                    <Row>
                      <Col md={6}>
                        <Form.Group className="mb-3">
                          <Form.Label className="dp-label">Expiry Date</Form.Label>
                          <Form.Control
                            type="text"
                            name="expiryDate"
                            placeholder="MM/YY"
                            value={formData.expiryDate}
                            onChange={handleChange}
                            maxLength="5"
                            required
                            className="dp-input"
                          />
                        </Form.Group>
                      </Col>
                      <Col md={6}>
                        <Form.Group className="mb-3">
                          <Form.Label className="dp-label">CVV</Form.Label>
                          <Form.Control
                            type="text"
                            name="cvv"
                            placeholder="123"
                            value={formData.cvv}
                            onChange={handleChange}
                            maxLength="4"
                            required
                            className="dp-input"
                          />
                        </Form.Group>
                      </Col>
                    </Row>

                    <Form.Group className="mb-3">
                      <Form.Label className="dp-label">Add a Message (Optional)</Form.Label>
                      <Form.Control
                        as="textarea"
                        rows={3}
                        name="message"
                        placeholder="Share why you're supporting this cause..."
                        value={formData.message}
                        onChange={handleChange}
                        className="dp-input"
                      />
                    </Form.Group>

                    <label className="dp-checkbox dp-checkbox-small">
                      <input
                        type="checkbox"
                        name="isAnonymous"
                        checked={formData.isAnonymous}
                        onChange={handleChange}
                      />
                      <span className="dp-checkbox-mark"></span>
                      <span className="dp-checkbox-label">Display my donation anonymously</span>
                    </label>
                  </div>

                  <button type="submit" className="btn-coral btn-lg dp-submit" disabled={loading}>
                    {loading ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2"></span>
                        Processing your donation...
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

            {/* Right column — Summary + Impact */}
            <Col lg={5} className="dp-summary-col">
              <div className="dp-summary-card">
                <div className="dp-summary-header">
                  <p className="eyebrow">Your Donation</p>
                  <div className="dp-summary-amount">
                    {formatCurrency(parseFloat(formData.amount) || 0)}
                  </div>
                  {formData.isRecurring && (
                    <span className="badge badge-coral">Monthly</span>
                  )}
                </div>

                <div className="dp-summary-detail">
                  <div className="dp-summary-row">
                    <span className="dp-summary-label">Cause</span>
                    <span className="dp-summary-value">
                      {causes.find(c => c.causeId === parseInt(formData.causeId))?.causeName || 'Not selected'}
                    </span>
                  </div>
                  {formData.campaignId && (
                    <div className="dp-summary-row">
                      <span className="dp-summary-label">Campaign</span>
                      <span className="dp-summary-value">
                        {campaigns.find(c => c.campaignId === parseInt(formData.campaignId))?.campaignName || '—'}
                      </span>
                    </div>
                  )}
                  <div className="dp-summary-row">
                    <span className="dp-summary-label">Frequency</span>
                    <span className="dp-summary-value">{formData.isRecurring ? 'Monthly' : 'One-time'}</span>
                  </div>
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
                      Estimated impact based on average programme costs. Final allocation is decided by programme managers.
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
