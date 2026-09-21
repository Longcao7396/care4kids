import React, { useState, useEffect, useCallback } from 'react';
import {
  Container, Row, Col, Form, Alert, Spinner, Collapse, Button
} from 'react-bootstrap';
import { faqService } from '../services';
import { sanitizeHtml } from '../utils/safeHtml';
import '../styles/AboutPages.css';

function FaqItem({ item, onSelect }) {
  const [open, setOpen] = useState(false);

  return (
    <div className={`faq-item-card mb-3 ${open ? 'faq-item-card--open' : ''}`}>
      <button
        type="button"
        className={`faq-question ${open ? 'open' : ''}`}
        onClick={() => {
          setOpen(!open);
          if (!open) onSelect(item);
        }}
        aria-expanded={open}
      >
        <span className="faq-question-text">{item.question}</span>
        <span className="faq-arrow-wrap">
          <i className={`bi ${open ? 'bi-chevron-up' : 'bi-chevron-down'}`}></i>
        </span>
      </button>
      <Collapse in={open}>
        <div className="faq-answer">
          {/* SECURITY: server already sanitizes, but client-side sanitize
              again in case a stale row predates the backend fix. */}
          <div dangerouslySetInnerHTML={{ __html: sanitizeHtml(item.answer) }} />
        </div>
      </Collapse>
    </div>
  );
}

