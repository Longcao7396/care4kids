import React, { useState, useEffect, useCallback } from 'react';
import {
  Alert, Button, Modal, Form, Spinner, Badge, InputGroup, Row, Col
} from 'react-bootstrap';
import { campaignReportsService, campaignsService } from '../../services';

const fmtVnd = (n) => n != null
  ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(n)
  : '—';

const fmtDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '—';

/* ── Validation ────────────────────────────── */
function validate(data) {
  const errs = {};
  if (!data.campaignId) errs.campaignId = 'Campaign is required.';
  if (!data.reportTitle?.trim()) errs.reportTitle = 'Report title is required.';
  if (data.totalReceived === '' || data.totalReceived == null)
    errs.totalReceived = 'Total received is required.';
  if (data.totalReceived < 0) errs.totalReceived = 'Must be >= 0.';
  if (data.totalSpent === '' || data.totalSpent == null)
    errs.totalSpent = 'Total spent is required.';
  if (data.totalSpent < 0) errs.totalSpent = 'Must be >= 0.';
  if (data.beneficiariesReached != null && data.beneficiariesReached < 0)
    errs.beneficiariesReached = 'Must be >= 0.';
  return errs;
}

const EMPTY = {
  campaignId: '',
  reportTitle: '',
  reportContent: '',
  totalReceived: 0,
  totalSpent: 0,
  beneficiariesReached: 0,
  expenseBreakdown: '',
  photos: '',
  documents: '',
  isPublished: false,
};

