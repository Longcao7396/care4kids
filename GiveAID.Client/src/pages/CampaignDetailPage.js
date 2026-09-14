import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Container, Row, Col, ProgressBar, Badge, Alert, ListGroup, Tab, Tabs } from 'react-bootstrap';
import api from '../services/api';
import './CampaignDetailPage.css';

function CampaignDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [campaign, setCampaign] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchCampaignDetail = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get(`/campaigns/${id}`);
      if (response.data.success) {
        setCampaign(response.data.data);
      }
    } catch (err) {
      setError('Unable to load campaign information');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchCampaignDetail();
  }, [fetchCampaignDetail]);

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
    navigate('/donate', {
      state: {
        campaignId: campaign.campaignId,
        campaignName: campaign.campaignName,
        causeId: campaign.cause?.causeId
      }
    });
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
  const isActive = campaign.status === 'Active';

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
                    {campaign.daysRemaining != null && campaign.daysRemaining >= 0
                      ? campaign.daysRemaining
                      : '∞'}
                  </div>
                  <div className="cdp-stat-lbl">Days Remaining</div>
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
                  <span className="cdp-donate-pct">{percentReached.toFixed(1)}% funded</span>
                  {isActive && campaign.daysRemaining != null && campaign.daysRemaining >= 0 && (
                    <span className="cdp-donate-days">{campaign.daysRemaining} days left</span>
                  )}
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
