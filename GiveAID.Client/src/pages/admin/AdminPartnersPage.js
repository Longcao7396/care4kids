import React, { useState, useEffect, useCallback } from 'react';
import {
  Container, Row, Col, Alert, Button,
  Modal, Form, Spinner
} from 'react-bootstrap';
import { supportersService } from '../../services';
import { useAuth } from '../../contexts/AuthContext';
import AdminPageFrame from '../../components/AdminPageFrame';
import '../admin/AdminForm.css';

const TYPE_OPTIONS = [
  'Supporter', 'Partner', 'NGO', 'Corporate', 'Government', 'Other'
];
const CONTRIBUTION_TYPES = ['Financial', 'In-Kind', 'Volunteering', 'Sponsorship', 'Other'];

/* ── Validation ───────────────────────────────────── */
function validate(data) {
  const errs = {};
  if (!data.organizationName?.trim())
    errs.organizationName = 'Organization name is required.';
  if (!data.organizationType?.trim())
    errs.organizationType = 'Organization type is required.';
  if (data.contactEmail && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.contactEmail))
    errs.contactEmail = 'Invalid email address.';
  if (data.websiteUrl && !/^https?:\/\/.+/.test(data.websiteUrl))
    errs.websiteUrl = 'Website must start with http:// or https://';
  if (data.contributionAmount && isNaN(Number(data.contributionAmount)))
    errs.contributionAmount = 'Contribution amount must be a number.';
  return errs;
}

const EMPTY_FORM = {
  organizationName: '',
  organizationType: 'Supporter',
  description: '',
  logoUrl: '',
  websiteUrl: '',
  contactEmail: '',
  contactPhone: '',
  address: '',
  registrationNumber: '',
  mission: '',
  vision: '',
  contributionAmount: '',
  contributionType: '',
  isActive: true,
  isFeatured: false,
  displayOrder: 0,
};

/* ── Partner Form Modal ────────────────────────────── */
function PartnerFormModal({ show, editing, initial, onSave, onClose }) {
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (show) {
      setForm(editing ? { ...initial, contributionAmount: initial.contributionAmount ?? '' } : EMPTY_FORM);
      setErrors({});
    }
  }, [show, editing, initial]);

  const set = (field) => (e) =>
    setForm((f) => ({ ...f, [field]: e.target.value }));

  const setBool = (field) => (e) =>
    setForm((f) => ({ ...f, [field]: e.target.checked }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errs = validate(form);
    if (Object.keys(errs).length) { setErrors(errs); return; }
    setSaving(true);
    try {
      const payload = {
        ...form,
        contributionAmount: form.contributionAmount ? parseFloat(form.contributionAmount) : null,
        displayOrder: parseInt(form.displayOrder) || 0,
      };
      await onSave(payload);
    } finally {
      setSaving(false);
    }
  };

  const fieldGroup = (label, field, as = 'input', rows = 3) => (
    <Form.Group className="mb-3" controlId={`form-${field}`}>
      <Form.Label>{label}</Form.Label>
      {as === 'textarea' ? (
        <Form.Control
          as="textarea"
          rows={rows}
          value={form[field]}
          onChange={set(field)}
          isInvalid={!!errors[field]}
        />
      ) : (
        <Form.Control
          type={as === 'input' ? 'text' : as}
          value={form[field]}
          onChange={set(field)}
          isInvalid={!!errors[field]}
        />
      )}
      {errors[field] && (
        <Form.Control.Feedback type="invalid">{errors[field]}</Form.Control.Feedback>
      )}
    </Form.Group>
  );

  return (
    <Modal show={show} onHide={onClose} size="lg" centered className="admin-modal-dark">
      <Modal.Header closeButton>
        <Modal.Title>
          <i className={`bi ${editing ? 'bi-pencil-square' : 'bi-plus-circle'} me-2 text-accent`}></i>
          {editing ? 'Edit Partner' : 'Add New Partner'}
        </Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body>
          <Row>
            <Col md={8}>
              {fieldGroup('Organization Name *', 'organizationName')}
            </Col>
            <Col md={4}>
              <Form.Group className="mb-3" controlId="form-type">
                <Form.Label>Organization Type *</Form.Label>
                <Form.Select value={form.organizationType} onChange={set('organizationType')}
                  isInvalid={!!errors.organizationType}>
                  {TYPE_OPTIONS.map((t) => <option key={t} value={t}>{t}</option>)}
                </Form.Select>
                {errors.organizationType && (
                  <Form.Control.Feedback type="invalid">{errors.organizationType}</Form.Control.Feedback>
                )}
              </Form.Group>
            </Col>
          </Row>

          {fieldGroup('Description', 'description', 'textarea', 3)}
          {fieldGroup('Logo URL', 'logoUrl')}
          <Row>
            <Col md={6}>{fieldGroup('Website URL', 'websiteUrl')}</Col>
            <Col md={6}>{fieldGroup('Registration Number', 'registrationNumber')}</Col>
          </Row>
          <Row>
            <Col md={6}>{fieldGroup('Contact Email', 'contactEmail', 'email')}</Col>
            <Col md={6}>{fieldGroup('Contact Phone', 'contactPhone')}</Col>
          </Row>
          {fieldGroup('Address', 'address')}
          <Row>
            <Col md={6}>{fieldGroup('Mission', 'mission', 'textarea', 2)}</Col>
            <Col md={6}>{fieldGroup('Vision', 'vision', 'textarea', 2)}</Col>
          </Row>
          <Row>
            <Col md={4}>
              {fieldGroup('Contribution Amount (VND)', 'contributionAmount', 'number')}
            </Col>
            <Col md={4}>
              <Form.Group className="mb-3" controlId="form-contrib-type">
                <Form.Label>Contribution Type</Form.Label>
                <Form.Select value={form.contributionType} onChange={set('contributionType')}>
                  <option value="">— Select —</option>
                  {CONTRIBUTION_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                </Form.Select>
              </Form.Group>
            </Col>
            <Col md={4}>
              {fieldGroup('Display Order', 'displayOrder', 'number')}
            </Col>
          </Row>

          <Row className="mt-2">
            <Col md={4}>
              <Form.Check
                type="switch"
                id="is-active"
                label="Active"
                checked={form.isActive}
                onChange={setBool('isActive')}
              />
            </Col>
            <Col md={4}>
              <Form.Check
                type="switch"
                id="is-featured"
                label="Featured"
                checked={form.isFeatured}
                onChange={setBool('isFeatured')}
              />
            </Col>
          </Row>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onClose} disabled={saving}>Cancel</Button>
          <Button variant="primary" type="submit" disabled={saving}>
            {saving ? (
              <><Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />
              Saving...</>
            ) : (
              <><i className={`bi ${editing ? 'bi-check-circle' : 'bi-plus-circle'} me-2`}></i>
              {editing ? 'Update Partner' : 'Add Partner'}</>
            )}
          </Button>
        </Modal.Footer>
      </Form>

      <style>{`
        .admin-modal-dark .modal-content {
          background: var(--bg-card);
          border: 1px solid var(--border-slate);
          color: var(--text-light);
        }
        .admin-modal-dark .modal-header {
          border-bottom: 1px solid var(--border-slate);
          background: rgba(56,189,248,0.04);
        }
        .admin-modal-dark .modal-footer {
          border-top: 1px solid var(--border-slate);
        }
        .admin-modal-dark .btn-close {
          filter: invert(1);
          opacity: 0.5;
        }
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
        .admin-modal-dark .form-control.is-invalid,
        .admin-modal-dark .form-select.is-invalid {
          border-color: #EF4444;
        }
      `}</style>
    </Modal>
  );
}

