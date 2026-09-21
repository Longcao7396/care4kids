import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Container, Row, Col, ProgressBar, Alert, Tab, Tabs, Badge } from 'react-bootstrap';
import api from '../services/api';
import { campaignReportsService } from '../services';
import { useAuth } from '../contexts/AuthContext';
import './CampaignDetailPage.css';

function CampaignDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [campaign, setCampaign] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [registerLoading, setRegisterLoading] = useState(false);
  const [registerSuccess, setRegisterSuccess] = useState('');
  const [registerError, setRegisterError] = useState('');
  const [reports, setReports] = useState([]);
  const [reportsLoading, setReportsLoading] = useState(false);

  const fetchCampaignDetail = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get(`/campaigns/${id}`);
      // interceptor unwraps envelope → response is the object directly
      if (response && typeof response === 'object' && response.campaignId) {
        setCampaign(response);
      }
    } catch (err) {
      setError('Unable to load campaign information');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [id]);

  const fetchReports = useCallback(async () => {
    if (!id) return;
    try {
      setReportsLoading(true);
      const response = await campaignReportsService.getByCampaign(id, true);
      if (response.success) setReports(response.data || []);
    } catch (err) {
      console.error('Failed to load campaign reports:', err);
    } finally {
      setReportsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchCampaignDetail();
    fetchReports();
  }, [fetchCampaignDetail, fetchReports]);

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(amount);
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const handleDonate = () => {
    // Navigate to DonatePage with campaignId as URL param
    navigate(`/donate?campaignId=${campaign.campaignId}`);
  };

  const handleRegister = async () => {
    if (!user) {
      navigate('/login', { state: { from: `/campaigns/${id}` } });
      return;
    }
    setRegisterError('');
    setRegisterSuccess('');
    setRegisterLoading(true);
    try {
      const response = await api.post(`/campaigns/${id}/register`, {
        notes: ''
      });
      // interceptor unwraps envelope → response is truthy on success
      if (response) {
        setRegisterSuccess('Registration submitted! We will confirm your spot soon.');
        // Refresh campaign to update participant count
        fetchCampaignDetail();
      } else {
        setRegisterError('Registration failed. Please try again.');
      }
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.Message;
      if (msg && msg.toLowerCase().includes('already')) {
        setRegisterError('You have already registered for this event.');
      } else {
        setRegisterError(msg || 'Registration failed. Please try again.');
      }
    } finally {
      setRegisterLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="cdp-loading">
        <div className="spinner-c4k" />
        <p>Loading campaign...</p>
      </div>
    );
  }

  if (error || !campaign) {
    return (
      <div className="cdp-error-page">
        <Container>
          <Alert variant="danger">
            {error || 'Campaign does not exist'}
          </Alert>
          <Link to="/campaigns" className="btn-outline-teal">
            ← Back to Campaigns
          </Link>
        </Container>
      </div>
    );
  }

  const percentReached = Math.min(campaign.percentageReached || 0, 100);
  const isCompleted = campaign.status === 'Completed';
  const isEnded = campaign.status === 'Ended' || campaign.status === 'Expired' || campaign.status === 'Cancelled';
  const isActive = campaign.status === 'Active';
  const isUpcoming = campaign.status === 'Upcoming' || campaign.startDate > new Date().toISOString();

  // Determine days remaining display
  const getDaysDisplay = () => {
    if (isEnded) return 'Campaign Ended';
    if (isCompleted) return 'Goal Reached!';
    if (isUpcoming) return formatDate(campaign.startDate);
    if (campaign.daysRemaining != null && campaign.daysRemaining >= 0) {
      return `${campaign.daysRemaining} day${campaign.daysRemaining !== 1 ? 's' : ''} left`;
    }
    return '∞';
  };

  return (
    <div className="cdp-page">

      {/* ─── HERO ─── */}
      <section className="cdp-hero">
        <div className="cdp-hero-image">
          {campaign.imageUrl ? (
            <img
              src={campaign.imageUrl}
              alt={campaign.campaignName}
              onError={(e) => {
                e.target.src = 'https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=1600&q=80';
              }}
            />
          ) : (
            <div className="cdp-hero-placeholder" />
          )}
          <div className="cdp-hero-overlay" />
        </div>

        <Container className="cdp-hero-content">
          <Link to="/campaigns" className="cdp-back-link">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
            All Campaigns
          </Link>

          <div className="cdp-hero-badges">
            {campaign.cause?.causeName && (
              <span className="badge badge-teal">{campaign.cause.causeName}</span>
            )}
            <span className={`badge ${isActive ? 'badge-success' : 'badge-neutral'}`}>
              {campaign.status}
            </span>
            {campaign.isFeatured && (
              <span className="badge badge-coral">Featured</span>
            )}
          </div>

          <h1 className="cdp-hero-title">{campaign.campaignName}</h1>

          {campaign.location && (
            <p className="cdp-hero-meta">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
              {campaign.location}
              {campaign.beneficiariesCount && (
                <>
                  <span className="cdp-hero-divider">·</span>
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
                  {campaign.beneficiariesCount.toLocaleString('vi-VN')} children supported
                </>
              )}
            </p>
          )}
        </Container>
      </section>

      {/* ─── MAIN CONTENT ─── */}
      <section className="cdp-content">
        <Container>
          <Row>
            {/* Left column — story */}
            <Col lg={8} className="cdp-main">
              {/* Description */}
              <div className="cdp-section">
                <p className="eyebrow">About This Campaign</p>
                <p className="cdp-lead">{campaign.description}</p>
              </div>

              {/* Stats row */}
              <div className="cdp-stats-row">
                <div className="cdp-stat-card">
                  <div className="cdp-stat-num">{formatCurrency(campaign.raisedAmount)}</div>
                  <div className="cdp-stat-lbl">Raised So Far</div>
                </div>
                <div className="cdp-stat-card">
                  <div className="cdp-stat-num">{formatCurrency(campaign.goalAmount)}</div>
                  <div className="cdp-stat-lbl">Funding Goal</div>
                </div>
                <div className="cdp-stat-card">
                  <div className="cdp-stat-num">{campaign.donorCount || 0}</div>
                  <div className="cdp-stat-lbl">Generous Donors</div>
                </div>
                <div className="cdp-stat-card">
                  <div className="cdp-stat-num">
                    {getDaysDisplay()}
                  </div>
                  <div className="cdp-stat-lbl">Status</div>
                </div>
              </div>

              {/* Tabs */}
              <div className="cdp-section">
                <Tabs defaultActiveKey="details" className="cdp-tabs" mountOnEnter>
                  <Tab eventKey="details" title="Campaign Details">
                    <div className="cdp-tab-pane">
                      <h3 className="cdp-tab-title">Campaign Information</h3>

                      <div className="cdp-info-grid">
                        <div className="cdp-info-item">
                          <div className="cdp-info-label">Start Date</div>
                          <div className="cdp-info-value">{formatDate(campaign.startDate)}</div>
                        </div>
                        {campaign.endDate && (
                          <div className="cdp-info-item">
                            <div className="cdp-info-label">End Date</div>
                            <div className="cdp-info-value">{formatDate(campaign.endDate)}</div>
                          </div>
                        )}
                        {campaign.location && (
                          <div className="cdp-info-item">
                            <div className="cdp-info-label">Location</div>
                            <div className="cdp-info-value">{campaign.location}</div>
                          </div>
                        )}
                        {campaign.beneficiariesCount && (
                          <div className="cdp-info-item">
                            <div className="cdp-info-label">Beneficiaries</div>
                            <div className="cdp-info-value">
                              {campaign.beneficiariesCount.toLocaleString('vi-VN')} children
                            </div>
                          </div>
                        )}
                        <div className="cdp-info-item">
                          <div className="cdp-info-label">Progress</div>
                          <div className="cdp-info-value">{campaign.percentageReached?.toFixed(1) || 0}%</div>
                        </div>
                      </div>

                      {campaign.longDescription && (
                        <div className="cdp-long-desc">
                          <h3 className="cdp-tab-title">Our Approach</h3>
                          <div className="cdp-prose">
                            {campaign.longDescription.split('\n\n').map((para, i) => (
                              <p key={i}>{para}</p>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  </Tab>

                  <Tab eventKey="donations" title={`Donations (${campaign.recentDonations?.length || 0})`}>
                    <div className="cdp-tab-pane">
                      <h3 className="cdp-tab-title">Recent Donations</h3>

                      {campaign.recentDonations && campaign.recentDonations.length > 0 ? (
                        <div className="cdp-donations-list">
                          {campaign.recentDonations.map((donation, index) => (
                            <div key={index} className="cdp-donation-item">
                              <div className="cdp-donation-info">
                                <div className="cdp-donation-name">
                                  {donation.fullName || 'Anonymous Supporter'}
                                </div>
                                {donation.message && (
                                  <p className="cdp-donation-msg">"{donation.message}"</p>
                                )}
                                <div className="cdp-donation-date">
                                  {new Date(donation.donationDate).toLocaleDateString('en-US', {
                                    year: 'numeric', month: 'long', day: 'numeric'
                                  })}
                                </div>
                              </div>
                              <div className="cdp-donation-amount">
                                {formatCurrency(donation.amount)}
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <Alert variant="info" className="cdp-empty-alert">
                          No public donations to display yet. Be the first to support this campaign.
                        </Alert>
                      )}
                    </div>
                  </Tab>

                  <Tab eventKey="breakdown" title="Donation Breakdown">
                    <div className="cdp-tab-pane">
                      <h3 className="cdp-tab-title">Donations by Amount</h3>

                      {campaign.donationBreakdown && campaign.donationBreakdown.length > 0 ? (
                        <div className="cdp-breakdown">
                          {campaign.donationBreakdown.map((item, index) => (
                            <div key={index} className="cdp-breakdown-item">
                              <div className="cdp-breakdown-header">
                                <span className="cdp-breakdown-range">{item.range}</span>
                                <span className="cdp-breakdown-meta">
                                  {item.count} donation{item.count !== 1 ? 's' : ''} · {formatCurrency(item.total)}
                                </span>
                              </div>
                              <ProgressBar
                                now={(item.total / campaign.raisedAmount) * 100}
                                className="cdp-breakdown-progress"
                              />
                            </div>
                          ))}
                        </div>
                      ) : (
                        <Alert variant="info" className="cdp-empty-alert">
                          No donation breakdown data available
                        </Alert>
                      )}
                    </div>
                  </Tab>

                  <Tab eventKey="reports" title={`Impact Reports (${reports.length})`}>
                    <div className="cdp-tab-pane">
                      <h3 className="cdp-tab-title">Impact & Transparency Reports</h3>
                      <p className="text-muted mb-4">
                        See how your donations were used and the impact we achieved together.
                      </p>

                      {reportsLoading ? (
                        <div className="text-center py-4">
                          <div className="spinner-border spinner-border-sm text-primary" />
                          <p className="text-muted mt-2 mb-0 small">Loading reports…</p>
                        </div>
                      ) : reports.length === 0 ? (
                        <Alert variant="info" className="cdp-empty-alert">
                          <i className="bi bi-clipboard-data me-2"></i>
                          No impact reports have been published for this campaign yet.
                        </Alert>
                      ) : (
                        <div className="cdp-reports-list">
                          {reports.map((report) => {
                            const remaining = (report.totalReceived || 0) - (report.totalSpent || 0);
                            const spendPct = report.totalReceived > 0
                              ? Math.min((report.totalSpent / report.totalReceived) * 100, 100)
                              : 0;
                            return (
                              <div key={report.reportId} className="cdp-report-card">
                                <div className="cdp-report-header">
                                  <h4 className="cdp-report-title">
                                    <i className="bi bi-clipboard-check-fill text-success me-2"></i>
                                    {report.reportTitle}
                                  </h4>
                                  {report.publishedDate && (
                                    <Badge bg="success" pill>
                                      <i className="bi bi-calendar-check me-1"></i>
                                      {new Date(report.publishedDate).toLocaleDateString('en-US', {
                                        year: 'numeric', month: 'short', day: 'numeric'
                                      })}
                                    </Badge>
                                  )}
                                </div>

                                <div className="cdp-report-stats">
                                  <div className="cdp-report-stat">
                                    <div className="cdp-report-stat-num text-success">
                                      {formatCurrency(report.totalReceived)}
                                    </div>
                                    <div className="cdp-report-stat-lbl">Total Received</div>
                                  </div>
                                  <div className="cdp-report-stat">
                                    <div className="cdp-report-stat-num text-warning">
                                      {formatCurrency(report.totalSpent)}
                                    </div>
                                    <div className="cdp-report-stat-lbl">Total Spent</div>
                                  </div>
                                  <div className="cdp-report-stat">
                                    <div className="cdp-report-stat-num text-info">
                                      {formatCurrency(Math.max(remaining, 0))}
                                    </div>
                                    <div className="cdp-report-stat-lbl">Remaining</div>
                                  </div>
                                  <div className="cdp-report-stat">
                                    <div className="cdp-report-stat-num text-primary">
                                      {(report.beneficiariesReached ?? 0).toLocaleString('vi-VN')}
                                    </div>
                                    <div className="cdp-report-stat-lbl">Beneficiaries</div>
                                  </div>
                                </div>

                                <div className="cdp-report-progress-wrap">
                                  <div className="cdp-report-progress-label">
                                    <span>Fund Utilization</span>
                                    <span>{spendPct.toFixed(1)}%</span>
                                  </div>
                                  <ProgressBar
                                    now={spendPct}
                                    variant={spendPct >= 90 ? 'success' : spendPct >= 50 ? 'info' : 'warning'}
                                    className="cdp-report-progress"
                                  />
                                </div>

                                {report.reportContent && (
                                  <div className="cdp-report-content">
                                    <h6 className="cdp-report-content-title">
                                      <i className="bi bi-file-text me-1"></i>
                                      Report Summary
                                    </h6>
                                    {report.reportContent.split('\n\n').map((para, i) => (
                                      <p key={i} className="cdp-report-para">{para}</p>
                                    ))}
                                  </div>
                                )}

                                {report.expenseBreakdown && (() => {
                                  try {
                                    const items = JSON.parse(report.expenseBreakdown);
                                    if (Array.isArray(items) && items.length > 0) {
                                      return (
                                        <div className="cdp-report-expenses">
                                          <h6 className="cdp-report-content-title">
                                            <i className="bi bi-pie-chart me-1"></i>
                                            Expense Breakdown
                                          </h6>
                                          <div className="cdp-expense-list">
                                            {items.map((item, i) => (
                                              <div key={i} className="cdp-expense-row">
                                                <span className="cdp-expense-cat">{item.category || item.name || '—'}</span>
                                                <span className="cdp-expense-amt">
                                                  {formatCurrency(item.amount || 0)}
                                                </span>
                                              </div>
                                            ))}
                                          </div>
                                        </div>
                                      );
                                    }
                                  } catch (e) { /* ignore JSON parse errors */ }
                                  return null;
                                })()}
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>
                  </Tab>
                </Tabs>
              </div>
            </Col>

            {/* Right column — sticky donate */}
            <Col lg={4} className="cdp-sidebar-col">
              <div className="cdp-donate-card">
                <h3 className="cdp-donate-title">Support This Campaign</h3>

                <div className="cdp-donate-progress">
                  <div className="cdp-donate-progress-num">
                    {formatCurrency(campaign.raisedAmount)}
                  </div>
                  <div className="cdp-donate-progress-goal">
                    of {formatCurrency(campaign.goalAmount)} goal
                  </div>
                </div>

                <ProgressBar
                  now={percentReached}
                  className="cdp-main-progress"
                />

                <div className="cdp-donate-progress-meta">
                  <span className="cdp-donate-pct">
                    {isCompleted ? '🎉 Goal Reached!' : `${percentReached.toFixed(1)}% funded`}
                  </span>
                  {isActive && campaign.daysRemaining != null && campaign.daysRemaining > 0 && (
                    <span className="cdp-donate-days">{campaign.daysRemaining} days left</span>
                  )}
                  {isEnded && <span className="cdp-donate-days">Campaign Ended</span>}
                  {isUpcoming && <span className="cdp-donate-days">Starting {formatDate(campaign.startDate)}</span>}
                </div>

                {isActive ? (
                  <button className="btn-coral btn-lg w-100 cdp-donate-btn" onClick={handleDonate}>
                    Donate to This Campaign
                  </button>
                ) : (
                  <Alert variant="warning" className="cdp-ended-alert">
                    This campaign has {isCompleted ? 'been completed' : 'ended'}
                  </Alert>
                )}

                {/* ── Programme Registration CTA ── */}
                {campaign.registrationRequired && (
                  <div className="cdp-register-section">
                    {registerSuccess ? (
                      <Alert variant="success" className="cdp-register-success">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="20 6 9 17 4 12"/></svg>
                        {registerSuccess}
                      </Alert>
                    ) : (
                      <>
                        {registerError && (
                          <Alert variant="danger" className="cdp-register-error">{registerError}</Alert>
                        )}
                        {campaign.currentParticipants != null && campaign.maxParticipants && (
                          <div className="cdp-register-spots">
                            <div className="cdp-spots-label">
                              {campaign.maxParticipants - campaign.currentParticipants} spots remaining
                            </div>
                            <ProgressBar
                              now={Math.min((campaign.currentParticipants / campaign.maxParticipants) * 100, 100)}
                              className="cdp-spots-progress"
                            />
                          </div>
                        )}
                        <button
                          className="btn-teal btn-lg w-100 cdp-register-btn"
                          onClick={handleRegister}
                          disabled={registerLoading}
                        >
                          {registerLoading ? (
                            <><span className="spinner-border spinner-border-sm me-2"></span>Submitting...</>
                          ) : (
                            <>
                              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4">
                                <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                                <circle cx="8.5" cy="7" r="4"/>
                                <line x1="20" y1="8" x2="20" y2="14"/>
                                <line x1="23" y1="11" x2="17" y2="11"/>
                              </svg>
                              Register to Participate
                            </>
                          )}
                        </button>
                        <p className="cdp-register-note">
                          Free registration — join us in making a difference.
                        </p>
                      </>
                    )}
                  </div>
                )}

                <div className="cdp-share">
                  <p className="cdp-share-label">Share this campaign</p>
                  <div className="cdp-share-buttons">
                    <button className="cdp-share-btn" aria-label="Share on Facebook">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"/></svg>
                    </button>
                    <button className="cdp-share-btn" aria-label="Share on Twitter">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M23 3a10.9 10.9 0 0 1-3.14 1.53 4.48 4.48 0 0 0-7.86 3v1A10.66 10.66 0 0 1 3 4s-4 9 5 13a11.64 11.64 0 0 1-7 2c9 5 20 0 20-11.5a4.5 4.5 0 0 0-.08-.83A7.72 7.72 0 0 0 23 3z"/></svg>
                    </button>
                    <button className="cdp-share-btn" aria-label="Share on WhatsApp">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M17.5 14.4c-.3-.2-1.8-.9-2.1-1-.3-.1-.5-.2-.7.2s-.8 1-.9 1.2c-.2.2-.3.2-.6.1s-1.2-.5-2.3-1.4c-.9-.8-1.4-1.7-1.6-2-.2-.3 0-.4.1-.6.1-.1.3-.3.4-.5.1-.2.2-.3.3-.5.1-.2 0-.4 0-.5-.1-.2-.7-1.6-.9-2.2-.2-.6-.5-.5-.7-.5h-.6c-.2 0-.5.1-.7.4s-1 1-1 2.5 1.1 2.9 1.2 3.1c.2.2 2.1 3.2 5.2 4.5 2.3.9 3.1 1 4.2.7.6-.2 1.8-.8 2-1.5.2-.7.2-1.3.2-1.5-.1-.2-.3-.2-.6-.4z"/><path d="M12 2C6.5 2 2 6.5 2 12c0 1.8.5 3.5 1.3 5L2 22l5.3-1.4c1.5.8 3.1 1.3 4.7 1.3 5.5 0 10-4.5 10-10S17.5 2 12 2zm0 18.1c-1.5 0-3-.4-4.3-1.2l-.3-.2-3.2.8.9-3.1-.2-.3c-.9-1.4-1.4-3-1.4-4.7 0-4.6 3.7-8.3 8.3-8.3s8.3 3.7 8.3 8.3-3.5 8.7-8.1 8.7z"/></svg>
                    </button>
                    <button className="cdp-share-btn" aria-label="Copy link">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>
                    </button>
                  </div>
                </div>

                {campaign.campaignCode && (
                  <div className="cdp-code">
                    <div className="cdp-code-label">Campaign Reference</div>
                    <code className="cdp-code-value">{campaign.campaignCode}</code>
                  </div>
                )}
              </div>
            </Col>
          </Row>
        </Container>
      </section>

    </div>
  );
}

export default CampaignDetailPage;
