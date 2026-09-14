import React from 'react';
import { Container, Alert } from 'react-bootstrap';
import { useAuth } from '../../contexts/AuthContext';
import AdminCmsPage from './AdminCmsPage';

/* ── Legacy wrapper for /admin/about ────────────
   The full Content Management Center is now at /admin/cms.
   This page renders the same CMS dashboard for backward compat. */
function AdminAboutPage() {
  const { user } = useAuth();
  const canAccessAdmin = user?.role === 'Admin' || user?.role === 'SuperAdmin';

  if (!canAccessAdmin) {
    return (
      <Container className="py-5">
        <Alert variant="danger">You do not have permission to access this page.</Alert>
      </Container>
    );
  }

  return <AdminCmsPage />;
}

export default AdminAboutPage;
