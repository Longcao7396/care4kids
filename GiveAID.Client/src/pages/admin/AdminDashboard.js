import React, { useState, useEffect, useCallback } from 'react';
import { Spinner, Alert } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../services/api';
import './AdminDashboard.css';

/* ─────────────────────────────────────────────────
 * AdminDashboard
 *   Care4Kids admin home — KPIs + recent activity.
 *   Rendered inside AdminLayout (no own <Container/>).
 * ───────────────────────────────────────────────── */

function AdminDashboard() {
  const { user } = useAuth();
  const [stats, setStats] = useState(null);
  const [recentDonations, setRecentDonations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchAll = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const [statsRes, recentRes] = await Promise.all([
        api.get('/admin/stats'),
        api.get('/admin/recent-donations?count=5'),
      ]);
      if (statsRes.data.success) setStats(statsRes.data.data);
      if (recentRes.data.success) setRecentDonations(recentRes.data.data || []);
    } catch (err) {
      console.error(err);
      setError(
        err.response?.data?.message ||
        'Failed to load dashboard statistics. Please try again.'
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchAll(); }, [fetchAll]);

  /* ── Loading state ── */
  if (loading) {
    return (
      <div className="ad-loading">
        <Spinner animation="border" style={{ color: 'var(--c4k-teal)' }} />
      </div>
    );
  }

  /* ── Error state ── */
  if (error || !stats) {
    return (
      <div className="ad-state-wrap">
        <Alert variant="danger" className="ad-state-alert">
          <Alert.Heading>Unable to load dashboard</Alert.Heading>
          <p>{error || 'No data received from server.'}</p>
          <button className="ad-state-retry" onClick={fetchAll}>Try again</button>
        </Alert>
      </div>
    );
  }

  const overview = stats.overview || {};
  const maxMonthly = Math.max(
    1,
    ...(stats.donationsByMonth || []).map((m) => m.total)
  );

  return (
    <div className="ad-page">

      {/* ═══ GREETING ═══ */}
      <header className="ad-greeting">
        <div className="ad-greeting-text">
          <p className="ad-eyebrow">Administration · {new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' })}</p>
          <h1 className="ad-greeting-title">
            Good day, {user?.fullName?.split(' ').slice(-1)[0] || user?.fullName || 'Admin'}.
          </h1>
          <p className="ad-greeting-sub">
            Here's how Care4Kids is doing today — every donation brings a child closer to a meal, a book, a chance.
          </p>
        </div>
        <div className="ad-greeting-actions">
          <Link to="/admin/campaigns" className="ad-btn ad-btn-primary">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            <span>New campaign</span>
          </Link>
        </div>
      </header>

      {/* ═══ KPI CARDS ═══ */}
      <section className="ad-kpi-grid">
        <KpiCard
          tone="coral"
          icon={<IconDonate />}
          label="Total donations raised"
          value={formatCurrency(overview.totalDonations)}
          meta={overview.recentDonationsAmount
            ? `${formatCurrency(overview.recentDonationsAmount)} in last 30 days`
            : 'No recent activity'}
        />
        <KpiCard
          tone="teal"
          icon={<IconUsers />}
          label="Donors"
          value={overview.totalDonors}
          meta="Unique people who gave"
        />
        <KpiCard
          tone="coral-dark"
          icon={<IconCampaign />}
          label="Active campaigns"
          value={overview.activeCampaigns}
          meta={`${overview.completedCampaigns} completed`}
        />
        <KpiCard
          tone="teal-dark"
          icon={<IconEvent />}
          label="Ongoing programmes"
          value={overview.activeProgrammes}
          meta={`${overview.programmeRegistrations} registrations`}
        />
      </section>

      {/* ═══ TREND CHART + CAUSE BREAKDOWN ═══ */}
      <section className="ad-row ad-row-2col">

        {/* Donations trend (last 6 months) */}
        <div className="ad-panel">
          <div className="ad-panel-header">
            <div>
              <p className="ad-eyebrow">Trend</p>
              <h2 className="ad-panel-title">Donations · last 6 months</h2>
            </div>
          </div>
          <div className="ad-panel-body">
            {stats.donationsByMonth && stats.donationsByMonth.length > 0 ? (
              <div className="ad-chart">
                {stats.donationsByMonth.map((m) => {
                  const pct = Math.round((m.total / maxMonthly) * 100);
                  return (
                    <div key={`${m.year}-${m.month}`} className="ad-chart-col">
                      <div className="ad-chart-bar-wrap" title={`${formatCurrency(m.total)} · ${m.count} donations`}>
                        <div className="ad-chart-bar" style={{ height: `${pct}%` }}>
                          <span className="ad-chart-bar-val">{formatCurrencyShort(m.total)}</span>
                        </div>
                      </div>
                      <div className="ad-chart-label">
                        {monthShort(m.month)} {String(m.year).slice(-2)}
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <Empty>No donation data yet</Empty>
            )}
          </div>
        </div>

        {/* Causes */}
        <div className="ad-panel">
          <div className="ad-panel-header">
            <div>
              <p className="ad-eyebrow">Where it goes</p>
              <h2 className="ad-panel-title">Donations by cause</h2>
            </div>
          </div>
          <div className="ad-panel-body">
            {stats.donationsByCause && stats.donationsByCause.length > 0 ? (
              <ul className="ad-cause-list">
                {stats.donationsByCause.map((cause) => {
                  const percent = cause.targetAmount > 0
                    ? Math.min((cause.raisedAmount / cause.targetAmount) * 100, 100)
                    : null;
                  return (
                    <li key={cause.causeId} className="ad-cause-item">
                      <div className="ad-cause-top">
                        <div className="ad-cause-name">{cause.causeName}</div>
                        <span className="ad-cause-code">{cause.causeCode}</span>
                      </div>
                      <div className="ad-cause-numbers">
                        <span className="ad-cause-raised">{formatCurrency(cause.raisedAmount)}</span>
                        {cause.targetAmount > 0 && (
                          <span className="ad-cause-target">/ {formatCurrency(cause.targetAmount)}</span>
                        )}
                      </div>
                      {percent !== null && (
                        <div className="ad-cause-progress">
                          <div className="ad-cause-progress-track">
                            <div className="ad-cause-progress-fill" style={{ width: `${percent}%` }} />
                          </div>
                          <span className="ad-cause-progress-num">{percent.toFixed(0)}%</span>
                        </div>
                      )}
                      <div className="ad-cause-meta">{cause.donationCount} donations</div>
                    </li>
                  );
                })}
              </ul>
            ) : (
              <Empty>No causes configured</Empty>
            )}
          </div>
        </div>
      </section>

      {/* ═══ TOP CAMPAIGNS + RECENT ACTIVITY ═══ */}
      <section className="ad-row ad-row-2col">

        {/* Top campaigns */}
        <div className="ad-panel">
          <div className="ad-panel-header">
            <div>
              <p className="ad-eyebrow">Performance</p>
              <h2 className="ad-panel-title">Top active campaigns</h2>
            </div>
            <Link to="/admin/campaigns" className="ad-link-arrow">View all
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><polyline points="9 18 15 12 9 6"/></svg>
            </Link>
          </div>
          <div className="ad-panel-body ad-panel-body-tight">
            {stats.donationsByCampaign && stats.donationsByCampaign.length > 0 ? (
              <ul className="ad-campaign-list">
                {stats.donationsByCampaign.map((c) => (
                  <li key={c.campaignId}>
                    <Link to={`/campaigns/${c.campaignId}`} className="ad-campaign-row">
                      <div className="ad-campaign-info">
                        <div className="ad-campaign-name">{c.campaignName}</div>
                        <div className="ad-campaign-meta">
                          <span className="ad-tag">{c.causeName}</span>
                          <span className="ad-campaign-donors">
                            <IconHeart /> {c.donorCount} donors
                          </span>
                        </div>
                      </div>
                      <div className="ad-campaign-progress">
                        <div className="ad-campaign-amount">
                          <span className="ad-campaign-raised">{formatCurrency(c.raisedAmount)}</span>
                          <span className="ad-campaign-goal">of {formatCurrency(c.goalAmount)}</span>
                        </div>
                        <div className="ad-campaign-bar-track">
                          <div className="ad-campaign-bar-fill" style={{ width: `${Math.min(c.percentageReached || 0, 100)}%` }} />
                        </div>
                        <div className="ad-campaign-pct">{(c.percentageReached || 0).toFixed(0)}%</div>
                      </div>
                    </Link>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty>No active campaigns</Empty>
            )}
          </div>
        </div>

        {/* Recent donations */}
        <div className="ad-panel">
          <div className="ad-panel-header">
            <div>
              <p className="ad-eyebrow">Activity</p>
              <h2 className="ad-panel-title">Recent donations</h2>
            </div>
          </div>
          <div className="ad-panel-body ad-panel-body-tight">
            {recentDonations.length > 0 ? (
              <ul className="ad-donation-list">
                {recentDonations.map((d) => (
                  <li key={d.donationId} className="ad-donation-item">
                    <div className={`ad-don-avatar ${d.isAnonymous ? 'is-anon' : ''}`} aria-hidden="true">
                      {d.isAnonymous ? <IconAnon /> : (d.userName || '?').charAt(0).toUpperCase()}
                    </div>
                    <div className="ad-don-info">
                      <div className="ad-don-name">
                        {d.userName}
                        {d.isAnonymous && <span className="ad-don-anon-tag">hidden</span>}
                      </div>
                      <div className="ad-don-meta">
                        <span>{d.causeName}</span>
                        {d.campaignName && <span>· {d.campaignName}</span>}
                      </div>
                    </div>
                    <div className="ad-don-amount">
                      <div className="ad-don-value">{formatCurrency(d.amount)}</div>
                      <div className="ad-don-time">{relativeTime(d.donationDate)}</div>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty>No donations yet</Empty>
            )}
          </div>
        </div>
      </section>

      {/* ═══ NEW USERS ═══ */}
      <section className="ad-row ad-row-1col">
        <div className="ad-panel">
          <div className="ad-panel-header">
            <div>
              <p className="ad-eyebrow">Community</p>
              <h2 className="ad-panel-title">Recently registered users</h2>
            </div>
            <span className="ad-panel-meta">{stats.recentUsers?.length || 0} latest</span>
          </div>
          <div className="ad-panel-body">
            {stats.recentUsers && stats.recentUsers.length > 0 ? (
              <ul className="ad-user-list">
                {stats.recentUsers.map((u) => (
                  <li key={u.userId} className="ad-user-row">
                    <div className="ad-user-avatar" aria-hidden="true">
                      {(u.fullName || u.email || 'U').charAt(0).toUpperCase()}
                    </div>
                    <div className="ad-user-info">
                      <div className="ad-user-name">{u.fullName}</div>
                      <div className="ad-user-email">{u.email}</div>
                    </div>
                    <div className="ad-user-date">{relativeTime(u.createdAt)}</div>
                  </li>
                ))}
              </ul>
            ) : (
              <Empty>No new users found</Empty>
            )}
          </div>
        </div>
      </section>

    </div>
  );
}

/* ══════════════════════════════════════════
   Sub-components
   ══════════════════════════════════════════ */

function KpiCard({ tone, icon, label, value, meta }) {
  return (
    <div className={`ad-kpi ad-kpi-${tone}`}>
      <div className="ad-kpi-icon">{icon}</div>
      <div className="ad-kpi-label">{label}</div>
      <div className="ad-kpi-value">{value}</div>
      <div className="ad-kpi-meta">{meta}</div>
    </div>
  );
}

function Empty({ children }) {
  return <div className="ad-empty">{children}</div>;
}

/* ══════════════════════════════════════════
   Icons
   ══════════════════════════════════════════ */
const baseIcon = {
  width: 22, height: 22, viewBox: '0 0 24 24',
  fill: 'none', stroke: 'currentColor',
  strokeWidth: 1.8, strokeLinecap: 'round', strokeLinejoin: 'round',
  'aria-hidden': true,
};
const IconDonate   = () => <svg {...baseIcon}><path d="M12 2v20M17 5H9.5a3.5 3.5 0 1 0 0 7h5a3.5 3.5 0 1 1 0 7H6"/></svg>;
const IconUsers    = () => <svg {...baseIcon}><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>;
const IconCampaign = () => <svg {...baseIcon}><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>;
const IconEvent    = () => <svg {...baseIcon}><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>;
const IconHeart    = () => <svg {...baseIcon}><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>;
const IconAnon     = () => <svg {...baseIcon}><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg>;

/* ══════════════════════════════════════════
   Helpers
   ══════════════════════════════════════════ */

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount || 0);
}

function formatCurrencyShort(amount) {
  const a = amount || 0;
  if (a >= 1_000_000_000) return `${(a / 1_000_000_000).toFixed(1)}B`;
  if (a >= 1_000_000) return `${(a / 1_000_000).toFixed(1)}M`;
  if (a >= 1_000) return `${(a / 1_000).toFixed(0)}K`;
  return `${a}`;
}

function monthShort(m) {
  return ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'][m - 1] || '';
}

function relativeTime(date) {
  const d = new Date(date);
  const diff = Date.now() - d.getTime();
  const sec = Math.floor(diff / 1000);
  if (sec < 60) return 'just now';
  const min = Math.floor(sec / 60);
  if (min < 60) return `${min}m ago`;
  const hr = Math.floor(min / 60);
  if (hr < 24) return `${hr}h ago`;
  const day = Math.floor(hr / 24);
  if (day < 30) return `${day}d ago`;
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

export default AdminDashboard;
