-- Performance indexes for GiveAIDDB
-- Created: 2026-09-18

USE [GiveAIDDB];
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Index for recent completed donations query (AdminDashboard)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Donations_PaymentStatus_DonationDate')
CREATE NONCLUSTERED INDEX IX_Donations_PaymentStatus_DonationDate
ON Donations(payment_status, donation_date DESC)
INCLUDE (amount);

-- Index for user donation history (MyDonationsPage)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Donations_UserId_DonationDate')
CREATE NONCLUSTERED INDEX IX_Donations_UserId_DonationDate
ON Donations(user_id, donation_date DESC);

-- Index for active campaign filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Campaigns_Status_StartDate')
CREATE NONCLUSTERED INDEX IX_Campaigns_Status_StartDate
ON Campaigns(status, start_date, end_date)
INCLUDE (campaign_name, goal_amount, raised_amount);

-- Index for campaigns by cause
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Campaigns_CauseId_Status')
CREATE NONCLUSTERED INDEX IX_Campaigns_CauseId_Status
ON Campaigns(cause_id, status);

-- Index for campaigns by organization
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Campaigns_OrganizationId')
CREATE NONCLUSTERED INDEX IX_Campaigns_OrganizationId
ON Campaigns(organization_id)
WHERE organization_id IS NOT NULL;

-- Index for campaign reports by campaign
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CampaignReports_CampaignId_PublishedDate')
CREATE NONCLUSTERED INDEX IX_CampaignReports_CampaignId_PublishedDate
ON CampaignReports(campaign_id, published_date DESC)
WHERE is_published = 1;

-- Index for active causes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Causes_IsActive_DisplayOrder')
CREATE NONCLUSTERED INDEX IX_Causes_IsActive_DisplayOrder
ON Causes(is_active, display_order)
INCLUDE (cause_name, image_url);

-- Index for gallery by category and featured
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Gallery_IsFeatured_DisplayOrder')
CREATE NONCLUSTERED INDEX IX_Gallery_IsFeatured_DisplayOrder
ON Gallery(is_featured, display_order)
WHERE is_featured = 1;

-- Index for conversation messages
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ConversationMessages_ConversationId_CreatedAt')
CREATE NONCLUSTERED INDEX IX_ConversationMessages_ConversationId_CreatedAt
ON ConversationMessages(conversation_id, created_at);

PRINT 'Performance indexes created successfully';
GO
