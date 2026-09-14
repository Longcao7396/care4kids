import React, { useState, useEffect, useCallback } from 'react';
import {
  Container, Row, Col, Card, Alert, Button, Modal, Form,
  Spinner, Badge, ProgressBar, Table
} from 'react-bootstrap';
import api from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';
import AdminPageFrame from '../../components/AdminPageFrame';
import './AdminForm.css';

/* ── Constants ────────────────────────────── */
const CAMPAIGN_STATUSES = ['Active', 'Ongoing', 'Completed', 'Cancelled', 'Upcoming'];
const CAMPAIGN_STATUS_COLORS = {
  Active: 'success', Ongoing: 'primary', Completed: 'secondary',
  Cancelled: 'danger', Upcoming: 'info',
};

/* ── Validation ────────────────────────────── */
function validate(data, isUpdate = false) {
  const errs = {};
  if (!isUpdate && !data.campaignName?.trim())
    errs.campaignName = 'Campaign name is required.';
  if (!isUpdate && !data.causeId)
    errs.causeId = 'Please select a cause.';
  if (!isUpdate && !data.goalAmount)
    errs.goalAmount = 'Goal amount is required.';
  if (data.goalAmount && isNaN(Number(data.goalAmount)))
    errs.goalAmount = 'Must be a valid number.';
  if (data.startDate && data.endDate && new Date(data.endDate) < new Date(data.startDate))
    errs.endDate = 'End date must be after start date.';
  return errs;
}

const EMPTY_FORM = {
  causeId: '', campaignName: '', campaignCode: '', description: '',
  goalAmount: '', startDate: '', endDate: '', imageUrl: '',
  beneficiariesCount: '', location: '',
  isFeatured: false, displayOrder: 0,
};

/* ── Campaign Form Modal ────────────────────── */
function CampaignFormModal({ show, editing, initial, causes, onSave, onClose }) {
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (show) {
      setForm(editing ? {
        ...EMPTY_FORM,
        ...initial,
        causeId: initial.causeId ?? '',
        goalAmount: initial.goalAmount ?? '',
        beneficiariesCount: initial.beneficiariesCount ?? '',
        startDate: initial.startDate ? initial.startDate.slice(0, 10) : '',
        endDate: initial.endDate ? initial.endDate.slice(0, 10) : '',
      } : EMPTY_FORM);
      setErrors({});
    }
  }, [show, editing, initial]);

  const set = (field) => (e) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errs = validate(form, !!editing);
    if (Object.keys(errs).length) { setErrors(errs); return; }
    setSaving(true);
    try {
      await onSave({
        ...form,
        causeId: form.causeId ? parseInt(form.causeId) : null,
        goalAmount: form.goalAmount ? parseFloat(form.goalAmount) : null,
        beneficiariesCount: form.beneficiariesCount ? parseInt(form.beneficiariesCount) : null,
        targetBeneficiaries: form.targetBeneficiaries ? parseInt(form.targetBeneficiaries) : null,
        displayOrder: parseInt(form.displayOrder) || 0,
      });
    } finally {
      setSaving(false);
    }
  };

  const field = (label, field, as = 'text', placeholder = '') => (
    <Form.Group className="mb-3">
      <Form.Label>{label}</Form.Label>
      <Form.Control
        type={as} value={form[field]} onChange={set(field)}
        placeholder={placeholder} isInvalid={!!errors[field]}
      />
      {errors[field] && <Form.Control.Feedback type="invalid">{errors[field]}</Form.Control.Feedback>}
    </Form.Group>
  );

  return (
    <Modal show={show} onHide={onClose} size="lg" centered className="admin-modal-dark">
      <Modal.Header closeButton>
        <Modal.Title>
          <i className={`bi ${editing ? 'bi-pencil-square' : 'bi-plus-circle'} me-2 text-accent`}></i>
          {editing ? 'Edit Campaign' : 'Create Campaign'}
        </Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          <Row>
            <Col md={8}>{field('Campaign Name *', 'campaignName', 'text', 'Clean Water Initiative')}</Col>
            <Col md={4}>{field('Campaign Code', 'campaignCode', 'text', 'CWI-2025')}</Col>
          </Row>
          <Row>
            <Col md={4}>
              <Form.Group className="mb-3">
                <Form.Label>Cause *</Form.Label>
                <Form.Select
                  value={form.causeId} onChange={set('causeId')}
                  isInvalid={!!errors.causeId}
                >
                  <option value="">— Select Cause —</option>
                  {(causes || []).map((c) => (
                    <option key={c.causeId} value={String(c.causeId)}>{c.causeName}</option>
                  ))}
                </Form.Select>
                {errors.causeId && <Form.Control.Feedback type="invalid">{errors.causeId}</Form.Control.Feedback>}
              </Form.Group>
            </Col>
            <Col md={4}>{field('Goal Amount (VND) *', 'goalAmount', 'number', '50000000')}</Col>
            <Col md={4}>{field('Display Order', 'displayOrder', 'number', '0')}</Col>
          </Row>
          {field('Description', 'description', 'textarea', 'Describe the campaign goals and impact...')}
          <Row>
            <Col md={4}>{field('Start Date', 'startDate', 'date')}</Col>
            <Col md={4}>{field('End Date', 'endDate', 'date')}</Col>
            <Col md={4}>{field('Location', 'location', 'text', 'Ho Chi Minh City')}</Col>
          </Row>
          <Row>
            <Col md={6}>{field('Image URL', 'imageUrl', 'url', 'https://...')}</Col>
            <Col md={3}>{field('Beneficiaries', 'beneficiariesCount', 'number', '500')}</Col>
            <Col md={3}>{field('Target Beneficiaries', 'targetBeneficiaries', 'number', '1000')}</Col>
            <Col md={3}>
              <Form.Check
                type="switch" id="camp-is-featured" label="Featured"
                checked={form.isFeatured}
                onChange={(e) => setForm((f) => ({ ...f, isFeatured: e.target.checked }))}
                className="mt-4"
              />
            </Col>
          </Row>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onClose} disabled={saving}>Cancel</Button>
          <Button variant="primary" type="submit" disabled={saving}>
            {saving ? (
              <><Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />Saving...</>
            ) : (
              <><i className={`bi ${editing ? 'bi-check-circle' : 'bi-plus-circle'} me-2`}></i>
              {editing ? 'Update Campaign' : 'Create Campaign'}</>
            )}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
}

