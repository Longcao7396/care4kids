-- =====================================================
-- CARE4KIDS CAMPAIGNS — Realistic NGO Data
-- =====================================================
USE GiveAIDDB;
GO

-- Use real Unsplash images of children (warm, human, NGO-appropriate)
-- All URLs are direct images from Unsplash CDN, free for use

-- Wipe old Vietnamese seed campaigns and replace with English ones
DELETE FROM CampaignReports WHERE campaign_id BETWEEN 1 AND 9;
DELETE FROM Donations WHERE campaign_id BETWEEN 1 AND 9;
DELETE FROM Campaigns WHERE campaign_id BETWEEN 1 AND 9;
DBCC CHECKIDENT('Campaigns', RESEED, 0);
GO

-- =====================================================
-- Ensure Children Welfare cause has enough categories
-- =====================================================
-- cause_id 1 = Children Welfare (used as fallback)
-- cause_id 2 = Education
-- We will use cause_id 1 (Children Welfare) for all campaigns to keep them
-- focused on the NGO mission, since children's welfare is the core.

-- =====================================================
-- CAMPAIGN 1 — Nutritious Meals for Children
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (1,
   N'Nutritious Meals for Children',
   'MEALS-2026',
   N'Many children in difficult circumstances go to bed hungry. Through this campaign, we provide daily balanced meals — including rice, vegetables, protein and milk — to children at care homes and rural community centres. Every meal we serve brings strength, focus, and hope to a child who needs it most. Your gift of just 50,000 VND can help provide one full, nutritious meal.',
   45000000, 21500000,
   '2026-08-01', '2026-11-30',
   'https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?auto=format&fit=crop&w=1200&q=80',
   350, N'Ho Chi Minh City & surrounding provinces',
   'Active', 1, 1, 1);
GO

-- =====================================================
-- CAMPAIGN 2 — School Supplies for a Brighter Future
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (2,
   N'School Supplies for a Brighter Future',
   'SUPPLIES-2026',
   N'For children from low-income families, going back to school often means choosing between buying a notebook or buying dinner. This campaign provides essential school supplies — backpacks, textbooks, notebooks, pens, pencils and learning kits — so that no child is held back simply because their family cannot afford the basics. A small donation can put a full set of learning tools into the hands of a child ready to learn.',
   30000000, 12750000,
   '2026-08-15', '2026-10-15',
   'https://images.unsplash.com/photo-1503676260728-1c00da094a0b?auto=format&fit=crop&w=1200&q=80',
   500, N'Hanoi, Hai Phong and northern provinces',
   'Active', 1, 2, 1);
GO

-- =====================================================
-- CAMPAIGN 3 — Support Children at Care Homes
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (1,
   N'Support Children at Care Homes',
   'CARE-HOMES-2026',
   N'Across Vietnam, hundreds of children live in small care homes that depend entirely on community goodwill. This campaign funds monthly food baskets, new clothing, hygiene supplies and educational materials for children in 12 partner care homes. Your support gives caregivers the resources they need and gives children the comfort of knowing someone out there is rooting for them.',
   60000000, 38200000,
   '2026-06-01', '2027-02-28',
   'https://images.unsplash.com/photo-1542810634-71277d95dcbb?auto=format&fit=crop&w=1200&q=80',
   220, N'Multiple cities in Vietnam',
   'Active', 1, 3, 1);
GO

-- =====================================================
-- CAMPAIGN 4 — Children's Healthcare Support
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (1,
   N'Children''s Healthcare Support',
   'HEALTH-2026',
   N'Children should never go without medical care because their family cannot afford it. This campaign funds routine health check-ups, essential medicines, vaccinations, and emergency treatment for children in underserved communities. We partner with local clinics and pediatric specialists to ensure every child receives timely, compassionate care.',
   50000000, 18500000,
   '2026-07-01', '2026-12-31',
   'https://images.unsplash.com/photo-1581594693702-fbdc51b2763b?auto=format&fit=crop&w=1200&q=80',
   180, N'Mekong Delta region',
   'Active', 1, 4, 1);
GO

-- =====================================================
-- CAMPAIGN 5 — Back-to-School Support
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (2,
   N'Back-to-School Support',
   'BACK2SCHOOL-2026',
   N'A new school year should be an exciting milestone — not a financial burden. Our Back-to-School Support campaign provides uniforms, shoes, school bags, and complete learning kits to children from families facing financial hardship. Together, we can make sure every child walks into the new school year with confidence, dignity and the right tools to succeed.',
   35000000, 9800000,
   '2026-08-20', '2026-09-15',
   'https://images.unsplash.com/photo-1497486751825-1233686d5d80?auto=format&fit=crop&w=1200&q=80',
   400, N'Central Highlands provinces',
   'Active', 0, 5, 1);