function HelpCentrePage() {
  const [faqs, setFaqs] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState('');
  const [activeCategory, setActiveCategory] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(search), 350);
    return () => clearTimeout(timer);
  }, [search]);

  const fetchFaqs = useCallback(async () => {
    try {
      setLoading(true);
      const params = { activeOnly: true };
      if (activeCategory) params.category = activeCategory;
      if (debouncedSearch) params.search = debouncedSearch;
      const response = await faqService.getAll(params);
      if (response.success) {
        setFaqs(response.data || []);
      }
    } catch (err) {
      setError('Failed to load FAQs.');
    } finally {
      setLoading(false);
    }
  }, [activeCategory, debouncedSearch]);

  const fetchCategories = useCallback(async () => {
    try {
      const response = await faqService.getCategories();
      if (response.success) {
        setCategories(response.data || []);
      }
    } catch (err) {
      // Non-fatal
    }
  }, []);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  useEffect(() => {
    fetchFaqs();
  }, [fetchFaqs]);

  const featuredFaqs = faqs.filter((f) => f.isFeatured);
  const regularFaqs = faqs.filter((f) => !f.isFeatured);

  return (
    <div className="about-subpage">
      <section className="page-header">
        <Container>
          <h1>
            Help <span className="text-accent">Centre</span>
          </h1>
          <p className="lead">
            Find answers to frequently asked questions about donations, programmes,
            volunteering and more. Can't find what you need?{' '}
            <a href="/contact">Contact us</a> directly.
          </p>
        </Container>
      </section>

      <Container className="pb-5">
        {/* Search */}
        <div className="team-filter-bar mb-4">
          <Row className="g-3 align-items-end">
            <Col md={7}>
              <Form.Group>
                <Form.Label className="text-light fw-semibold mb-1">
                  Search frequently asked questions
                </Form.Label>
                <div className="position-relative">
                  <Form.Control
                    type="text"
                    placeholder="Type to search (e.g. donation, register, volunteer)..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                  {search && (
                    <button
                      type="button"
                      className="search-clear"
                      onClick={() => setSearch('')}
                      aria-label="Clear search"
                    >
                      <i className="bi bi-x-circle-fill"></i>
                    </button>
                  )}
                </div>
              </Form.Group>
            </Col>
            <Col md={5}>
              <Form.Group>
                <Form.Label className="text-light fw-semibold mb-1">
                  Category
                </Form.Label>
                <Form.Select
                  value={activeCategory}
                  onChange={(e) => setActiveCategory(e.target.value)}
                  aria-label="Filter by category"
                >
                  <option value="">All Categories</option>
                  {categories.map((cat) => (
                    <option key={cat} value={cat}>{cat}</option>
                  ))}
                </Form.Select>
              </Form.Group>
            </Col>
          </Row>
        </div>

        {error && <Alert variant="danger">{error}</Alert>}

        {loading ? (
          <div className="text-center py-5">
            <Spinner animation="border" role="status" variant="primary">
              <span className="visually-hidden">Loading...</span>
            </Spinner>
          </div>
        ) : faqs.length === 0 ? (
          <div className="team-empty">
            <i className="bi bi-question-circle" style={{ fontSize: '3rem', color: 'var(--text-gray)' }}></i>
            <h5 className="mt-3">
              {debouncedSearch
                ? `No results for "${debouncedSearch}"`
                : 'No FAQs in this category'}
            </h5>
            <p className="text-muted">
              {debouncedSearch ? (
                <>Try a different search term or{' '}
                  <a href="/contact">contact us</a>.</>
              ) : (
                <>Check back soon or{' '}
                  <a href="/contact">contact us</a> for help.</>
              )}
            </p>
            {(debouncedSearch || activeCategory) && (
              <Button
                variant="outline-primary"
                className="mt-2"
                onClick={() => {
                  setSearch('');
                  setActiveCategory('');
                }}
              >
                Clear filters
              </Button>
            )}
          </div>
        ) : (
          <>
            {/* Featured FAQs — Popular Questions */}
            {featuredFaqs.length > 0 && !debouncedSearch && (
              <div className="faq-popular-section mb-5">
                <div className="faq-popular-header">
                  <div className="faq-popular-title-row">
                    <i className="bi bi-star-fill text-warning faq-popular-star"></i>
                    <h4 className="faq-popular-heading">Popular Questions</h4>
                  </div>
                  <p className="faq-popular-subtitle">
                    Quick answers to the questions we hear most often.
                  </p>
                </div>
                <div className="faq-popular-cards">
                  {featuredFaqs.map((item) => (
                    <FaqItem key={item.faqId} item={item} onSelect={() => {}} />
                  ))}
                </div>
              </div>
            )}

            {/* All FAQs */}
            {regularFaqs.length > 0 && (
              <div className="faq-all-section">
                {(featuredFaqs.length > 0 || debouncedSearch) && (
                  <div className="faq-all-header">
                    <div className="faq-all-title-row">
                      <h4 className="faq-all-heading">
                        {debouncedSearch ? 'Search Results' : 'All Questions'}
                      </h4>
                      <span className="faq-count-badge">{regularFaqs.length}</span>
                    </div>
                    <p className="faq-all-subtitle">
                      Frequently asked questions and helpful information.
                    </p>
                  </div>
                )}
                {regularFaqs.map((item) => (
                  <FaqItem key={item.faqId} item={item} onSelect={() => {}} />
                ))}
              </div>
            )}

            {/* Still need help */}
            <div className="mt-5 text-center">
              <div className="about-admin-card" style={{ maxWidth: 600, margin: '0 auto' }}>
                <i className="bi bi-chat-dots" style={{ fontSize: '2rem', color: 'var(--accent-sky)' }}></i>
                <h5 className="mt-2">Still need help?</h5>
                <p className="text-muted mb-3">
                  Can't find the answer you're looking for? Our team is here to help.
                </p>
                <Button as="a" href="/contact" variant="primary">
                  <i className="bi bi-envelope me-2"></i>
                  Contact Us
                </Button>
              </div>
            </div>
          </>
        )}
      </Container>

      <style>{`
        /* === FAQ Section Containers === */
        .faq-popular-section {
          background: rgba(14, 116, 144, 0.05);
          border: 1px solid rgba(14, 116, 144, 0.18);
          border-left: 4px solid #0E7490;
          border-radius: 16px;
          padding: 1.5rem 1.75rem;
        }
        .faq-popular-header {
          margin-bottom: 1.25rem;
        }
        .faq-popular-title-row {
          display: flex;
          align-items: center;
          gap: 0.5rem;
          margin-bottom: 0.3rem;
        }
        .faq-popular-star {
          font-size: 1.1rem;
        }
        .faq-popular-heading {
          font-size: 1.6rem;
          font-weight: 800;
          color: #0F172A;
          margin: 0;
          line-height: 1.3;
        }
        .faq-popular-subtitle {
          font-size: 0.95rem;
          color: #64748B;
          margin: 0;
          padding-left: 0.25rem;
        }
        .faq-popular-cards {
          display: flex;
          flex-direction: column;
          gap: 0;
        }

        .faq-all-section {
          max-width: 1040px;
          margin: 0 auto;
        }
        .faq-all-header {
          margin-bottom: 1.25rem;
        }
        .faq-all-title-row {
          display: flex;
          align-items: center;
          gap: 0.6rem;
          margin-bottom: 0.3rem;
        }
        .faq-all-heading {
          font-size: 1.6rem;
          font-weight: 800;
          color: #0F172A;
          margin: 0;
          line-height: 1.3;
        }
        .faq-count-badge {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          background: #38BDF8;
          color: #fff;
          font-size: 0.8rem;
          font-weight: 700;
          min-width: 1.7rem;
          height: 1.7rem;
          border-radius: 999px;
          padding: 0 0.5rem;
          line-height: 1;
        }
        .faq-all-subtitle {
          font-size: 0.95rem;
          color: #64748B;
          margin: 0;
          padding-left: 0.25rem;
        }

        /* === FAQ Card / Accordion Item === */
        .faq-item-card {
          background: #ffffff;
          border: 1px solid #E2E8F0;
          border-radius: 12px;
          padding: 0;
          margin-bottom: 12px;
          box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
          transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
        }
        .faq-item-card:hover {
          border-color: rgba(56, 189, 248, 0.45);
          box-shadow: 0 2px 6px rgba(56, 189, 248, 0.1);
        }
        .faq-item-card--open {
          border-color: rgba(56, 189, 248, 0.6);
          box-shadow: 0 2px 8px rgba(56, 189, 248, 0.15);
        }

        /* === Question Button === */
        .faq-question {
          width: 100%;
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 1.2rem 1.25rem;
          background: transparent;
          border: none;
          color: #1E293B;
          font-size: 1rem;
          font-weight: 600;
          text-align: left;
          cursor: pointer;
          gap: 1rem;
          transition: background 0.2s ease, color 0.2s ease;
          border-radius: 12px;
        }
        .faq-question:hover {
          background: rgba(56, 189, 248, 0.05);
          color: #0E7490;
        }
        .faq-question.open {
          color: #0E7490;
          background: rgba(56, 189, 248, 0.06);
          border-radius: 12px 12px 0 0;
        }
        .faq-question-text {
          flex: 1;
        }

        /* === Arrow Wrapper === */
        .faq-arrow-wrap {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          width: 28px;
          height: 28px;
          border-radius: 50%;
          background: rgba(56, 189, 248, 0.15);
          flex-shrink: 0;
          transition: transform 0.25s ease, background 0.2s ease;
        }
        .faq-arrow-wrap i {
          font-size: 0.8rem;
          color: #38BDF8;
        }
        .faq-question.open .faq-arrow-wrap {
          transform: rotate(180deg);
          background: rgba(14, 116, 144, 0.18);
        }
        .faq-question.open .faq-arrow-wrap i {
          color: #0E7490;
        }

        /* === Answer === */
        .faq-answer {
          padding: 1.1rem 1.25rem 1.25rem;
          color: #475569;
          font-size: 0.95rem;
          line-height: 1.7;
          border-top: 1px solid rgba(56, 189, 248, 0.12);
        }
        .faq-answer p { margin-bottom: 0.75rem; }
        .faq-answer ul, .faq-answer ol {
          padding-left: 1.5rem;
          margin-bottom: 0.75rem;
        }
        .faq-answer li { margin-bottom: 0.35rem; }
        .faq-answer a { color: #38BDF8; }

        /* === Search clear button === */
        .search-clear {
          position: absolute;
          right: 12px;
          top: 50%;
          transform: translateY(-50%);
          background: none;
          border: none;
          color: var(--text-gray);
          font-size: 1.1rem;
          cursor: pointer;
          padding: 0;
          line-height: 1;
          transition: color var(--transition-fast);
        }
        .search-clear:hover { color: var(--accent-sky); }
        .position-relative { position: relative; }
        .position-relative .form-control { padding-right: 2.5rem; }

        /* === Dark Mode === */
        @media (prefers-color-scheme: dark) {
          .faq-item-card {
            background: var(--bg-card);
            border-color: var(--border-slate);
            box-shadow: 0 1px 2px rgba(0, 0, 0, 0.18);
          }
          .faq-item-card:hover {
            border-color: rgba(56, 189, 248, 0.45);
            box-shadow: 0 2px 6px rgba(56, 189, 248, 0.12);
          }
          .faq-item-card--open {
            background: rgba(56, 189, 248, 0.06);
            border-color: rgba(56, 189, 248, 0.5);
          }
          .faq-question {
            color: var(--text-light);
          }
          .faq-question:hover {
            background: rgba(56, 189, 248, 0.06);
            color: #38BDF8;
          }
          .faq-question.open {
            color: #38BDF8;
            background: rgba(56, 189, 248, 0.08);
          }
          .faq-answer {
            color: var(--text-gray);
            border-top-color: rgba(56, 189, 248, 0.1);
          }
          .faq-answer a { color: var(--accent-sky); }
          .faq-popular-section {
            background: rgba(14, 116, 144, 0.08);
            border-color: rgba(14, 116, 144, 0.25);
          }
          .faq-popular-heading,
          .faq-all-heading {
            color: var(--text-light);
          }
          .faq-popular-subtitle,
          .faq-all-subtitle {
            color: var(--text-gray);
          }
        }

        /* === Responsive === */
        @media (max-width: 768px) {
          .faq-question {
            padding: 1rem 1rem;
            font-size: 0.975rem;
          }
          .faq-answer {
            padding: 1rem 1rem 1.1rem;
            font-size: 0.925rem;
          }
          .faq-arrow-wrap {
            width: 26px;
            height: 26px;
          }
          .faq-popular-section {
            padding: 1.25rem 1.25rem;
          }
          .faq-popular-heading,
          .faq-all-heading {
            font-size: 1.35rem;
          }
          .faq-count-badge {
            font-size: 0.75rem;
            min-width: 1.5rem;
            height: 1.5rem;
          }
        }
        @media (max-width: 576px) {
          .faq-question {
            padding: 0.875rem 0.9rem;
            font-size: 0.95rem;
            gap: 0.75rem;
          }
          .faq-answer {
            padding: 0.9rem 0.9rem 1rem;
            font-size: 0.9rem;
            line-height: 1.6;
          }
          .faq-arrow-wrap {
            width: 24px;
            height: 24px;
          }
          .faq-arrow-wrap i {
            font-size: 0.75rem;
          }
          .faq-popular-section {
            padding: 1rem 1rem;
            border-left-width: 3px;
          }
          .faq-popular-heading,
          .faq-all-heading {
            font-size: 1.2rem;
          }
          .faq-popular-subtitle,
          .faq-all-subtitle {
            font-size: 0.875rem;
          }
        }
      `}</style>
    </div>
  );
}

export default HelpCentrePage;