/* ── Admin Campaign Page ────────────────────── */
export default function AdminCampaignPage() {
  const { user } = useAuth();
  const canAccess = user?.role === 'Admin' || user?.role === 'SuperAdmin';

  const [campaigns, setCampaigns] = useState([]);
  const [causes, setCauses] = useState([]);
  const [, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [detailItem, setDetailItem] = useState(null);
  const [deleteConfirm, setDeleteConfirm] = useState(null);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [causeFilter, setCauseFilter] = useState('');
  const [activeTab, setActiveTab] = useState('campaigns');

  const fetchCampaigns = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await api.get('/campaigns', { params: { pageSize: 100, page: 1 } });
      if (res.data.success) setCampaigns(res.data.data || []);
    } catch {
      setError('Failed to load campaigns.');
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchCauses = useCallback(async () => {
    try {
      const res = await api.get('/causes', { params: { activeOnly: false } });
      if (res.data.success) setCauses(res.data.data || []);
    } catch { /* non-fatal */ }
  }, []);

  useEffect(() => {
    if (canAccess) { fetchCampaigns(); fetchCauses(); }
  }, [canAccess, fetchCampaigns, fetchCauses]);

  const openAdd = () => { setEditing(null); setShowForm(true); };
  const openEdit = (item) => { setEditing(item); setShowForm(true); };
  const openDetail = async (item) => {
    setDetailItem(null);
    try {
      const res = await api.get(`/campaigns/${item.campaignId}`);
      if (res.data.success) setDetailItem(res.data.data);
    } catch { setDetailItem(item); }
  };

  const handleSave = async (payload) => {
    if (editing) {
      await api.put(`/campaigns/${editing.campaignId}`, payload);
    } else {
      await api.post('/campaigns', payload);
    }
    setShowForm(false);
    fetchCampaigns();
  };

  const handleDelete = async () => {
    if (!deleteConfirm) return;
    try {
      await api.delete(`/campaigns/${deleteConfirm}`);
      setSuccess('Campaign deleted.');
    } catch (err) {
      setError(err.response?.data?.message || 'Delete failed.');
    } finally {
      setDeleteConfirm(null);
      fetchCampaigns();
    }
  };

  const filtered = campaigns.filter((c) => {
    const s = search.toLowerCase();
    const matchSearch = !s ||
      c.campaignName?.toLowerCase().includes(s) ||
      c.campaignCode?.toLowerCase().includes(s);
    const matchStatus = !statusFilter || c.status === statusFilter;
    const matchCause = !causeFilter || c.causeId === parseInt(causeFilter);
    return matchSearch && matchStatus && matchCause;
  });

  const fmtVnd = (n) => n != null
    ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(n)
    : '—';

  if (!canAccess) {
    return (
      <Container className="py-5">
        <Alert variant="danger">You do not have permission to access this page.</Alert>
      </Container>
    );
  }

  return (
    <AdminPageFrame
      eyebrow="Fundraising · Campaigns"
      title="Campaign management"
      sub="Create, manage and monitor fundraising campaigns and the causes they support."
      tabs={[
        { id: 'campaigns', label: 'Campaigns', count: campaigns.length, icon: 'bi-flag-fill' },
        { id: 'causes', label: 'Causes', count: causes.length, icon: 'bi-tags-fill' },
      ]}
      activeTab={activeTab}
      onTabChange={setActiveTab}
      error={error}
      success={success}
      actions={activeTab === 'campaigns' ? (
        <button type="button" className="af-btn af-btn-primary" onClick={openAdd}>
          <i className="bi bi-plus-circle" aria-hidden="true"></i>
          <span>New campaign</span>
        </button>
      ) : null}
    >

      {/* ── CAMPAIGNS TAB ── */}
      {activeTab === 'campaigns' && (
        <>
          <div className="af-toolbar">
            <div className="af-search">
              <span className="af-search-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
                </svg>
              </span>
              <input
                type="text"
                className="af-search-input"
                placeholder="Search by name or code…"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
              {search && (
                <button type="button" className="af-search-clear" onClick={() => setSearch('')} aria-label="Clear search">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                </button>
              )}
            </div>

            <select className="af-filter" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="">All statuses</option>
              {CAMPAIGN_STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>

            <select className="af-filter" value={causeFilter} onChange={(e) => setCauseFilter(e.target.value)}>
              <option value="">All causes</option>
              {causes.map((c) => <option key={c.causeId} value={String(c.causeId)}>{c.causeName}</option>)}
            </select>

            <div className="af-toolbar-spacer" />
            <span className="af-meta-count">{filtered.length} of {campaigns.length}</span>
          </div>

          {filtered.length === 0 ? (
            <div className="af-empty">
              <div className="af-empty-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
                </svg>
              </div>
              <h3 className="af-empty-title">No campaigns found</h3>
              <p className="af-empty-text">
                {(search || statusFilter || causeFilter)
                  ? 'Try adjusting your filters.'
                  : 'Click "New campaign" to create your first fundraising campaign.'}
              </p>
              {!search && !statusFilter && !causeFilter && (
                <button type="button" className="af-btn af-btn-primary" onClick={openAdd}>
                  <i className="bi bi-plus-circle" aria-hidden="true"></i>
                  <span>Create first campaign</span>
                </button>
              )}
            </div>
          ) : (
            <div className="af-panel">
              <div className="af-table-wrap">
                <table className="af-table">
                  <thead>
                    <tr>
                      <th>Campaign</th>
                      <th>Cause</th>
                      <th>Goal / Raised</th>
                      <th style={{ minWidth: 160 }}>Progress</th>
                      <th>Donors</th>
                      <th>Status</th>
                      <th className="text-end">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filtered.map((c) => (
                      <tr key={c.campaignId}>
                        <td>
                          <div className="af-cell-strong">{c.campaignName}</div>
                          <div className="af-cell-meta">
                            {c.campaignCode ? <span className="af-code">{c.campaignCode}</span> : null}
                            {c.location && <span style={{ marginLeft: 6 }}><i className="bi bi-geo-alt"></i> {c.location}</span>}
                          </div>
                        </td>
                        <td>
                          <span className="af-tag">{c.cause?.causeName || '—'}</span>
                          {c.isFeatured && <span className="af-tag af-tag-accent" style={{ marginLeft: 6 }}>★ Featured</span>}
                        </td>
                        <td>
                          <div className="af-cell-strong">{fmtVnd(c.raisedAmount)}</div>
                          <div className="af-cell-meta">of {fmtVnd(c.goalAmount)}</div>
                          <div className="af-cell-meta">
                            {c.startDate && <>Start: {new Date(c.startDate).toLocaleDateString('en-GB')}</>}
                          </div>
                        </td>
                        <td>
                          <div className="af-progress">
                            <div className="af-progress-track">
                              <div className="af-progress-fill" style={{ width: `${Math.min(c.percentageReached || 0, 100)}%` }} />
                            </div>
                            <span className="af-progress-num">{(c.percentageReached || 0).toFixed(0)}%</span>
                          </div>
                        </td>
                        <td>
                          <div className="af-cell-strong">{c.donorCount || 0}</div>
                          <div className="af-cell-meta">donors</div>
                        </td>
                        <td>
                          <span className={`af-pill af-pill-${(c.status || '').toLowerCase()}`}>
                            {c.status || '—'}
                          </span>
                        </td>
                        <td className="text-end">
                          <div className="af-row-actions">
                            <button type="button" className="af-icon-btn" onClick={() => openDetail(c)} title="View details" aria-label="View details">
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                            </button>
                            <button type="button" className="af-icon-btn" onClick={() => openEdit(c)} title="Edit" aria-label="Edit">
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                            </button>
                            {user?.role === 'SuperAdmin' && (
                              <button type="button" className="af-icon-btn danger" onClick={() => setDeleteConfirm(c.campaignId)} title="Delete" aria-label="Delete">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6M14 11v6"/><path d="M9 6V4a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2"/></svg>
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}

      {/* ── CAUSES TAB ── */}
      {activeTab === 'causes' && <CausesAdminTab causes={causes} setCauses={setCauses} />}

      {/* ── CAMPAIGN DETAIL MODAL ── */}
      <Modal show={!!detailItem} onHide={() => setDetailItem(null)} size="lg" centered className="admin-modal-dark">
        <Modal.Header closeButton>
          <Modal.Title>{detailItem?.campaignName}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {detailItem ? (
            <Row>
              <Col md={5}>
                {detailItem.imageUrl && (
                  <img src={detailItem.imageUrl} alt={detailItem.campaignName}
                    style={{ width: '100%', borderRadius: 'var(--radius-md)', marginBottom: '1rem' }}
                    onError={(e) => { e.currentTarget.style.display = 'none'; }} />
                )}
                <div className="d-flex flex-wrap gap-2 mb-3">
                  <Badge bg={CAMPAIGN_STATUS_COLORS[detailItem.status] || 'secondary'}>
                    {detailItem.status}
                  </Badge>
                  {detailItem.cause && (
                    <Badge bg="info">{detailItem.cause.causeName}</Badge>
                  )}
                  {detailItem.isFeatured && <Badge bg="warning" text="dark">★ Featured</Badge>}
                </div>
                <div className="small">
                  {detailItem.location && (
                    <div className="mb-1"><i className="bi bi-geo-alt-fill me-1 text-accent"></i>{detailItem.location}</div>
                  )}
                  {detailItem.startDate && (
                    <div className="mb-1"><i className="bi bi-calendar-event me-1 text-accent"></i>
                      {new Date(detailItem.startDate).toLocaleDateString('en-GB')}
                      {detailItem.endDate && ` – ${new Date(detailItem.endDate).toLocaleDateString('en-GB')}`}
                    </div>
                  )}
                    {detailItem.beneficiariesCount && (
                      <div className="mb-1"><i className="bi bi-people-fill me-1 text-accent"></i>
                        {detailItem.beneficiariesCount.toLocaleString()} beneficiaries
                      </div>
                    )}
                    {detailItem.campaignCode && (
                      <div className="mb-1"><i className="bi bi-hash me-1 text-accent"></i>{detailItem.campaignCode}</div>
                    )}
                  </div>
                </Col>
                <Col md={7}>
                  <h6 className="text-light mb-2">Fundraising Progress</h6>
                  <div className="mb-1 fw-bold" style={{ color: 'var(--accent-sky)', fontSize: '1.1rem' }}>
                    {fmtVnd(detailItem.raisedAmount)} <span className="text-muted">/ {fmtVnd(detailItem.goalAmount)}</span>
                  </div>
                  <ProgressBar
                    now={Math.min(detailItem.percentageReached || 0, 100)}
                    variant={detailItem.percentageReached >= 100 ? 'success' : detailItem.percentageReached >= 50 ? 'info' : 'primary'}
                    className="mb-3"
                    style={{ height: 12 }}
                  />
                  <div className="d-flex justify-content-between small text-muted mb-3">
                    <span>{detailItem.percentageReached?.toFixed(1)}% funded</span>
                    <span>{detailItem.donorCount || 0} donors</span>
                  </div>
                  {detailItem.description && (
                    <>
                      <h6 className="text-light mb-2">Description</h6>
                      <p className="text-muted small">{detailItem.description}</p>
                    </>
                  )}
                  {detailItem.recentDonations?.length > 0 && (
                    <>
                      <h6 className="text-light mb-2 mt-3">Recent Donations</h6>
                      <div className="small">
                        {detailItem.recentDonations.map((d, i) => (
                          <div key={i} className="d-flex justify-content-between py-1 border-bottom" style={{ borderColor: 'var(--border-slate) !important' }}>
                            <span className="text-muted">{d.fullName || 'Anonymous'}</span>
                            <span style={{ color: 'var(--accent-sky)' }}>{fmtVnd(d.amount)}</span>
                          </div>
                        ))}
                      </div>
                    </>
                  )}
                </Col>
              </Row>
            ) : (
              <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>
            )}
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setDetailItem(null)}>Close</Button>
          </Modal.Footer>
        </Modal>

        {/* ── CAMPAIGN FORM MODAL ── */}
        <CampaignFormModal
          show={showForm}
          editing={!!editing}
          initial={editing || EMPTY_FORM}
          causes={causes}
          onSave={handleSave}
          onClose={() => setShowForm(false)}
        />

        {/* ── DELETE CONFIRM ── */}
        <Modal show={!!deleteConfirm} onHide={() => setDeleteConfirm(null)} centered className="admin-modal-dark">
          <Modal.Header closeButton>
            <Modal.Title>
              <i className="bi bi-exclamation-triangle-fill text-danger me-2"></i>
              Delete Campaign
            </Modal.Title>
          </Modal.Header>
          <Modal.Body>
            Are you sure? Campaigns with donations cannot be deleted — only cancelled.
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setDeleteConfirm(null)}>Cancel</Button>
            <Button variant="danger" onClick={handleDelete}>Delete</Button>
          </Modal.Footer>
        </Modal>

      <style>{`
        .cms-tabs-bar {
          display: flex;
          gap: 0.5rem;
          padding: 0.5rem;
          background: var(--bg-card);
          border: 1px solid var(--border-slate);
          border-radius: var(--radius-lg);
          margin-bottom: 1.5rem;
          flex-wrap: wrap;
        }
        .cms-tab {
          display: inline-flex; align-items: center; gap: 0.4rem;
          padding: 0.55rem 1rem;
          border-radius: var(--radius-md);
          border: 1px solid transparent;
          background: transparent;
          color: var(--text-gray);
          font-size: 0.875rem; font-weight: 600;
          cursor: pointer;
          white-space: nowrap;
          transition: all var(--transition-fast);
        }
        .cms-tab:hover { background: rgba(56,189,248,0.08); color: var(--accent-sky); }
        .cms-tab.active { background: var(--accent-sky); color: var(--primary-navy); }
        .inactive-row td { opacity: 0.5; }
        .admin-table thead th {
          background: var(--primary-slate);
          border-bottom: 1px solid var(--border-slate);
          color: var(--text-gray);
          font-size: 0.78rem;
          text-transform: uppercase;
          letter-spacing: 0.05em;
          padding: 0.75rem 1rem;
          white-space: nowrap;
        }
        .admin-table tbody td {
          padding: 0.75rem 1rem;
          border-bottom: 1px solid var(--border-slate);
          vertical-align: middle;
        }
        .admin-table tbody tr:last-child td { border-bottom: none; }
        .admin-table tbody tr:hover td { background: rgba(56,189,248,0.03); }
        .admin-modal-dark .modal-content { background: var(--bg-card); border: 1px solid var(--border-slate); color: var(--text-light); }
        .admin-modal-dark .modal-header, .admin-modal-dark .modal-footer { border-color: var(--border-slate); }
        .admin-modal-dark .btn-close { filter: invert(1); opacity: 0.5; }
        .admin-modal-dark .form-label { color: var(--text-light); font-size: 0.875rem; font-weight: 600; }
        .admin-modal-dark .form-control, .admin-modal-dark .form-select {
          background: var(--primary-slate); border: 1px solid var(--border-slate); color: var(--text-light);
        }
        .admin-modal-dark .form-control:focus, .admin-modal-dark .form-select:focus {
          border-color: var(--accent-sky);
          box-shadow: 0 0 0 3px rgba(56,189,248,0.15);
          background: var(--primary-slate); color: var(--text-light);
        }
      `}</style>
    </AdminPageFrame>
  );
}

/* ── Causes Admin Tab ─────────────────────── */
function CausesAdminTab({ causes: externalCauses, setCauses: setExternalCauses }) {
  const [causes, setCauses] = useState(externalCauses || []);
  const [loading, setLoading] = useState(externalCauses ? false : true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({});
  const [deleteConfirm, setDeleteConfirm] = useState(null);
  const isSuperAdmin = JSON.parse(localStorage.getItem('giveaid_user') || '{}')?.role === 'SuperAdmin';

  const load = useCallback(async () => {
    try {
      setLoading(true);
      const res = await api.get('/causes', { params: { activeOnly: false } });
      if (res.data.success) setCauses(res.data.data || []);
    } catch {
      setError('Failed to load causes.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => {
    setEditing(null);
    setForm({ causeName: '', causeCode: '', description: '', imageUrl: '', icon: '', targetAmount: '', displayOrder: 0, isActive: true });
    setShowForm(true);
  };

  const openEdit = (c) => {
    setEditing(c);
    setForm({
      causeName: c.causeName || '', causeCode: c.causeCode || '',
      description: c.description || '', imageUrl: c.imageUrl || '',
      icon: c.icon || '', targetAmount: c.targetAmount ?? '',
      displayOrder: c.displayOrder || 0, isActive: c.isActive !== false,
    });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = { ...form, targetAmount: form.targetAmount ? parseFloat(form.targetAmount) : null, displayOrder: parseInt(form.displayOrder) || 0 };
      if (editing) {
        await api.put(`/causes/${editing.causeId}`, payload);
      } else {
        await api.post('/causes', payload);
      }
      setSuccess(editing ? 'Cause updated.' : 'Cause created.');
      setShowForm(false);
      load();
    } catch {
      setError('Save failed.');
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirm) return;
    try {
      await api.delete(`/causes/${deleteConfirm}`);
      setSuccess('Cause deactivated.');
    } catch {
      setError('Delete failed.');
    } finally {
      setDeleteConfirm(null);
      load();
    }
  };

  const fmtVnd = (n) => n != null
    ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(n)
    : '—';

  return (
    <div>
      {error && <Alert variant="danger" dismissible onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert variant="success" dismissible onClose={() => setSuccess(null)}>{success}</Alert>}

      <div className="d-flex justify-content-between align-items-center mb-3">
        <h4 className="text-light mb-0">Causes / Categories</h4>
        {isSuperAdmin && (
          <Button variant="primary" onClick={openCreate}>
            <i className="bi bi-plus-circle me-2"></i>Add Cause
          </Button>
        )}
      </div>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>
      ) : causes.length === 0 ? (
        <Alert variant="info">No causes yet.</Alert>
      ) : (
        <Card>
          <Table responsive hover className="mb-0 align-middle">
            <thead>
              <tr>
                <th>Cause</th>
                <th>Code</th>
                <th>Goal / Raised</th>
                <th>Progress</th>
                <th>Status</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {causes.map((c) => (
                <tr key={c.causeId} className={!c.isActive ? 'inactive-row' : ''}>
                  <td>
                    <div className="fw-semibold" style={{ color: 'var(--text-light)' }}>{c.causeName}</div>
                    {c.description && <div className="small text-muted">{c.description.slice(0, 60)}{c.description.length > 60 ? '…' : ''}</div>}
                  </td>
                  <td><code>{c.causeCode || '—'}</code></td>
                  <td>
                    <div className="small fw-semibold">{fmtVnd(c.raisedAmount)}</div>
                    <div className="small text-muted">of {fmtVnd(c.targetAmount)}</div>
                  </td>
                  <td style={{ minWidth: 100 }}>
                    <div className="small mb-1">{c.percentageReached?.toFixed(0) || 0}%</div>
                    <ProgressBar
                      now={Math.min(c.percentageReached || 0, 100)}
                      variant={c.percentageReached >= 100 ? 'success' : c.percentageReached >= 50 ? 'info' : 'primary'}
                      style={{ height: 5 }}
                    />
                  </td>
                  <td>
                    <Badge bg={c.isActive ? 'success' : 'secondary'} style={{ fontSize: '0.72rem' }}>
                      {c.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                  </td>
                  <td className="text-end">
                    <Button variant="outline-primary" size="sm" onClick={() => openEdit(c)}>Edit</Button>
                    {isSuperAdmin && (
                      <Button variant="outline-danger" size="sm" className="ms-1" onClick={() => setDeleteConfirm(c.causeId)}>Delete</Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </Card>
      )}

      {/* Cause Form Modal */}
      <Modal show={showForm} onHide={() => setShowForm(false)} centered className="admin-modal-dark">
        <Modal.Header closeButton>
          <Modal.Title>{editing ? 'Edit Cause' : 'Add Cause'}</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            <div className="row g-3">
              <div className="col-md-6">
                <Form.Group><Form.Label>Cause Name *</Form.Label>
                  <Form.Control value={form.causeName || ''} onChange={(e) => setForm((f) => ({ ...f, causeName: e.target.value }))} required />
                </Form.Group>
              </div>
              <div className="col-md-6">
                <Form.Group><Form.Label>Cause Code</Form.Label>
                  <Form.Control value={form.causeCode || ''} onChange={(e) => setForm((f) => ({ ...f, causeCode: e.target.value }))} placeholder="EDU" />
                </Form.Group>
              </div>
              <div className="col-12">
                <Form.Group><Form.Label>Description</Form.Label>
                  <Form.Control as="textarea" rows={2} value={form.description || ''} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
                </Form.Group>
              </div>
              <div className="col-md-6">
                <Form.Group><Form.Label>Image URL</Form.Label>
                  <Form.Control value={form.imageUrl || ''} onChange={(e) => setForm((f) => ({ ...f, imageUrl: e.target.value }))} />
                </Form.Group>
              </div>
              <div className="col-md-3">
                <Form.Group><Form.Label>Icon (Bootstrap Icons)</Form.Label>
                  <Form.Control value={form.icon || ''} onChange={(e) => setForm((f) => ({ ...f, icon: e.target.value }))} placeholder="book-fill" />
                </Form.Group>
              </div>
              <div className="col-md-3">
                <Form.Group><Form.Label>Target Amount (VND)</Form.Label>
                  <Form.Control type="number" value={form.targetAmount ?? ''} onChange={(e) => setForm((f) => ({ ...f, targetAmount: e.target.value }))} />
                </Form.Group>
              </div>
              <div className="col-md-3">
                <Form.Group><Form.Label>Display Order</Form.Label>
                  <Form.Control type="number" value={form.displayOrder || 0} onChange={(e) => setForm((f) => ({ ...f, displayOrder: e.target.value }))} />
                </Form.Group>
              </div>
              <div className="col-md-3">
                <Form.Check type="switch" label="Active" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} className="mt-4" />
              </div>
            </div>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShowForm(false)}>Cancel</Button>
            <Button variant="primary" type="submit">{editing ? 'Update' : 'Create'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>

      <Modal show={!!deleteConfirm} onHide={() => setDeleteConfirm(null)} centered className="admin-modal-dark">
        <Modal.Header closeButton>
          <Modal.Title><i className="bi bi-exclamation-triangle-fill text-danger me-2"></i>Deactivate Cause</Modal.Title>
        </Modal.Header>
        <Modal.Body>Are you sure you want to deactivate this cause?</Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setDeleteConfirm(null)}>Cancel</Button>
          <Button variant="danger" onClick={handleDelete}>Deactivate</Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
}