/* ── Admin Partners Page ───────────────────────────── */
function AdminPartnersPage() {
  const { user } = useAuth();
  const canAccess = user?.role === 'Admin' || user?.role === 'SuperAdmin';

  const [partners, setPartners] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState(null); // null = add, else edit
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [deleteConfirm, setDeleteConfirm] = useState(null);

  const fetchPartners = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const params = { activeOnly: false };
      const response = await supportersService.getAll(params);
      if (response.success) setPartners(response.data || []);
    } catch {
      setError('Failed to load partners.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (canAccess) fetchPartners();
  }, [canAccess, fetchPartners]);

  const openAdd = () => { setEditing(null); setShowForm(true); };
  const openEdit = (p) => { setEditing(p); setShowForm(true); };

  const handleSave = async (payload) => {
    if (editing) {
      await supportersService.update(editing.organizationId, payload);
    } else {
      await supportersService.create(payload);
    }
    setShowForm(false);
    fetchPartners();
  };

  const handleDelete = async () => {
    if (!deleteConfirm) return;
    try {
      await supportersService.remove(deleteConfirm);
    } finally {
      setDeleteConfirm(null);
      fetchPartners();
    }
  };

  const getInitials = (name) => {
    if (!name) return '?';
    return name.split(/\s+/).filter(Boolean).slice(0, 2)
      .map((p) => p[0]).join('').toUpperCase();
  };

  const filtered = partners.filter((p) => {
    const s = search.toLowerCase();
    const matchSearch = !s ||
      p.organizationName?.toLowerCase().includes(s) ||
      p.contactEmail?.toLowerCase().includes(s);
    const matchType = !typeFilter || p.organizationType === typeFilter;
    const matchStatus = !statusFilter ||
      (statusFilter === 'active' && p.isActive) ||
      (statusFilter === 'inactive' && !p.isActive);
    return matchSearch && matchType && matchStatus;
  });

  if (!canAccess) {
    return (
      <Container className="py-5">
        <Alert variant="danger">You do not have permission to access this page.</Alert>
      </Container>
    );
  }

  return (
    <AdminPageFrame
      eyebrow="People · Partners & supporters"
      title="Partner management"
      sub="Manage partner organizations displayed on the Our Partners page."
      error={error}
      actions={
        <button type="button" className="af-btn af-btn-primary" onClick={openAdd}>
          <i className="bi bi-plus-circle" aria-hidden="true"></i>
          <span>Add partner</span>
        </button>
      }
    >
      {/* Toolbar */}
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
            placeholder="Search by name or email…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <button type="button" className="af-search-clear" onClick={() => setSearch('')} aria-label="Clear search">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          )}
        </div>

        <select className="af-filter" value={typeFilter} onChange={(e) => setTypeFilter(e.target.value)}>
          <option value="">All types</option>
          {TYPE_OPTIONS.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>

        <select className="af-filter" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="">All statuses</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </select>

        <div className="af-toolbar-spacer" />
        <span className="af-meta-count">{filtered.length} of {partners.length}</span>
      </div>

      {!loading && filtered.length === 0 ? (
        <div className="af-empty">
          <div className="af-empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/>
            </svg>
          </div>
          <h3 className="af-empty-title">No partners found</h3>
          <p className="af-empty-text">
            {search || typeFilter || statusFilter
              ? 'Try adjusting your filters.'
              : 'Click "Add partner" to onboard your first partner organization.'}
          </p>
          {!search && !typeFilter && !statusFilter && (
            <button type="button" className="af-btn af-btn-primary" onClick={openAdd}>
              <i className="bi bi-plus-circle" aria-hidden="true"></i>
              <span>Add first partner</span>
            </button>
          )}
        </div>
      ) : (
        <div className="af-panel">
          <div className="af-table-wrap">
            <table className="af-table">
              <thead>
                <tr>
                  <th style={{ width: 56 }}>Logo</th>
                  <th>Organization</th>
                  <th>Type</th>
                  <th>Contact</th>
                  <th>Contribution</th>
                  <th style={{ width: 70 }}>Order</th>
                  <th style={{ width: 90 }}>Status</th>
                  <th className="text-end">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((p) => (
                  <tr key={p.organizationId}>
                    <td>
                      {p.logoUrl ? (
                        <img
                          src={p.logoUrl}
                          alt={p.organizationName}
                          className="af-logo"
                          onError={(e) => {
                            e.currentTarget.style.display = 'none';
                            e.currentTarget.parentElement.innerHTML =
                              `<span class="af-logo-fallback">${getInitials(p.organizationName)}</span>`;
                          }}
                        />
                      ) : (
                        <span className="af-logo-fallback">{getInitials(p.organizationName)}</span>
                      )}
                    </td>
                    <td>
                      <div className="af-cell-strong">{p.organizationName}</div>
                      {p.websiteUrl && (
                        <div className="af-cell-meta">
                          <i className="bi bi-box-arrow-up-right"></i>{' '}
                          {p.websiteUrl.replace(/^https?:\/\//, '').slice(0, 32)}
                        </div>
                      )}
                      {p.isFeatured && (
                        <span className="af-tag af-tag-accent" style={{ marginTop: 4 }}>★ Featured</span>
                      )}
                    </td>
                    <td>
                      <span className={`af-tag af-tag-${(p.organizationType || '').toLowerCase()}`}>
                        {p.organizationType}
                      </span>
                    </td>
                    <td>
                      {p.contactEmail && <div className="af-cell-meta">{p.contactEmail}</div>}
                      {p.contactPhone && <div className="af-cell-meta">{p.contactPhone}</div>}
                    </td>
                    <td>
                      {p.contributionAmount != null ? (
                        <>
                          <div className="af-cell-strong">
                            {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(p.contributionAmount)}
                          </div>
                          {p.contributionType && <div className="af-cell-meta">{p.contributionType}</div>}
                        </>
                      ) : (
                        <span className="af-cell-meta">—</span>
                      )}
                    </td>
                    <td>
                      <span className="af-code">{p.displayOrder}</span>
                    </td>
                    <td>
                      <span className={`af-pill af-pill-${p.isActive ? 'active' : 'completed'}`}>
                        {p.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="text-end">
                      <div className="af-row-actions">
                        <button type="button" className="af-icon-btn" onClick={() => openEdit(p)} title="Edit" aria-label="Edit">
                          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                        </button>
                        {user?.role === 'SuperAdmin' && (
                          <button type="button" className="af-icon-btn danger" onClick={() => setDeleteConfirm(p.organizationId)} title="Deactivate" aria-label="Deactivate">
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

      {/* Form Modal */}
      <PartnerFormModal
        show={showForm}
        editing={!!editing}
        initial={editing || EMPTY_FORM}
        onSave={handleSave}
        onClose={() => setShowForm(false)}
      />

      {/* Delete Confirm */}
      <Modal show={!!deleteConfirm} onHide={() => setDeleteConfirm(null)} centered className="admin-modal-dark">
        <Modal.Header closeButton>
          <Modal.Title>
            <i className="bi bi-exclamation-triangle-fill text-danger me-2"></i>
            Deactivate Partner
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to deactivate this partner? They will no longer appear
          on the public Our Partners page. Linked donations will be preserved.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setDeleteConfirm(null)}>Cancel</Button>
          <Button variant="danger" onClick={handleDelete}>Deactivate</Button>
        </Modal.Footer>
      </Modal>
    </AdminPageFrame>
  );
}

export default AdminPartnersPage;
