import React, { useState, useEffect, useCallback } from 'react';
import { Spinner } from 'react-bootstrap';
import api from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';
import AdminPageFrame from '../../components/AdminPageFrame';
import '../admin/AdminForm.css';

/* â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
 * AdminUsersPage
 *   Manage all platform users.
 *   - Admins can deactivate/reactivate and change Userâ†’Admin (role guard).
 *   - SuperAdmin can change any role including SuperAdmin demotion/promotion
 *     (with last-SuperAdmin guard left to backend).
 * â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ */

const ROLES = ['User', 'Admin', 'SuperAdmin'];

const fmtDate = (iso) => {
  if (!iso) return 'â€”';
  const d = new Date(iso);
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
};
const fmtDateTime = (iso) => {
  if (!iso) return 'â€”';
  const d = new Date(iso);
  return d.toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
};
const fmtVnd = (n) => new Intl.NumberFormat('vi-VN', {
  style: 'currency', currency: 'VND', maximumFractionDigits: 0,
}).format(n || 0);

function AdminUsersPage() {
  const { user: me, isSuperAdmin } = useAuth();
  const [items, setItems] = useState([]);
  const [pagination, setPagination] = useState({ page: 1, pageSize: 20, total: 0, totalPages: 1 });
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [page, setPage] = useState(1);

  const fetch = useCallback(async () => {
    try {
      setLoading(true);
      setErrorMsg(null);
      const params = { page, pageSize: 20 };
      if (roleFilter) params.role = roleFilter;
      if (search.trim()) params.search = search.trim();
      const res = await api.get('/admin/users', { params });
      if (res.data.success) {
        const d = res.data.data;
        setItems(d.items || []);
        setPagination({
          page: d.page,
          pageSize: d.pageSize,
          total: d.total,
          totalPages: d.totalPages || 1,
        });
      }
    } catch (err) {
      console.error(err);
      setErrorMsg(err.response?.data?.message || 'Failed to load users.');
    } finally {
      setLoading(false);
    }
  }, [page, roleFilter, search]);

  useEffect(() => { fetch(); }, [fetch]);
  useEffect(() => { setPage(1); }, [roleFilter, search]);

  /* Auto-clear success */
  useEffect(() => {
    if (!successMsg) return;
    const t = setTimeout(() => setSuccessMsg(null), 3000);
    return () => clearTimeout(t);
  }, [successMsg]);

  const flashSuccess = (m) => setSuccessMsg(m);

  const handleChangeRole = async (target, newRole) => {
    if (target.role === newRole) return;
    try {
      const res = await api.put(`/admin/users/${target.userId}/role`, { role: newRole });
      if (res.data.success) {
        setItems((arr) => arr.map((u) => (u.userId === target.userId ? { ...u, role: newRole } : u)));
        flashSuccess(`Role updated to ${newRole}.`);
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Failed to update role.');
    }
  };

  const handleToggleActive = async (target) => {
    try {
      const res = await api.put(`/admin/users/${target.userId}/status`, { isActive: !target.isActive });
      if (res.data.success) {
        setItems((arr) => arr.map((u) => (u.userId === target.userId ? { ...u, isActive: !target.isActive } : u)));
        flashSuccess(target.isActive ? 'User deactivated.' : 'User reactivated.');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Failed to update status.');
    }
  };

  const handleDelete = async (target) => {
    if (!window.confirm(`Deactivate ${target.fullName}? They will no longer be able to sign in.`)) return;
    try {
      const res = await api.delete(`/admin/users/${target.userId}`);
      if (res.data.success) {
        setItems((arr) => arr.map((u) => (u.userId === target.userId ? { ...u, isActive: false } : u)));
        flashSuccess('User deactivated.');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Failed to deactivate user.');
    }
  };

  return (
    <AdminPageFrame
      eyebrow="People Â· Users"
      title="Users"
      sub="Everyone who has an account on GiveAID. Manage roles, status, and access."
      error={errorMsg}
      success={successMsg}
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
            placeholder="Search name, email, or usernameâ€¦"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <button type="button" className="af-search-clear" onClick={() => setSearch('')} aria-label="Clear search">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          )}
        </div>

        <select className="af-filter" value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
          <option value="">All roles</option>
          {ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
        </select>

        <div className="af-toolbar-spacer" />
        <button type="button" className="af-btn af-btn-secondary" onClick={fetch}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" style={{ width: 14, height: 14 }}><polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg>
          <span>Refresh</span>
        </button>
      </div>

      {/* Table or empty */}
      {!loading && items.length === 0 ? (
        <div className="af-empty">
          <div className="af-empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/>
            </svg>
          </div>
          <h3 className="af-empty-title">No users found</h3>
          <p className="af-empty-text">
            {search || roleFilter ? 'Try adjusting your filters.' : 'No accounts have been created yet.'}
          </p>
        </div>
      ) : (
        <div className="af-panel">
          <div className="af-table-wrap">
            <table className="af-table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Donations</th>
                  <th>Last login</th>
                  <th>Joined</th>
                  <th className="text-end">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((u) => {
                  const canEditRole =
                    isSuperAdmin ||
                    (u.role === 'User'); // Admin can promote Userâ†’Admin (their own choice)
                  const canDeactivate = isSuperAdmin || u.userId !== me?.userId;

                  return (
                    <tr key={u.userId}>
                      <td>
                        <div className="ad-user-row-mini">
                          <div className="ad-user-avatar" aria-hidden="true">
                            {(u.fullName || u.email || 'U').charAt(0).toUpperCase()}
                          </div>
                          <div className="ad-user-info">
                            <div className="af-cell-strong">
                              {u.fullName}
                              {u.userId === me?.userId && <span className="ad-you-tag">You</span>}
                              {!u.isActive && <span className="ad-anon-tag">inactive</span>}
                            </div>
                            <div className="af-cell-meta">{u.email}</div>
                            {u.phone && <div className="af-cell-meta">{u.phone}</div>}
                          </div>
                        </div>
                      </td>
                      <td>
                        {canEditRole ? (
                          <select
                            className="af-filter"
                            style={{ minWidth: 110, padding: '5px 8px', fontSize: '0.8125rem' }}
                            value={u.role}
                            onChange={(e) => handleChangeRole(u, e.target.value)}
                            disabled={u.userId === me?.userId}
                            title={u.userId === me?.userId ? 'You cannot change your own role' : undefined}
                          >
                            {ROLES.map((r) => (
                              <option
                                key={r}
                                value={r}
                                disabled={r === 'SuperAdmin' && !isSuperAdmin}
                              >
                                {r}
                              </option>
                            ))}
                          </select>
                        ) : (
                          <span className={`af-pill af-pill-${rolePillKey(u.role)}`}>
                            {u.role}
                          </span>
                        )}
                      </td>
                      <td>
                        <span className={`af-pill ${u.isActive ? 'af-pill-active' : 'af-pill-completed'}`}>
                          {u.isActive ? 'Active' : 'Inactive'}
                        </span>
                        {u.isVerified && <span className="ad-verified-tag" title="Email verified">âœ“ Verified</span>}
                      </td>
                      <td>
                        <div className="af-cell-strong">{u.donationCount}</div>
                        <div className="af-cell-meta">{fmtVnd(u.totalDonated)}</div>
                      </td>
                      <td><span className="af-cell-meta">{fmtDateTime(u.lastLogin)}</span></td>
                      <td><span className="af-cell-meta">{fmtDate(u.createdAt)}</span></td>
                      <td className="text-end">
                        <div className="af-row-actions">
                          <button
                            type="button"
                            className="af-icon-btn"
                            onClick={() => handleToggleActive(u)}
                            disabled={!canDeactivate}
                            title={u.isActive ? 'Deactivate' : 'Reactivate'}
                            aria-label={u.isActive ? 'Deactivate' : 'Reactivate'}
                          >
                            {u.isActive ? (
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M18.36 6.64a9 9 0 1 1-12.73 0"/><line x1="12" y1="2" x2="12" y2="12"/></svg>
                            ) : (
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><polyline points="20 6 9 17 4 12"/></svg>
                            )}
                          </button>
                          {isSuperAdmin && u.userId !== me?.userId && (
                            <button
                              type="button"
                              className="af-icon-btn danger"
                              onClick={() => handleDelete(u)}
                              title="Deactivate permanently"
                              aria-label="Deactivate"
                            >
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/></svg>
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <div className="af-pagination">
              <span>
                Page <strong>{pagination.page}</strong> of {pagination.totalPages}
                <span style={{ marginLeft: 8, color: 'var(--c4k-gray-400, #9CA3AF)' }}>
                  ({pagination.total} total)
                </span>
              </span>
              <div className="af-pagination-pages">
                <button
                  type="button"
                  className="af-page-btn"
                  disabled={pagination.page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                >
                  â€¹ Prev
                </button>
                <button
                  type="button"
                  className="af-page-btn"
                  disabled={pagination.page >= pagination.totalPages}
                  onClick={() => setPage((p) => Math.min(pagination.totalPages, p + 1))}
                >
                  Next â€º
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {loading && items.length === 0 && (
        <div className="af-loading"><Spinner animation="border" /></div>
      )}
    </AdminPageFrame>
  );
}

/* map role â†’ af-pill-* key */
function rolePillKey(role) {
  switch (role) {
    case 'SuperAdmin': return 'cancelled'; /* coral-toned */
    case 'Admin':      return 'ongoing';
    default:           return 'completed';
  }
}

export default AdminUsersPage;

