import React, { useState, useEffect, useCallback } from 'react';
import {
  Container, Row, Col, Form, Alert, Spinner, Collapse, Button
} from 'react-bootstrap';
import api from '../services/api';
import '../styles/AboutPages.css';

function FaqItem({ item, onSelect }) {
  const [open, setOpen] = useState(false);

  return (
    <div className="faq-item mb-3">
      <button
        type="button"
        className={`faq-question ${open ? 'open' : ''}`}
        onClick={() => {
          setOpen(!open);
          if (!open) onSelect(item);
        }}
        aria-expanded={open}
      >
        <span>{item.question}</span>
        <i className={`bi ${open ? 'bi-chevron-up' : 'bi-chevron-down'}`}></i>
      </button>
      <Collapse in={open}>
        <div className="faq-answer">
          <div dangerouslySetInnerHTML={{ __html: item.answer }} />
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
      const response = await api.get('/faqs', { params });
      if (response.data.success) {
        setFaqs(response.data.data || []);
      }
    } catch (err) {
      setError('Failed to load FAQs.');
    } finally {
      setLoading(false);
    }
  }, [activeCategory, debouncedSearch]);

  const fetchCategories = useCallback(async () => {
    try {
      const response = await api.get('/faqs/categories');
      if (response.data.success) {
        setCategories(response.data.data || []);
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
            {/* Featured FAQs */}
            {featuredFaqs.length > 0 && !debouncedSearch && (
              <div className="mb-4">
                <h4 className="text-light mb-3">
                  <i className="bi bi-star-fill text-warning me-2"></i>
                  Popular Questions
                </h4>
                {featuredFaqs.map((item) => (
                  <FaqItem key={item.faqId} item={item} onSelect={() => {}} />
                ))}
              </div>
            )}

            {/* All FAQs */}
            {regularFaqs.length > 0 && (
              <div>
                {(featuredFaqs.length > 0 || debouncedSearch) && (
                  <h4 className="text-light mb-3">
                    {debouncedSearch ? 'Search Results' : 'All Questions'}
                    <span className="badge bg-primary ms-2">{regularFaqs.length}</span>
                  </h4>
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
        .faq-item {
          background: var(--bg-card);
          border: 1px solid var(--border-slate);
          border-radius: var(--radius-lg);
          overflow: hidden;
          transition: border-color var(--transition-base);
        }
        .faq-item:hover {
          border-color: rgba(56, 189, 248, 0.4);
        }
        .faq-question {
          width: 100%;
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 1.1rem 1.25rem;
          background: transparent;
          border: none;
          color: var(--text-light);
          font-size: 1rem;
          font-weight: 600;
          text-align: left;
          cursor: pointer;
          gap: 1rem;
          transition: all var(--transition-base);
        }
        .faq-question:hover {
          background: rgba(56, 189, 248, 0.06);
        }
        .faq-question.open {
          color: var(--accent-sky);
          background: rgba(56, 189, 248, 0.08);
          border-bottom: 1px solid var(--border-slate);
        }
        .faq-question i {
          font-size: 1rem;
          flex-shrink: 0;
          color: var(--accent-sky);
        }
        .faq-answer {
          padding: 1.1rem 1.25rem;
          color: var(--text-gray);
          font-size: 0.95rem;
          line-height: 1.65;
        }
        .faq-answer p { margin-bottom: 0.75rem; }
        .faq-answer ul, .faq-answer ol {
          padding-left: 1.5rem;
          margin-bottom: 0.75rem;
        }
        .faq-answer li { margin-bottom: 0.35rem; }
        .faq-answer a { color: var(--accent-sky); }
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
      `}</style>
    </div>
  );
}

export default HelpCentrePage;
