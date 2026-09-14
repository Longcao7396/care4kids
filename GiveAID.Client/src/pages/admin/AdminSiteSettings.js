import React, { useState, useEffect, useCallback } from 'react';
import { Row, Col, Card, Alert, Button, Form, Spinner } from 'react-bootstrap';
import api from '../../services/api';

/* ── Site Settings: Manage Privacy / Terms / etc. ──── */
export default function AdminSiteSettings() {
  const [pages, setPages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [editing, setEditing] = useState(null);
  const [pageTitle, setPageTitle] = useState('');
  const [content, setContent] = useState('');
  const [metaDescription, setMetaDescription] = useState('');
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      // Privacy / Terms / etc.
      const response = await api.get('/cms/pages', {
        params: { keys: 'privacy_policy,terms_of_service,help_centre,about_us,contact_info', includeInactive: true },
      });
      if (response.data.success) setPages(response.data.data || []);
    } catch (err) {
      setError('Failed to load site pages.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const openEdit = (page) => {
    setEditing(page);
    setPageTitle(page.pageTitle || '');
    setContent(page.content || '');
    setMetaDescription(page.metaDescription || '');
  };

  const save = async () => {
    if (!editing) return;
    setSaving(true);
    try {
      const response = await api.put(`/cms/pages/${editing.pageId}`, {
        pageTitle, content, metaDescription,
      });
      if (response.data.success) {
        setSuccess('Page saved successfully.');
        setEditing(null);
        load();
      }
    } catch (err) {
      setError('Save failed.');
    } finally {
      setSaving(false);
    }
  };

  const getIconForKey = (key) => {
    const icons = {
      privacy_policy: 'bi-shield-lock-fill',
      terms_of_service: 'bi-file-earmark-text-fill',
      help_centre: 'bi-question-circle-fill',
      about_us: 'bi-info-circle-fill',
      contact_info: 'bi-envelope-fill',
    };
    return icons[key] || 'bi-file-text-fill';
  };

  const getDisplayName = (key) => {
    const names = {
      privacy_policy: 'Privacy Policy',
      terms_of_service: 'Terms of Service',
      help_centre: 'Help Centre Page',
      about_us: 'About Us Page',
      contact_info: 'Contact Information Page',
    };
    return names[key] || key;
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </div>
    );
  }

  return (
    <div>
      {error && <Alert variant="danger" dismissible onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert variant="success" dismissible onClose={() => setSuccess(null)}>{success}</Alert>}

      <h4 className="text-light mb-3">Site-Wide Pages</h4>
      <p className="text-muted mb-3">
        Edit privacy policy, terms of service, and other globally-used CMS pages.
      </p>

      <Row className="g-3">
        {pages.map((page) => (
          <Col key={page.pageId} md={6}>
            <Card style={{ background: 'var(--bg-card)', border: '1px solid var(--border-slate)' }}>
              <Card.Body>
                <div className="d-flex align-items-start gap-3">
                  <div
                    style={{
                      width: 48, height: 48, borderRadius: 10,
                      background: 'rgba(56,189,248,0.12)',
                      color: 'var(--accent-sky)',
                      display: 'flex', alignItems: 'center', justifyContent: 'center',
                      fontSize: '1.4rem', flexShrink: 0,
                    }}
                  >
                    <i className={`bi ${getIconForKey(page.pageKey)}`}></i>
                  </div>
                  <div className="flex-grow-1">
                    <h6 className="text-light mb-1">{getDisplayName(page.pageKey)}</h6>
                    <p className="text-muted small mb-2">
                      {page.content
                        ? `${page.content.replace(/<[^>]+>/g, '').slice(0, 100)}…`
                        : 'No content yet'}
                    </p>
                    <Button
                      variant="outline-primary"
                      size="sm"
                      onClick={() => openEdit(page)}
                    >
                      <i className="bi bi-pencil-square me-1"></i>Edit
                    </Button>
                  </div>
                </div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>

      {/* Edit Modal */}
      {editing && (
        <div className="modal show d-block" style={{ background: 'rgba(0,0,0,0.6)' }} tabIndex="-1">
          <div className="modal-dialog modal-lg modal-dialog-centered">
            <div className="modal-content" style={{ background: 'var(--bg-card)', border: '1px solid var(--border-slate)' }}>
              <div className="modal-header" style={{ borderColor: 'var(--border-slate)' }}>
                <h5 className="modal-title">
                  <i className={`bi ${getIconForKey(editing.pageKey)} me-2 text-accent`}></i>
                  Edit {getDisplayName(editing.pageKey)}
                </h5>
                <button className="btn-close btn-close-white" onClick={() => setEditing(null)}></button>
              </div>
              <div className="modal-body">
                <Form.Group className="mb-3">
                  <Form.Label>Page Title</Form.Label>
                  <Form.Control
                    value={pageTitle}
                    onChange={(e) => setPageTitle(e.target.value)}
                    maxLength={100}
                  />
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label>Content (HTML allowed)</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={16}
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
                  />
                  <Form.Text className="text-muted">
                    {content.length} characters · HTML allowed
                  </Form.Text>
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label>Meta Description (SEO)</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={2}
                    value={metaDescription}
                    onChange={(e) => setMetaDescription(e.target.value)}
                    maxLength={255}
                  />
                </Form.Group>
              </div>
              <div className="modal-footer" style={{ borderColor: 'var(--border-slate)' }}>
                <Button variant="secondary" onClick={() => setEditing(null)} disabled={saving}>Cancel</Button>
                <Button variant="primary" onClick={save} disabled={saving}>
                  {saving ? (
                    <><Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />Saving…</>
                  ) : (
                    <><i className="bi bi-check-circle me-2"></i>Save Changes</>
                  )}
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
