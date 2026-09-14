USE GiveAIDDB;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =====================================================
-- Clean up old Vietnamese placeholder campaigns (1-5)
-- Donations linked to them get cascaded via FK delete
-- =====================================================
DELETE FROM Donations WHERE campaign_id BETWEEN 1 AND 5;
DELETE FROM Campaigns WHERE campaign_id BETWEEN 1 AND 5;
GO

-- =====================================================
-- Insert the missing "Warm Winter for Children" campaign
-- (status 'Active' — DB CHECK constraint disallows 'Upcoming',
--  but the frontend will label upcoming campaigns by date)
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (1,
   N'Warm Winter for Children',
   'WINTER-2026',
   N'Northern Vietnam''s winters can be bitterly cold, and for children without warm clothing, the season brings real suffering. This campaign provides winter jackets, blankets, socks, hats and gloves to children in mountainous provinces. Together we can make sure no child has to choose between going to school and staying warm.',
   40000000, 7100000,
   '2026-10-15', '2027-01-31',
   'https://images.unsplash.com/photo-1545193544-312983719627?auto=format&fit=crop&w=1200&q=80',
   450, N'Northern mountainous provinces',
   'Active', 1, 7, 1);
GO

-- =====================================================
-- Make sure featured campaigns are flagged
-- =====================================================
UPDATE Campaigns SET is_featured = 1 WHERE campaign_id IN (6, 7, 8, 9);
GO

-- =====================================================
-- Verify
-- =====================================================
SELECT
  campaign_id,
  campaign_name,
  cause_id,
  status,
  is_featured,
  goal_amount,
  raised_amount,
  beneficiaries_count,
  CASE WHEN image_url IS NULL THEN 'NO IMAGE' ELSE 'OK' END AS ImageStatus
FROM Campaigns
ORDER BY is_featured DESC, display_order, campaign_id;
GO

SELECT COUNT(*) AS TotalCampaigns FROM Campaigns;
SELECT COUNT(*) AS TotalDonations FROM Donations;
GO

PRINT 'Cleanup + Warm Winter campaign inserted successfully.';
GO
