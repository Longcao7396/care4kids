import React, { useState, useEffect, useCallback } from 'react';
import { Spinner, Modal, Button } from 'react-bootstrap';
import api from '../../services/api';
import AdminPageFrame from '../../components/AdminPageFrame';
import '../admin/AdminForm.css';

/* ─────────────────────────────────────────────────────────────────────
 * AdminEmailLogsPage
 *   View all outbound email attempts, retry failed ones, and inspect errors.
 *   All admin-only (SuperAdmin / Admin role).
 * ───────────────────────────────────────────────────────────────────── */

const STATUS_OPTIONS  = ['Sent', 'Failed', 'MockSent', 'Pending'];
const CATEGORY_OPTIONS = [
  'invitation',
  'donation_receipt',
  'registration_confirmation',
  'contact_reply',
  'general',
];

const fmtDate = (iso) => {
  if (!iso) return '—';
  const d = new Date(iso);
  return d.toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

function statusPill(key) {
  switch (key) {
    case 'Sent':      return 'active';
    case 'Failed':    return 'failed';
    case 'MockSent':  return 'pending';
    case 'Pending':   return 'ongoing';
    default:          return 'reviewed';
  }
}

export default function AdminEmailLogsPage() {
  const [items, setItems]           = useState([]);
  const [stats, setStats]           = useState({});
  const [pagination, setPagination] = useState({ page: 1, pageSize: 30, total: 0, totalPages: 1 });
  const [loading, setLoading]       = useState(true);
  const [errorMsg, setErrorMsg]     = useState(null);
  const [actionLoading, setActionLoading] = useState(false);

  // Filters
  const [statusFilter,  setStatusFilter]  = useState('');
  const [categoryFilter,setCategoryFilter]= useState('');
  const [search,        setSearch]        = useState('');
  const [dateFrom,      setDateFrom]      = useState('');
  const [dateTo,        setDateTo]        = useState('');
  const [page,         setPage]          = useState(1);

  // Preview modal
  const [previewItem, setPreviewItem] = useState(null);

  /* Fetch list + stats */
  const fetch = useCallback(async () => {
    try {
      setLoading(true);
      setErrorMsg(null);
      const params = { page, pageSize: 30 };
      if (statusFilter)   params.status   = statusFilter;
      if (categoryFilter) params.category = categoryFilter;
      if (search.trim())  params.search   = search.trim();
      if (dateFrom)       params.dateFrom = dateFrom;
      if (dateTo)         params.dateTo   = dateTo;

      const [listRes, statsRes] = await Promise.all([
        api.get('/admin/emails', { params }),
        api.get('/admin/emails/stats').catch(() => null),
      ]);

      // interceptor unwraps envelope → listRes is the raw data directly
      const d = listRes && typeof listRes === 'object' ? listRes : {};
      setItems(d.items || []);
      const p = d.pagination || {};
      setPagination({
        page:       p.page       ?? page,
        pageSize:   p.pageSize  ?? 30,
        total:      p.total      ?? 0,
        totalPages: p.totalPages ?? 1,
      });
      // interceptor unwraps envelope → statsRes is the raw data directly
      if (statsRes && typeof statsRes === 'object') setStats(statsRes);
    } catch (err) {
      console.error(err);
      setErrorMsg(err.response?.data?.message || 'Failed to load email logs.');
    } finally {
      setLoading(false);
    }
  }, [page, statusFilter, categoryFilter, search, dateFrom, dateTo]);

  useEffect(() => { fetch(); }, [fetch]);
  useEffect(() => { setPage(1); }, [statusFilter, categoryFilter, search, dateFrom, dateTo]);

  /* Retry single entry */
  const handleResend = async (item) => {
    if (!window.confirm(`Re-send email to ${item.toEmail}?\n\nSubject: ${item.subject}`)) return;
    try {
      setActionLoading(true);
      const res = await api.post(`/admin/emails/${item.emailLogId}/resend`);
      // interceptor unwraps envelope → res is the raw data directly
      if (res && typeof res === 'object' && res.emailLogId) {
        await fetch();
      } else {
        setErrorMsg(typeof res === 'string' ? res : 'Resend failed.');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Resend failed.');
    } finally {
      setActionLoading(false);
    }
  };

  /* Retry all failed */
  const handleRetryAll = async () => {
    if (!window.confirm('Retry all failed emails (up to 3 attempts per row)?')) return;
    try {
      setActionLoading(true);
      const res = await api.post('/admin/emails/retry-all', { MaxAttempts: 3 });
      // interceptor unwraps envelope → res is the raw data directly
      if (res && typeof res === 'object') {
        await fetch();
      } else {
        setErrorMsg(typeof res === 'string' ? res : 'Retry failed.');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Retry failed.');
    } finally {
      setActionLoading(false);
    }
  };

  /* Compact pagination window */
  const pageNumbers = (() => {
    const tp = pagination.totalPages;
    const cur = pagination.page;
    const arr = [];
    for (let i = 1; i <= tp; i++) {
      if (i === 1 || i === tp || Math.abs(i - cur) <= 2) arr.push(i);
      else if (arr[arr.length - 1] !== '…') arr.push('…');
    }
    return arr;
  })();

  const canRetry = items.some(i => i.status === 'Failed' || i.status === 'Pending');

  return (
    <AdminPageFrame
      eyebrow="Communication · Email"
      title="Email Logs"
      sub="Every outbound email sent (or attempted) by Care4Kids. Inspect bodies, retry failures, and verify delivery."
      error={errorMsg}
    >
      {/* KPI summary */}
      <div className="ad-summary-row">
        <div className="ad-summary-card">
          <span className="ad-summary-lbl">Total</span>
          <span className="ad-summary-val">{stats.total ?? 0}</span>
        </div>
        <div className="ad-summary-card">
          <span className="ad-summary-lbl">Sent</span>
          <span className="ad-summary-val" style={{ color: 'var(--c4k-success, #166534)' }}>
            {stats.sent ?? 0}
          </span>
        </div>
        <div className="ad-summary-card">
          <span className="ad-summary-lbl">MockSent</span>
          <span className="ad-summary-val" style={{ color: 'var(--c4k-warning, #92400E)' }}>
            {stats.mockSent ?? 0}
          </span>
        </div>
        <div className="ad-summary-card">
          <span className="ad-summary-lbl">Failed</span>
          <span className="ad-summary-val" style={{ color: 'var(--c4k-danger, #B91C1C)' }}>
            {stats.failed ?? 0}
          </span>
        </div>
        <div className="ad-summary-card">
          <span className="ad-summary-lbl">Sent Today</span>
          <span className="ad-summary-val">{stats.sentToday ?? 0}</span>
        </div>
      </div>

      {/* Toolbar */}
      <div className="af-toolbar">
        {/* Status filter */}
        <div className="af-search" style={{ flex: 'none', minWidth: 160 }}>
          <span className="af-search-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M22 2L11 13"/><path d="M22 2L15 22 11 13 2 9 22 2z"/>
            </svg>
          </span>
          <select
            className="af-search-input"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            style={{ paddingLeft: 36, appearance: 'auto' }}
          >
            <option value="">All statuses</option>
            {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
        </div>

        {/* Category filter */}
        <select
          className="af-filter"
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
        >
          <option value="">All categories</option>
          {CATEGORY_OPTIONS.map((c) => <option key={c} value={c}>{c.replace(/_/g, ' ')}</option>)}
        </select>

        {/* Date range */}
        <input type="date" className="af-filter" value={dateFrom}
          onChange={(e) => setDateFrom(e.target.value)} placeholder="From" />
        <input type="date" className="af-filter" value={dateTo}
          onChange={(e) => setDateTo(e.target.value)} placeholder="To" />

        <div className="af-toolbar-spacer" />

        {/* Search */}
        <div className="af-search" style={{ minWidth: 220 }}>
          <span className="af-search-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
            </svg>
          </span>
          <input
            type="text"
            className="af-search-input"
            placeholder="Search email, subject…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <button type="button" className="af-search-clear" onClick={() => setSearch('')} aria-label="Clear search">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          )}
        </div>

        <button type="button" className="af-btn af-btn-secondary" onClick={fetch} disabled={actionLoading}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" style={{ width: 14, height: 14 }}>
            <polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/>
          </svg>
          <span>Refresh</span>
        </button>

        {canRetry && (
          <button type="button" className="af-btn af-btn-secondary" onClick={handleRetryAll} disabled={actionLoading}
            style={{ color: 'var(--c4k-warning, #92400E)', borderColor: 'var(--c4k-warning, #92400E)' }}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" style={{ width: 14, height: 14 }}>
              <polyline points="1 4 1 10 7 10"/><path d="M3.51 15a9 9 0 1 0 .49-3.91"/>
            </svg>
            <span>Retry All Failed</span>
          </button>
        )}
      </div>

      {/* Table */}
      {!loading && items.length === 0 ? (
        <div className="af-empty">
          <div className="af-empty-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
              <polyline points="22,6 12,13 2,6"/>
            </svg>
          </div>
          <h3 className="af-empty-title">No email logs found</h3>
          <p className="af-empty-text">
            {statusFilter || search ? 'Try adjusting your filters.' : 'No emails have been logged yet.'}
          </p>
        </div>
      ) : (
        <div className="af-panel">
          <div className="af-table-wrap">
            <table className="af-table">
              <thead>
                <tr>
                  <th>To</th>
                  <th>Subject</th>
                  <th>Category</th>
                  <th>Status</th>
                  <th>Sent / Created</th>
                  <th>Retries</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.emailLogId}>
                    <td>
                      <div className="af-cell-strong">{item.toEmail}</div>
                      {item.relatedId && (
                        <div className="af-cell-meta">ref: #{item.relatedId}</div>
                      )}
                    </td>
                    <td>
                      <div style={{ maxWidth: 280, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}
                        title={item.subject}>
                        {item.subject}
                      </div>
                    </td>
                    <td>
                      <span className="af-cell-meta"
                        style={{ textTransform: 'capitalize', whiteSpace: 'nowrap' }}>
                        {(item.category || 'general').replace(/_/g, ' ')}
                      </span>
                    </td>
                    <td>
                      <span className={`af-pill af-pill-${statusPill(item.status)}`}>
                        {item.status}
                      </span>
                      {item.errorMessage && (
                        <div className="af-cell-meta"
                          style={{ marginTop: 4, color: 'var(--c4k-danger, #B91C1C)',
                                   maxWidth: 240, whiteSpace: 'nowrap',
                                   overflow: 'hidden', textOverflow: 'ellipsis' }}
                          title={item.errorMessage}>
                          {item.errorMessage}
                        </div>
                      )}
                    </td>
                    <td>
                      <div className="af-cell-meta">{fmtDate(item.sentAt || item.createdAt)}</div>
                      {item.sentAt && (
                        <div className="af-cell-meta" style={{ fontSize: 11, color: '#AAA' }}>
                          logged {fmtDate(item.createdAt)}
                        </div>
                      )}
                    </td>
                    <td>
                      <span className="af-cell-meta">
                        {item.retryCount > 0 ? `${item.retryCount}×` : '—'}
                      </span>
                    </td>
                    <td>
                      <div style={{ display: 'flex', gap: 6 }}>
                        <button
                          type="button"
                          className="af-btn af-btn-secondary"
                          style={{ padding: '4px 10px', fontSize: 12 }}
                          title="Preview email body"
                          onClick={() => setPreviewItem(item)}
                        >
                          View
                        </button>
                        {(item.status === 'Failed' || item.status === 'Pending') && (
                          <button
                            type="button"
                            className="af-btn af-btn-secondary"
                            style={{ padding: '4px 10px', fontSize: 12,
                                     color: 'var(--c4k-warning, #92400E)',
                                     borderColor: 'var(--c4k-warning, #92400E)' }}
                            title="Re-send this email"
                            disabled={actionLoading}
                            onClick={() => handleResend(item)}
                          >
                            Resend
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <div className="af-pagination">
              <span>
                Page <strong>{pagination.page}</strong> of {pagination.totalPages}
                <span style={{ marginLeft: 8, color: '#9CA3AF' }}>({pagination.total} total)</span>
              </span>
              <div className="af-pagination-pages">
                <button type="button" className="af-page-btn"
                  disabled={pagination.page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}>
                  ‹ Prev
                </button>
                {pageNumbers.map((n, idx) =>
                  n === '…' ? (
                    <span key={`gap-${idx}`} className="af-page-btn"
                      style={{ border: 0, background: 'transparent' }}>…</span>
                  ) : (
                    <button key={n} type="button"
                      className={`af-page-btn ${pagination.page === n ? 'is-current' : ''}`}
                      onClick={() => setPage(n)}>
                      {n}
                    </button>
                  )
                )}
                <button type="button" className="af-page-btn"
                  disabled={pagination.page >= pagination.totalPages}
                  onClick={() => setPage((p) => Math.min(pagination.totalPages, p + 1))}>
                  Next ›
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {loading && items.length === 0 && (
        <div className="af-loading"><Spinner animation="border" /></div>
      )}

      {/* Loading overlay for actions */}
      {actionLoading && (
        <div style={{
          position: 'fixed', bottom: 24, right: 24,
          background: '#fff', borderRadius: 8, padding: '10px 16px',
          boxShadow: '0 4px 16px rgba(0,0,0,0.12)', display: 'flex', alignItems: 'center', gap: 8,
          fontSize: 14, color: '#555', zIndex: 999
        }}>
          <Spinner animation="border" style={{ width: 16, height: 16 }} />
          Retrying…
        </div>
      )}

      {/* Email preview modal */}
      {previewItem && (
        <EmailPreviewModal item={previewItem} onClose={() => setPreviewItem(null)} />
      )}
    </AdminPageFrame>
  );
}

/* ── Email preview modal ─────────────────────────────────────────────── */

function EmailPreviewModal({ item, onClose }) {
  return (
    <Modal show onHide={onClose} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title style={{ fontSize: 16 }}>
          Email Preview
          <span className={`af-pill af-pill-${statusPill(item.status)}`}
            style={{ marginLeft: 10, verticalAlign: 'middle' }}>
            {item.status}
          </span>
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <div style={{ marginBottom: 16 }}>
          <div style={{ marginBottom: 6 }}>
            <strong>To:</strong>{' '}
            <span>{item.toEmail}</span>
          </div>
          <div style={{ marginBottom: 6 }}>
            <strong>Category:</strong>{' '}
            <span style={{ textTransform: 'capitalize' }}>
              {(item.category || 'general').replace(/_/g, ' ')}
            </span>
          </div>
          <div style={{ marginBottom: 6 }}>
            <strong>Created:</strong>{' '}
            <span className="af-cell-meta">{fmtDate(item.createdAt)}</span>
          </div>
          {item.sentAt && (
            <div style={{ marginBottom: 6 }}>
              <strong>Sent:</strong>{' '}
              <span className="af-cell-meta">{fmtDate(item.sentAt)}</span>
            </div>
          )}
          {item.errorMessage && (
            <div style={{ marginBottom: 6, color: 'var(--c4k-danger, #B91C1C)' }}>
              <strong>Error:</strong>{' '}
              <span style={{ fontSize: 13 }}>{item.errorMessage}</span>
            </div>
          )}
        </div>

        <div style={{ marginBottom: 8, fontWeight: 600, fontSize: 15 }}>
          {item.subject}
        </div>

        <div style={{
          border: '1px solid #E5E7EB', borderRadius: 6, padding: 20,
          background: '#FAFAFA', maxHeight: 450, overflowY: 'auto'
        }}
          dangerouslySetInnerHTML={{ __html: item.body || '<em style="color:#999">No body content.</em>' }}
        />
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </Modal.Footer>
    </Modal>
  );
}
