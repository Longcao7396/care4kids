import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Alert, Spinner } from 'react-bootstrap';
import { contactService } from '../services';
import api from '../services/api';
import { sanitizeHtml } from '../utils/safeHtml';
import './ContactPage.css';

const ContactPage = () => {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    subject: '',
    message: ''
  });
  const [cmsPage, setCmsPage] = useState(null);
  const [success, setSuccess] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const response = await api.get('/cms/pages/contact_info');
        if (!cancelled && response.data.success) {
          setCmsPage(response.data.data);
        }
      } catch {
        // Non-fatal
      } finally {
        if (!cancelled) {
          // intentionally no UI feedback for CMS load; page falls back to defaults
        }
      }
    })();
    return () => { cancelled = true; };
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    setLoading(true);

    try {
      const response = await contactService.submit(formData);
      if (response.success) {
        setSuccess(response.message || 'Thank you for contacting us. We will get back to you within 2 business days.');
        setFormData({ name: '', email: '', phone: '', subject: '', message: '' });
      } else {
        setError(response.message || 'Failed to send message. Please try again.');
      }
    } catch (err) {
      const msg = err.response?.data?.Message || err.response?.data?.message || 'Failed to send message. Please try again later.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  // Default contact info
  const defaultContact = {
    address: '123 Charity Street\nDistrict 1, Ho Chi Minh City\nVietnam',
    phone: '1800-123-456',
    email: 'info@care4kids.org',
    hours: 'Mon–Fri: 9am–6pm (GMT+7)'
  };

  // Contact info cards
  const contactChannels = [
    {
      icon: (
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
      ),
      title: 'Visit Us',
      lines: ['123 Charity Street', 'District 1, Ho Chi Minh City', 'Vietnam']
    },
    {
      icon: (
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
      ),
      title: 'Call Us',
      lines: [defaultContact.phone, defaultContact.hours]
    },
    {
      icon: (
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
      ),
      title: 'Email Us',
      lines: [defaultContact.email, 'We reply within 2 business days']
    }
  ];

  return (
    <div className="cp-page">

      {/* ─── HERO ─── */}
      <section className="cp-hero">
        <Container className="cp-hero-content">
          <p className="eyebrow">Get in Touch</p>
          <h1 className="cp-hero-title">Contact Care4Kids</h1>
          <p className="cp-hero-sub">
            Have a question about our work, want to partner with us, or interested in volunteering?
            We'd love to hear from you. Choose the channel that works best for you.
          </p>
        </Container>
      </section>

      {/* ─── CONTACT CHANNELS ─── */}
      <section className="cp-channels">
        <Container>
          <div className="cp-channels-grid">
            {contactChannels.map((channel, i) => (
              <div key={i} className="cp-channel-card">
                <div className="cp-channel-icon">{channel.icon}</div>
                <h3 className="cp-channel-title">{channel.title}</h3>
                {channel.lines.map((line, j) => (
                  <p key={j} className="cp-channel-line">{line}</p>
                ))}
              </div>
            ))}
          </div>
        </Container>
      </section>

      {/* ─── FORM + MAP ─── */}
      <section className="cp-form-section">
        <Container>
          <Row className="cp-form-row">

            {/* Left — Form */}
            <Col lg={7}>
              <div className="cp-form-card">
                <p className="eyebrow">Send a Message</p>
                <h2 className="cp-form-title">How Can We Help?</h2>
                <p className="cp-form-desc">
                  Fill out the form below and we'll get back to you within 2 business days.
                </p>

                {error && (
                  <Alert variant="danger" dismissible onClose={() => setError(null)}>
                    {error}
                  </Alert>
                )}
                {success && (
                  <Alert variant="success" dismissible onClose={() => setSuccess(null)}>
                    {success}
                  </Alert>
                )}

                <Form onSubmit={handleSubmit} className="cp-form">
                  <Row>
                    <Col md={6}>
                      <Form.Group className="mb-4">
                        <Form.Label className="cp-label">Your Name *</Form.Label>
                        <Form.Control
                          type="text"
                          name="name"
                          placeholder="Full name"
                          value={formData.name}
                          onChange={handleChange}
                          required
                          maxLength={100}
                          className="cp-input"
                        />
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group className="mb-4">
                        <Form.Label className="cp-label">Email *</Form.Label>
                        <Form.Control
                          type="email"
                          name="email"
                          placeholder="your@email.com"
                          value={formData.email}
                          onChange={handleChange}
                          required
                          maxLength={100}
                          className="cp-input"
                        />
                      </Form.Group>
                    </Col>
                  </Row>

                  <Row>
                    <Col md={6}>
                      <Form.Group className="mb-4">
                        <Form.Label className="cp-label">Phone (Optional)</Form.Label>
                        <Form.Control
                          type="tel"
                          name="phone"
                          placeholder="+84 ..."
                          value={formData.phone}
                          onChange={handleChange}
                          maxLength={20}
                          className="cp-input"
                        />
                      </Form.Group>
                    </Col>
                    <Col md={6}>
                      <Form.Group className="mb-4">
                        <Form.Label className="cp-label">Subject *</Form.Label>
                        <Form.Control
                          type="text"
                          name="subject"
                          placeholder="How can we help?"
                          value={formData.subject}
                          onChange={handleChange}
                          required
                          maxLength={200}
                          className="cp-input"
                        />
                      </Form.Group>
                    </Col>
                  </Row>

                  <Form.Group className="mb-4">
                    <Form.Label className="cp-label">Message *</Form.Label>
                    <Form.Control
                      as="textarea"
                      rows={6}
                      name="message"
                      placeholder="Please describe your inquiry in detail..."
                      value={formData.message}
                      onChange={handleChange}
                      required
                      className="cp-input cp-textarea"
                    />
                    <Form.Text className="cp-help">{formData.message.length} characters</Form.Text>
                  </Form.Group>

                  <button type="submit" className="btn-coral btn-lg cp-submit" disabled={loading}>
                    {loading ? (
                      <>
                        <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />
                        Sending…
                      </>
                    ) : (
                      <>
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><line x1="22" y1="2" x2="11" y2="13"/><polygon points="22 2 15 22 11 13 2 9 22 2"/></svg>
                        Send Message
                      </>
                    )}
                  </button>
                </Form>
              </div>
            </Col>

            {/* Right — CMS contact info + Map */}
            <Col lg={5}>
              <div className="cp-sidebar">
                {cmsPage?.content && (
                  <div className="cp-info-card">
                    <p className="eyebrow">{cmsPage.pageTitle || 'Additional Information'}</p>
                    <div
                      className="cp-cms-content"
                      dangerouslySetInnerHTML={{ __html: sanitizeHtml(cmsPage.content) }}
                    />
                  </div>
                )}

                <div className="cp-map-card">
                  <p className="eyebrow">Find Us</p>
                  <h3 className="cp-map-title">Ho Chi Minh City Office</h3>
                  <div className="cp-map">
                    <iframe
                      src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.4868245765676!2d106.70042347485675!3d10.773476259241107!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f47866c1b87%3A0x6e98a4e2c5a5e6db!2sHo%20Chi%20Minh%20City%2C%20Vietnam!5e0!3m2!1sen!2s!4v1700000000000"
                      width="100%"
                      height="280"
                      style={{ border: 0, borderRadius: 'var(--radius-md)' }}
                      allowFullScreen=""
                      loading="lazy"
                      referrerPolicy="no-referrer-when-downgrade"
                      title="Care4Kids HCMC Office Location"
                    />
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

export default ContactPage;