GO

-- =====================================================
-- CAMPAIGN 6 — Gifts for Children
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (1,
   N'Gifts for Children',
   'GIFTS-2026',
   N'Every child deserves to feel remembered and loved on their birthday and during the festive season. Through this campaign, we deliver carefully chosen gifts — toys, books, art supplies, and personalised care packages — to children in hospitals, care homes and remote communities. A single gift can brighten a child''s entire year.',
   20000000, 14300000,
   '2026-09-01', '2026-12-25',
   'https://images.unsplash.com/photo-1513885535751-8b9238bd345a?auto=format&fit=crop&w=1200&q=80',
   600, N'Nationwide',
   'Active', 0, 6, 1);
GO

-- =====================================================
-- CAMPAIGN 7 — Warm Winter for Children
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
   'Upcoming', 1, 7, 1);
GO

-- =====================================================
-- CAMPAIGN 8 — Education Opportunity Fund
-- =====================================================
INSERT INTO Campaigns
  (cause_id, campaign_name, campaign_code, description, goal_amount, raised_amount,
   start_date, end_date, image_url, beneficiaries_count, location,
   status, is_featured, display_order, created_by)
VALUES
  (2,
   N'Education Opportunity Fund',
   'EDU-FUND-2026',
   N'This long-term scholarship program supports promising students from disadvantaged backgrounds through secondary school and university. Donations cover tuition fees, textbooks, mentoring, and a small monthly stipend so that students can focus entirely on their studies. Every scholar we support today becomes a future role model for their community.',
   80000000, 45600000,
   '2026-05-01', '2027-06-30',
   'https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=1200&q=80',
   80, N'Nationwide',
   'Active', 0, 8, 1);
GO

-- =====================================================
-- Make campaign 1, 2, 3 featured (top of list)
-- =====================================================
UPDATE Campaigns SET is_featured = 1 WHERE campaign_id IN (1, 2, 3);
GO

-- =====================================================
-- Insert realistic donations for demo (so progress bars feel real)
-- User IDs 1, 2, 3 exist in the system. We cycle through them.
-- =====================================================
INSERT INTO Donations (user_id, cause_id, campaign_id, organization_id, amount, payment_method, payment_status, card_last_four, card_type, is_anonymous, donation_date, transaction_id)
VALUES
  (2, 1, 1, NULL, 500000, 'CreditCard', 'Completed', '1234', 'Visa', 0, '2026-08-05', 'TXN-C4K-0001'),
  (3, 1, 1, NULL, 200000, 'CreditCard', 'Completed', '5678', 'MasterCard', 0, '2026-08-12', 'TXN-C4K-0002'),
  (2, 1, 1, NULL, 1000000, 'BankTransfer', 'Completed', NULL, NULL, 0, '2026-09-01', 'TXN-C4K-0003'),
  (3, 1, 1, NULL, 300000, 'CreditCard', 'Completed', '9012', 'Visa', 1, '2026-09-04', 'TXN-C4K-0004'),
  (3, 1, 2, NULL, 250000, 'CreditCard', 'Completed', '3456', 'Visa', 0, '2026-08-20', 'TXN-C4K-0005'),
  (2, 1, 2, NULL, 500000, 'CreditCard', 'Completed', '7890', 'Visa', 0, '2026-08-28', 'TXN-C4K-0006'),
  (3, 1, 3, NULL, 2000000, 'BankTransfer', 'Completed', NULL, NULL, 0, '2026-07-15', 'TXN-C4K-0007'),
  (1, 1, 3, NULL, 5000000, 'BankTransfer', 'Completed', NULL, NULL, 1, '2026-08-10', 'TXN-C4K-0008'),
  (2, 1, 4, NULL, 800000, 'CreditCard', 'Completed', '1111', 'MasterCard', 0, '2026-07-25', 'TXN-C4K-0009'),
  (3, 1, 5, NULL, 400000, 'CreditCard', 'Completed', '2222', 'Visa', 0, '2026-08-22', 'TXN-C4K-0010'),
  (1, 1, 6, NULL, 1500000, 'CreditCard', 'Completed', '3333', 'Visa', 1, '2026-09-02', 'TXN-C4K-0011'),
  (2, 2, 8, NULL, 5000000, 'BankTransfer', 'Completed', NULL, NULL, 0, '2026-06-12', 'TXN-C4K-0012'),
  (3, 2, 8, NULL, 3500000, 'BankTransfer', 'Completed', NULL, NULL, 0, '2026-07-08', 'TXN-C4K-0013');
GO

PRINT 'Care4Kids campaigns seeded successfully.';
SELECT COUNT(*) AS TotalCampaigns FROM Campaigns;
SELECT COUNT(*) AS TotalDonations FROM Donations;
GO
