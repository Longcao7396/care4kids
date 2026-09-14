import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Badge, Alert, Button, Modal, Form, Spinner, ButtonGroup } from 'react-bootstrap';
import api from '../../services/api';

export default function TeamAdmin() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({});
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get('/team', { params: { activeOnly: false } });
      if (response.data.success) setItems(response.data.data || []);
    } catch (err) {
      setError('Failed to load team.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => {
    setEditing(null);
    setForm({
      fullName: '', roleTitle: '', department: '', bio: '',
      photoUrl: '', email: '', linkedInUrl: '', twitterUrl: '', facebookUrl: '',
      displayOrder: 0, isActive: true, isFeatured: false,
    });
    setShowModal(true);
  };

  const openEdit = (item) => {
    setEditing(item);
    setForm({
      fullName: item.fullName || '',
      roleTitle: item.roleTitle || '',
      department: item.department || '',
      bio: item.bio || '',
      photoUrl: item.photoUrl || '',
      email: item.email || '',
      linkedInUrl: item.linkedInUrl || '',
      twitterUrl: item.twitterUrl || '',
      facebookUrl: item.facebookUrl || '',
      displayOrder: item.displayOrder || 0,
      isActive: item.isActive !== false,
      isFeatured: !!item.isFeatured,
    });
    setShowModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setError(null);
      const payload = { ...form, displayOrder: Number(form.displayOrder) || 0 };
      const response = editing
        ? await api.put(`/team/${editing.teamMemberId}`, payload)
        : await api.post('/team', payload);
      if (response.data.success) {
        setSuccess(editing ? 'Team member updated.' : 'Team member added.');
        setShowModal(false);
        load();
      } else {
        setError(response.data.message || 'Save failed.');
      }
    } catch (err) {
      setError('Save failed.');
    }
  };

  const handleDelete = async (item) => {
    if (!window.confirm(`Delete ${item.fullName}? This cannot be undone.`)) return;
    try {
      await api.delete(`/team/${item.teamMemberId}`);
      setSuccess('Team member deleted.');
      load();
    } catch (err) {
      setError('Delete failed.');
    }
  };

  const isSuperAdmin = JSON.parse(localStorage.getItem('giveaid_user') || '{}')?.role === 'SuperAdmin';

  return (
    <div>
      {error && <Alert variant="danger" dismissible onClose={() => setError(null)}>{error}</Alert>}
      {success && <Alert variant="success" dismissible onClose={() => setSuccess(null)}>{success}</Alert>}

      <div className="d-flex justify-content-between align-items-center mb-3">
        <h4 className="text-light mb-0">Team Members</h4>
        <Button variant="primary" onClick={openCreate}>
          <i className="bi bi-plus-circle me-2"></i>Add Member
        </Button>
      </div>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" variant="primary" /></div>
      ) : items.length === 0 ? (
        <Alert variant="info">No team members yet.</Alert>
      ) : (
        <Card>
          <Table responsive hover className="mb-0">
            <thead className="bg-light">
              <tr>
                <th>Name</th>
                <th>Role</th>
                <th>Department</th>
                <th>Display Order</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {items.map((m) => (
                <tr key={m.teamMemberId}>
                  <td>
                    <div className="d-flex align-items-center gap-2">
                      <img
                        src={m.photoUrl || 'https://ui-avatars.com/api/?name=' + encodeURIComponent(m.fullName)}
                        alt=""
                        style={{ width: 32, height: 32, borderRadius: '50%', objectFit: 'cover' }}
                      />
                      <span className="fw-semibold">{m.fullName}</span>
                    </div>
                  </td>
                  <td>{m.roleTitle}</td>
                  <td>{m.department || '—'}</td>
                  <td>{m.displayOrder}</td>
                  <td>
                    <Badge bg={m.isActive ? 'success' : 'secondary'}>
                      {m.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                    {m.isFeatured && <Badge bg="warning" className="ms-1">Featured</Badge>}
                  </td>
                  <td className="text-end">
                    <ButtonGroup size="sm">
                      <Button variant="outline-primary" onClick={() => openEdit(m)}>Edit</Button>
                      {isSuperAdmin && (
                        <Button variant="outline-danger" onClick={() => handleDelete(m)}>Delete</Button>
                      )}
                    </ButtonGroup>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </Card>
      )}

      <Modal show={showModal} onHide={() => setShowModal(false)} size="lg" centered>
        <Modal.Header closeButton>
          <Modal.Title>{editing ? 'Edit Team Member' : 'Add Team Member'}</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            <div className="row g-3">
              <div className="col-md-6">
                <Form.Group>
                  <Form.Label>Full Name *</Form.Label>
                  <Form.Control value={form.fullName || ''} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required maxLength={150} />
                </Form.Group>
              </div>
              <div className="col-md-6">
                <Form.Group>
                  <Form.Label>Role Title *</Form.Label>
                  <Form.Control value={form.roleTitle || ''} onChange={(e) => setForm({ ...form, roleTitle: e.target.value })} required maxLength={150} />
                </Form.Group>
              </div>
              <div className="col-md-6">
                <Form.Group>
                  <Form.Label>Department</Form.Label>
                  <Form.Control value={form.department || ''} onChange={(e) => setForm({ ...form, department: e.target.value })} maxLength={100} />
                </Form.Group>
              </div>
              <div className="col-md-6">
                <Form.Group>
                  <Form.Label>Email</Form.Label>
                  <Form.Control type="email" value={form.email || ''} onChange={(e) => setForm({ ...form, email: e.target.value })} maxLength={100} />
                </Form.Group>
              </div>
              <div className="col-12">
                <Form.Group>
                  <Form.Label>Photo URL</Form.Label>
                  <Form.Control value={form.photoUrl || ''} onChange={(e) => setForm({ ...form, photoUrl: e.target.value })} maxLength={500} />
                </Form.Group>
              </div>
              <div className="col-12">
                <Form.Group>
                  <Form.Label>Bio</Form.Label>
                  <Form.Control as="textarea" rows={3} value={form.bio || ''} onChange={(e) => setForm({ ...form, bio: e.target.value })} />
                </Form.Group>
              </div>
              <div className="col-md-4">
                <Form.Group>
                  <Form.Label>LinkedIn URL</Form.Label>
                  <Form.Control value={form.linkedInUrl || ''} onChange={(e) => setForm({ ...form, linkedInUrl: e.target.value })} />
                </Form.Group>
              </div>
              <div className="col-md-4">
                <Form.Group>
                  <Form.Label>Twitter URL</Form.Label>
                  <Form.Control value={form.twitterUrl || ''} onChange={(e) => setForm({ ...form, twitterUrl: e.target.value })} />
                </Form.Group>
              </div>
              <div className="col-md-4">
                <Form.Group>
                  <Form.Label>Facebook URL</Form.Label>
                  <Form.Control value={form.facebookUrl || ''} onChange={(e) => setForm({ ...form, facebookUrl: e.target.value })} />
                </Form.Group>
              </div>
              <div className="col-md-4">
                <Form.Group>
                  <Form.Label>Display Order</Form.Label>
                  <Form.Control type="number" value={form.displayOrder || 0} onChange={(e) => setForm({ ...form, displayOrder: e.target.value })} />
                </Form.Group>
              </div>
              <div className="col-md-4">
                <Form.Group className="pt-4">
                  <Form.Check type="switch" label="Active" checked={!!form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                  <Form.Check type="switch" label="Featured" checked={!!form.isFeatured} onChange={(e) => setForm({ ...form, isFeatured: e.target.checked })} />
                </Form.Group>
              </div>
            </div>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShowModal(false)}>Cancel</Button>
            <Button variant="primary" type="submit">{editing ? 'Save Changes' : 'Create'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </div>
  );
}