/* ── Admin Campaign Reports Manager ─────────── */
export default function AdminCampaignReportsManager() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(EMPTY);
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [campaigns, setCampaigns] = useState([]);
  const [campaignFilter, setCampaignFilter] = useState('');
  const [publishedFilter, setPublishedFilter] = useState('');
  const [search, setSearch] = useState('');
  const [deleteConfirm, setDeleteConfirm] = useState(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      // Admin view — include drafts (publishedOnly=false)
      const params = { publishedOnly: false, pageSize: 100 };
      const response = await campaignReportsService.getAll(params);
      if (response.success) {
        const data = response.data;
        setItems(data?.items || data || []);
      }
    } catch (err) {
      setError('Failed to load campaign reports.');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadCampaigns = useCallback(async () => {
    try {
      const response = await campaignsService.getAll({ pageSize: 200, page: 1 });
      if (response.success) setCampaigns(response.data?.items || response.data || []);
    } catch (err) {
      // Silent fail — campaign select will just be empty.
    }
  }, []);

  useEffect(() => { load(); loadCampaigns(); }, [load, loadCampaigns]);

  const openAdd = () => {
    setEditing(null);
    setForm(EMPTY);
    setErrors({});
    setShowForm(true);
  };

  const openEdit = (item) => {
    setEditing(item);
    setForm({
      campaignId: item.campaignId || '',
      reportTitle: item.reportTitle || '',
      reportContent: item.reportContent || '',
      totalReceived: item.totalReceived ?? 0,
      totalSpent: item.totalSpent ?? 0,
      beneficiariesReached: item.beneficiariesReached ?? 0,
      expenseBreakdown: item.expenseBreakdown || '',
      photos: item.photos || '',
      documents: item.documents || '',
      isPublished: !!item.isPublished,
    });
    setErrors({});
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errs = validate(form);
    if (Object.keys(errs).length) { setErrors(errs); return; }
    setSaving(true);
    try {
      const payload = {
        ...form,
        campaignId: parseInt(form.campaignId) || 0,
        totalReceived: parseFloat(form.totalReceived) || 0,
        totalSpent: parseFloat(form.totalSpent) || 0,
        beneficiariesReached: parseInt(form.beneficiariesReached) || 0,
      };
      const response = editing
        ? await campaignReportsService.update(editing.reportId, payload)
        : await campaignReportsService.create(payload);
      if (response.success) {
        setSuccess(editing ? 'Report updated.' : 'Report created.');
        setShowForm(false);
        load();
      }
    } catch (err) {
      setError('Save failed. Check the form values.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirm) return;
    try {
      await campaignReportsService.remove(deleteConfirm);
      setSuccess('Report deleted.');
    } catch (err) {
      setError('Delete failed.');
    } finally {
      setDeleteConfirm(null);
      load();
    }
  };

  const isSuperAdmin = JSON.parse(localStorage.getItem('giveaid_user') || '{}')?.role === 'SuperAdmin';

  const filtered = items.filter((item) => {
    const s = search.toLowerCase();
    const matchSearch = !s ||
      item.reportTitle?.toLowerCase().includes(s) ||
      item.reportContent?.toLowerCase().includes(s);
    const matchCampaign = !campaignFilter || String(item.campaignId) === String(campaignFilter);
    const matchPublished = !publishedFilter ||
      (publishedFilter === 'published' && item.isPublished) ||
      (publishedFilter === 'draft' && !item.isPublished);
    return matchSearch && matchCampaign && matchPublished;
  });

  return (
    <div>
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

      <div className="d-flex justify-content-between align-items-center mb-3">
        <div>
          <h4 className="text-light mb-0">Campaign Reports</h4>
          <small className="text-muted">Impact & transparency reports attached to a campaign</small>
        </div>
        <Button variant="primary" onClick={openAdd}>
          <i className="bi bi-plus-circle me-2"></i>New Report
        </Button>
      </div>

      {/* Filters */}
      <div className="team-filter-bar mb-3">
        <Row className="g-3 align-items-end">
          <Col md={4}>
            <Form.Group>
              <Form.Label className="text-light fw-semibold mb-1">Search</Form.Label>
              <InputGroup>
                <Form.Control
                  type="text"
                  placeholder="Search report title or content..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
                {search && (
                  <InputGroup.Text style={{ cursor: 'pointer' }} onClick={() => setSearch('')}>
                    <i className="bi bi-x-circle"></i>
                  </InputGroup.Text>
                )}
              </InputGroup>
            </Form.Group>
          </Col>
          <Col md={4}>
            <Form.Group>
              <Form.Label className="text-light fw-semibold mb-1">Campaign</Form.Label>
              <Form.Select value={campaignFilter} onChange={(e) => setCampaignFilter(e.target.value)}>
                <option value="">All Campaigns</option>
                {campaigns.map((c) => (
                  <option key={c.campaignId} value={String(c.campaignId)}>
                    {c.campaignName || c.name}
                  </option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={3}>
            <Form.Group>
              <Form.Label className="text-light fw-semibold mb-1">Status</Form.Label>
              <Form.Select value={publishedFilter} onChange={(e) => setPublishedFilter(e.target.value)}>
                <option value="">All</option>
                <option value="published">Published</option>
                <option value="draft">Draft</option>
              </Form.Select>
            </Form.Group>
          </Col>
        </Row>
      </div>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>
      ) : filtered.length === 0 ? (
        <Alert variant="info">
          {items.length === 0
            ? 'No campaign reports yet. Click "New Report" to create your first one.'
            : 'No reports match your filters.'}
        </Alert>
      ) : (
        <div className="report-admin-list">
          {filtered.map((item) => {
            const camp = campaigns.find((c) => c.campaignId === item.campaignId);
            return (
              <div key={item.reportId} className={`report-admin-item ${!item.isPublished ? 'draft' : ''}`}>
                <div className="d-flex justify-content-between align-items-start gap-3 flex-wrap">
                  <div className="flex-grow-1 min-w-0">
                    <div className="d-flex align-items-center gap-2 mb-1 flex-wrap">
                      <h6 className="report-title mb-0">{item.reportTitle || '(untitled)'}</h6>
                      {item.isPublished
                        ? <Badge bg="success" style={{ fontSize: '0.65rem' }}>Published</Badge>
                        : <Badge bg="secondary" style={{ fontSize: '0.65rem' }}>Draft</Badge>}
                    </div>
                    <div className="report-meta mb-2">
                      <span><i className="bi bi-flag me-1"></i>{camp?.campaignName || `Campaign #${item.campaignId}`}</span>
                      {item.publishedDate && (
                        <span className="ms-3"><i className="bi bi-calendar-check me-1"></i>Published {fmtDate(item.publishedDate)}</span>
                      )}
                      <span className="ms-3"><i className="bi bi-clock me-1"></i>Created {fmtDate(item.createdAt)}</span>
                    </div>
                    <div className="report-stats">
                      <span><strong>Received:</strong> {fmtVnd(item.totalReceived)}</span>
                      <span><strong>Spent:</strong> {fmtVnd(item.totalSpent)}</span>
                      <span><strong>Remaining:</strong> <span style={{ color: (item.totalReceived - item.totalSpent) >= 0 ? '#10B981' : '#EF4444' }}>
                        {fmtVnd(item.totalReceived - item.totalSpent)}
                      </span></span>
                      <span><strong>Beneficiaries:</strong> {(item.beneficiariesReached ?? 0).toLocaleString()}</span>
                    </div>
                    {item.reportContent && (
                      <p className="report-snippet mt-2 mb-0">
                        {item.reportContent.length > 200
                          ? item.reportContent.slice(0, 200) + '…'
                          : item.reportContent}
                      </p>
                    )}
                  </div>
                  <div className="d-flex flex-column gap-2" style={{ minWidth: 130 }}>
                    <Button variant="outline-primary" size="sm" onClick={() => openEdit(item)}>
                      <i className="bi bi-pencil me-1"></i>Edit
                    </Button>
                    {isSuperAdmin && (
                      <Button variant="outline-danger" size="sm" onClick={() => setDeleteConfirm(item.reportId)}>
                        <i className="bi bi-trash me-1"></i>Delete
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <p className="text-muted small mt-3">
        Showing {filtered.length} of {items.length} report(s)
      </p>

      {/* Form Modal */}
      <Modal show={showForm} onHide={() => setShowForm(false)} size="lg" centered className="admin-modal-dark">
        <Modal.Header closeButton>
          <Modal.Title>
            <i className={`bi ${editing ? 'bi-pencil-square' : 'bi-plus-circle'} me-2 text-accent`}></i>
            {editing ? 'Edit Report' : 'New Report'}
          </Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            <Row className="g-3">
              <Col md={6}>
                <Form.Group>
                  <Form.Label>Campaign *</Form.Label>
                  <Form.Select
                    value={form.campaignId}
                    onChange={(e) => setForm({ ...form, campaignId: e.target.value })}
                    isInvalid={!!errors.campaignId}
                  >
                    <option value="">— Select a campaign —</option>
                    {campaigns.map((c) => (
                      <option key={c.campaignId} value={String(c.campaignId)}>
                        {c.campaignName || c.name}
                      </option>
                    ))}
                  </Form.Select>
                  {errors.campaignId && (
                    <Form.Control.Feedback type="invalid">{errors.campaignId}</Form.Control.Feedback>
                  )}
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group>
                  <Form.Label>Report Title *</Form.Label>
                  <Form.Control
                    value={form.reportTitle}
                    onChange={(e) => setForm({ ...form, reportTitle: e.target.value })}
                    isInvalid={!!errors.reportTitle}
                    maxLength={200}
                    placeholder="e.g. Q3 2026 Impact Report"
                  />
                  {errors.reportTitle && (
                    <Form.Control.Feedback type="invalid">{errors.reportTitle}</Form.Control.Feedback>
                  )}
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3 mt-3">
              <Form.Label>Report Content</Form.Label>
              <Form.Control
                as="textarea"
                rows={6}
                value={form.reportContent}
                onChange={(e) => setForm({ ...form, reportContent: e.target.value })}
                placeholder="Detailed narrative of what was achieved, challenges faced, next steps..."
              />
            </Form.Group>

            <Row className="g-3">
              <Col md={4}>
                <Form.Group>
                  <Form.Label>Total Received (VND) *</Form.Label>
                  <Form.Control
                    type="number"
                    min="0"
                    step="1000"
                    value={form.totalReceived}
                    onChange={(e) => setForm({ ...form, totalReceived: e.target.value })}
                    isInvalid={!!errors.totalReceived}
                  />
                  {errors.totalReceived && (
                    <Form.Control.Feedback type="invalid">{errors.totalReceived}</Form.Control.Feedback>
                  )}
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group>
                  <Form.Label>Total Spent (VND) *</Form.Label>
                  <Form.Control
                    type="number"
                    min="0"
                    step="1000"
                    value={form.totalSpent}
                    onChange={(e) => setForm({ ...form, totalSpent: e.target.value })}
                    isInvalid={!!errors.totalSpent}
                  />
                  {errors.totalSpent && (
                    <Form.Control.Feedback type="invalid">{errors.totalSpent}</Form.Control.Feedback>
                  )}
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group>
                  <Form.Label>Beneficiaries Reached</Form.Label>
                  <Form.Control
                    type="number"
                    min="0"
                    value={form.beneficiariesReached}
                    onChange={(e) => setForm({ ...form, beneficiariesReached: e.target.value })}
                    isInvalid={!!errors.beneficiariesReached}
                  />
                  {errors.beneficiariesReached && (
                    <Form.Control.Feedback type="invalid">{errors.beneficiariesReached}</Form.Control.Feedback>
                  )}
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3 mt-3">
              <Form.Label>Expense Breakdown (JSON)</Form.Label>
              <Form.Control
                as="textarea"
                rows={3}
                value={form.expenseBreakdown}
                onChange={(e) => setForm({ ...form, expenseBreakdown: e.target.value })}
                placeholder='[{"category":"Meals","amount":15000000},{"category":"Books","amount":5000000}]'
                style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
              />
              <Form.Text className="text-muted">
                Optional. JSON array of expense items.
              </Form.Text>
            </Form.Group>

            <Row className="g-3">
              <Col md={6}>
                <Form.Group>
                  <Form.Label>Photos (JSON)</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={2}
                    value={form.photos}
                    onChange={(e) => setForm({ ...form, photos: e.target.value })}
                    placeholder='["https://.../photo1.jpg","https://.../photo2.jpg"]'
                    style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group>
                  <Form.Label>Documents (JSON)</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={2}
                    value={form.documents}
                    onChange={(e) => setForm({ ...form, documents: e.target.value })}
                    placeholder='[{"name":"Audit.pdf","url":"https://..."}]'
                    style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
                  />
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mt-3">
              <Form.Check
                type="switch"
                id="report-published"
                label={form.isPublished ? 'Published (visible to public)' : 'Draft (admin only)'}
                checked={form.isPublished}
                onChange={(e) => setForm({ ...form, isPublished: e.target.checked })}
              />
              <Form.Text className="text-muted">
                Publishing will automatically stamp today's date as the publish date.
              </Form.Text>
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShowForm(false)} disabled={saving}>
              Cancel
            </Button>
            <Button variant="primary" type="submit" disabled={saving}>
              {saving ? (
                <><Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />Saving...</>
              ) : (
                <><i className={`bi ${editing ? 'bi-check-circle' : 'bi-plus-circle'} me-2`}></i>
                {editing ? 'Update Report' : 'Create Report'}</>
              )}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>

      {/* Delete Confirm */}
      <Modal show={!!deleteConfirm} onHide={() => setDeleteConfirm(null)} centered className="admin-modal-dark">
        <Modal.Header closeButton>
          <Modal.Title>
            <i className="bi bi-exclamation-triangle-fill text-danger me-2"></i>
            Delete Report
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete this campaign report? This action cannot be undone.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setDeleteConfirm(null)}>Cancel</Button>
          <Button variant="danger" onClick={handleDelete}>Delete</Button>
        </Modal.Footer>
      </Modal>

      <style>{`
        .report-admin-list { display: flex; flex-direction: column; gap: 0.75rem; }
        .report-admin-item {
          background: var(--bg-card);
          border: 1px solid var(--border-slate);
          border-radius: var(--radius-lg);
          padding: 1rem 1.25rem;
          transition: all var(--transition-fast);
        }
        .report-admin-item.draft {
          background: rgba(100, 116, 139, 0.08);
          opacity: 0.85;
        }
        .report-admin-item:hover { border-color: rgba(56,189,248,0.4); }
        .report-title { color: var(--text-light); font-weight: 600; font-size: 1rem; }
        .report-meta {
          color: var(--text-gray);
          font-size: 0.8rem;
        }
        .report-meta i { color: var(--accent-sky); }
        .report-stats {
          display: flex;
          flex-wrap: wrap;
          gap: 1rem;
          color: var(--text-gray);
          font-size: 0.85rem;
        }
        .report-stats strong { color: var(--text-light); margin-right: 0.25rem; }
        .report-snippet {
          color: var(--text-gray);
          font-size: 0.85rem;
          line-height: 1.5;
        }
        .min-w-0 { min-width: 0; }

        /* Reuse dark modal styles */
        .admin-modal-dark .modal-content {
          background: var(--bg-card);
          border: 1px solid var(--border-slate);
          color: var(--text-light);
        }
        .admin-modal-dark .modal-header,
        .admin-modal-dark .modal-footer {
          border-color: var(--border-slate);
        }
        .admin-modal-dark .btn-close { filter: invert(1); opacity: 0.5; }
        .admin-modal-dark .form-label {
          color: var(--text-light);
          font-size: 0.875rem;
          font-weight: 600;
        }
        .admin-modal-dark .form-control,
        .admin-modal-dark .form-select {
          background: var(--primary-slate);
          border: 1px solid var(--border-slate);
          color: var(--text-light);
        }
        .admin-modal-dark .form-control:focus,
        .admin-modal-dark .form-select:focus {
          border-color: var(--accent-sky);
          box-shadow: 0 0 0 3px rgba(56,189,248,0.15);
          background: var(--primary-slate);
          color: var(--text-light);
        }
      `}</style>
    </div>
  );
}
