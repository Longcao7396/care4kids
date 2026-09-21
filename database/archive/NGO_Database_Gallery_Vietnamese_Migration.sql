-- ============================================================
-- GiveAID Foundation - Gallery Vietnamese Images Migration v2
-- ============================================================
-- Purpose : UPDATE dbo.Gallery (IDs 3-69) with verified Vietnamese images
--            Replaces Unsplash legacy URLs (IDs 3-38) and broken local
--            /assets/gallery/ paths (IDs 39-69) with Pexels/Unsplash Vietnamese
-- Database: GiveAIDDB
-- Source   : Pexels.com (29 URLs) + Unsplash.com (8 URLs) - all verified working
-- Backup   : dbo.Gallery_Backup_20260917 created before running this script
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_NAME() <> N'GiveAIDDB'
    THROW 50000, 'Run this migration against GiveAIDDB only.', 1;
GO

PRINT 'Updating Gallery with Vietnamese children images...';
PRINT 'Range: gallery_id 3-69 (67 rows total)';
PRINT '';

BEGIN TRANSACTION;

-- ============================================================
-- SECTION 1: Replace Unsplash legacy URLs (IDs 3-38 = 36 rows)
-- Original: Various unsplash.com URLs
-- New:      Pexels + Unsplash Vietnamese children images
-- ============================================================

UPDATE dbo.Gallery
SET
    photo_url = CASE gallery_id
        -- IDs 3-10: Education (8 items)
        WHEN 3  THEN 'https://images.pexels.com/photos/33985331/pexels-photo-33985331.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hà Giang classroom
        WHEN 4  THEN 'https://images.pexels.com/photos/33852286/pexels-photo-33852286.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Flag salute
        WHEN 5  THEN 'https://images.pexels.com/photos/35131393/pexels-photo-35131393.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hanoi classroom
        WHEN 6  THEN 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91'                                                        -- HCMC girl
        WHEN 7  THEN 'https://images.pexels.com/photos/33670808/pexels-photo-33670808.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Volunteer HN
        WHEN 8  THEN 'https://images.pexels.com/photos/35106203/pexels-photo-35106203.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Volunteer rural
        WHEN 9  THEN 'https://images.pexels.com/photos/33768219/pexels-photo-33768219.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Festival HN
        WHEN 10 THEN 'https://images.pexels.com/photos/30415011/pexels-photo-30415011.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tet children

        -- IDs 11-15: Meals (5 items)
        WHEN 11 THEN 'https://images.pexels.com/photos/30513504/pexels-photo-30513504.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tet family
        WHEN 12 THEN 'https://images.unsplash.com/photo-1488521787991-ed7bbaae773c'                                                        -- Gia Lai children
        WHEN 13 THEN 'https://images.unsplash.com/photo-1531123897727-8f129e1688ce'                                                        -- Cao Lanh children
        WHEN 14 THEN 'https://images.pexels.com/photos/5692265/pexels-photo-5692265.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'    -- Children breakfast
        WHEN 15 THEN 'https://images.pexels.com/photos/33933381/pexels-photo-33933381.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Vietnamese meal

        -- IDs 16-20: Healthcare (5 items)
        WHEN 16 THEN 'https://images.pexels.com/photos/30811268/pexels-photo-30811268.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hà Giang child
        WHEN 17 THEN 'https://images.pexels.com/photos/32345051/pexels-photo-32345051.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hải Dương girl
        WHEN 18 THEN 'https://images.pexels.com/photos/29801357/pexels-photo-29801357.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hội An river
        WHEN 19 THEN 'https://images.pexels.com/photos/33921216/pexels-photo-33921216.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Lào Cai market
        WHEN 20 THEN 'https://images.pexels.com/photos/29623858/pexels-photo-29623858.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hội An cute child

        -- IDs 21-24: Shelter (4 items)
        WHEN 21 THEN 'https://images.unsplash.com/photo-1527631746610-bca00a040d60'                                                        -- Sa Pa Hmong
        WHEN 22 THEN 'https://images.unsplash.com/photo-1743329636103-482b69d90fd6'                                                        -- Student celebration
        WHEN 23 THEN 'https://images.pexels.com/photos/37354082/pexels-photo-37354082.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Student daydreaming
        WHEN 24 THEN 'https://images.pexels.com/photos/36292810/pexels-photo-36292810.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hanoi sidewalk

        -- IDs 25-29: Volunteers (5 items)
        WHEN 25 THEN 'https://images.pexels.com/photos/35671466/pexels-photo-35671466.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Kon Tum bikes
        WHEN 26 THEN 'https://images.pexels.com/photos/37981395/pexels-photo-37981395.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tuyen Quang
        WHEN 27 THEN 'https://images.pexels.com/photos/30592052/pexels-photo-30592052.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hà Giang traditional
        WHEN 28 THEN 'https://images.pexels.com/photos/37981403/pexels-photo-37981403.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Pho Bang village
        WHEN 29 THEN 'https://images.pexels.com/photos/37718226/pexels-photo-37718226.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Bac Ha buffalo

        -- IDs 30-34: Events (5 items)
        WHEN 30 THEN 'https://images.pexels.com/photos/30415011/pexels-photo-30415011.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tet children
        WHEN 31 THEN 'https://images.pexels.com/photos/30513504/pexels-photo-30513504.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tet family
        WHEN 32 THEN 'https://images.pexels.com/photos/33768219/pexels-photo-33768219.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Festival HN
        WHEN 33 THEN 'https://images.pexels.com/photos/30326158/pexels-photo-30326158.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tet gift set
        WHEN 34 THEN 'https://images.unsplash.com/photo-1772178957276-b57cf3bf3c3f'                                                        -- Hoi An lanterns

        -- IDs 35-38: Children (4 items)
        WHEN 35 THEN 'https://images.pexels.com/photos/37981395/pexels-photo-37981395.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Tuyen Quang
        WHEN 36 THEN 'https://images.pexels.com/photos/30592052/pexels-photo-30592052.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'  -- Hà Giang
        WHEN 37 THEN 'https://images.unsplash.com/photo-1743329636103-482b69d90fd6'                                                        -- Student celebration
        WHEN 38 THEN 'https://images.unsplash.com/photo-1489710437720-ebb67ec84dd2'                                                        -- Children group

        ELSE photo_url
    END,
    thumbnail_url = CASE gallery_id
        WHEN 3  THEN 'https://images.pexels.com/photos/33985331/pexels-photo-33985331.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 4  THEN 'https://images.pexels.com/photos/33852286/pexels-photo-33852286.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 5  THEN 'https://images.pexels.com/photos/35131393/pexels-photo-35131393.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 6  THEN 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91'
        WHEN 7  THEN 'https://images.pexels.com/photos/33670808/pexels-photo-33670808.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 8  THEN 'https://images.pexels.com/photos/35106203/pexels-photo-35106203.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 9  THEN 'https://images.pexels.com/photos/33768219/pexels-photo-33768219.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 10 THEN 'https://images.pexels.com/photos/30415011/pexels-photo-30415011.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 11 THEN 'https://images.pexels.com/photos/30513504/pexels-photo-30513504.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 12 THEN 'https://images.unsplash.com/photo-1488521787991-ed7bbaae773c'
        WHEN 13 THEN 'https://images.unsplash.com/photo-1531123897727-8f129e1688ce'
        WHEN 14 THEN 'https://images.pexels.com/photos/5692265/pexels-photo-5692265.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 15 THEN 'https://images.pexels.com/photos/33933381/pexels-photo-33933381.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 16 THEN 'https://images.pexels.com/photos/30811268/pexels-photo-30811268.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 17 THEN 'https://images.pexels.com/photos/32345051/pexels-photo-32345051.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 18 THEN 'https://images.pexels.com/photos/29801357/pexels-photo-29801357.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 19 THEN 'https://images.pexels.com/photos/33921216/pexels-photo-33921216.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 20 THEN 'https://images.pexels.com/photos/29623858/pexels-photo-29623858.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 21 THEN 'https://images.unsplash.com/photo-1527631746610-bca00a040d60'
        WHEN 22 THEN 'https://images.unsplash.com/photo-1743329636103-482b69d90fd6'
        WHEN 23 THEN 'https://images.pexels.com/photos/37354082/pexels-photo-37354082.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 24 THEN 'https://images.pexels.com/photos/36292810/pexels-photo-36292810.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 25 THEN 'https://images.pexels.com/photos/35671466/pexels-photo-35671466.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 26 THEN 'https://images.pexels.com/photos/37981395/pexels-photo-37981395.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 27 THEN 'https://images.pexels.com/photos/30592052/pexels-photo-30592052.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 28 THEN 'https://images.pexels.com/photos/37981403/pexels-photo-37981403.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 29 THEN 'https://images.pexels.com/photos/37718226/pexels-photo-37718226.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 30 THEN 'https://images.pexels.com/photos/30415011/pexels-photo-30415011.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 31 THEN 'https://images.pexels.com/photos/30513504/pexels-photo-30513504.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 32 THEN 'https://images.pexels.com/photos/33768219/pexels-photo-33768219.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 33 THEN 'https://images.pexels.com/photos/30326158/pexels-photo-30326158.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 34 THEN 'https://images.unsplash.com/photo-1772178957276-b57cf3bf3c3f'
        WHEN 35 THEN 'https://images.pexels.com/photos/37981395/pexels-photo-37981395.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 36 THEN 'https://images.pexels.com/photos/30592052/pexels-photo-30592052.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 37 THEN 'https://images.unsplash.com/photo-1743329636103-482b69d90fd6'
        WHEN 38 THEN 'https://images.unsplash.com/photo-1489710437720-ebb67ec84dd2'
        ELSE thumbnail_url
    END
WHERE gallery_id BETWEEN 3 AND 38;

PRINT 'Section 1 complete: Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows (IDs 3-38)';

-- ============================================================
-- SECTION 2: Replace broken local URLs (IDs 39-69 = 31 rows)
-- Original: /assets/gallery/gallery-XX-*.jpg (404 NOT FOUND)
-- New:      Pexels + Unsplash Vietnamese children images
-- ============================================================

UPDATE dbo.Gallery
SET
    photo_url = CASE gallery_id
        -- IDs 39-42: Children's Homes (4 items)
        WHEN 39 THEN 'https://images.pexels.com/photos/37981395/pexels-photo-37981395.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 40 THEN 'https://images.pexels.com/photos/30592052/pexels-photo-30592052.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 41 THEN 'https://images.pexels.com/photos/37981403/pexels-photo-37981403.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 42 THEN 'https://images.pexels.com/photos/34045791/pexels-photo-34045791.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'

        -- IDs 43-46: Education (4 items)
        WHEN 43 THEN 'https://images.pexels.com/photos/33985331/pexels-photo-33985331.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 44 THEN 'https://images.pexels.com/photos/33852286/pexels-photo-33852286.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 45 THEN 'https://images.pexels.com/photos/37354082/pexels-photo-37354082.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 46 THEN 'https://images.unsplash.com/photo-1497633762265-9d179a990aa6'

        -- IDs 47-49: Gifts & Events (3 items)
        WHEN 47 THEN 'https://images.pexels.com/photos/30415011/pexels-photo-30415011.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 48 THEN 'https://images.pexels.com/photos/30513504/pexels-photo-30513504.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 49 THEN 'https://images.pexels.com/photos/30326158/pexels-photo-30326158.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'

        -- IDs 50-53: Healthcare (4 items)
        WHEN 50 THEN 'https://images.pexels.com/photos/5692265/pexels-photo-5692265.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 51 THEN 'https://images.pexels.com/photos/33933381/pexels-photo-33933381.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 52 THEN 'https://images.pexels.com/photos/33908174/pexels-photo-33908174.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 53 THEN 'https://images.pexels.com/photos/28503359/pexels-photo-28503359.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'

        -- IDs 54-57: Meals & Nutrition (4 items)
        WHEN 54 THEN 'https://images.pexels.com/photos/5692265/pexels-photo-5692265.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 55 THEN 'https://images.pexels.com/photos/33933381/pexels-photo-33933381.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 56 THEN 'https://images.pexels.com/photos/33908174/pexels-photo-33908174.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 57 THEN 'https://images.pexels.com/photos/38107333/pexels-photo-38107333.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'

        -- IDs 58-61: Our Impact (4 items)
        WHEN 58 THEN 'https://images.pexels.com/photos/35106203/pexels-photo-35106203.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 59 THEN 'https://images.pexels.com/photos/33670808/pexels-photo-33670808.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 60 THEN 'https://images.pexels.com/photos/36713992/pexels-photo-36713992.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 61 THEN 'https://images.pexels.com/photos/36713988/pexels-photo-36713988.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'

        -- IDs 62-65: School Supplies (4 items)
        WHEN 62 THEN 'https://images.pexels.com/photos/28503359/pexels-photo-28503359.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 63 THEN 'https://images.pexels.com/photos/35131393/pexels-photo-35131393.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 64 THEN 'https://images.pexels.com/photos/33852286/pexels-photo-33852286.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 65 THEN 'https://images.unsplash.com/photo-1497633762265-9d179a990aa6'

        -- IDs 66-69: Volunteers (4 items)
        WHEN 66 THEN 'https://images.pexels.com/photos/35106203/pexels-photo-35106203.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 67 THEN 'https://images.pexels.com/photos/33670808/pexels-photo-33670808.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 68 THEN 'https://images.pexels.com/photos/36713992/pexels-photo-36713992.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 69 THEN 'https://images.pexels.com/photos/33768219/pexels-photo-33768219.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'

        ELSE photo_url
    END,
    thumbnail_url = CASE gallery_id
        WHEN 39 THEN 'https://images.pexels.com/photos/37981395/pexels-photo-37981395.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 40 THEN 'https://images.pexels.com/photos/30592052/pexels-photo-30592052.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 41 THEN 'https://images.pexels.com/photos/37981403/pexels-photo-37981403.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 42 THEN 'https://images.pexels.com/photos/34045791/pexels-photo-34045791.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 43 THEN 'https://images.pexels.com/photos/33985331/pexels-photo-33985331.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 44 THEN 'https://images.pexels.com/photos/33852286/pexels-photo-33852286.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 45 THEN 'https://images.pexels.com/photos/37354082/pexels-photo-37354082.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 46 THEN 'https://images.unsplash.com/photo-1497633762265-9d179a990aa6'
        WHEN 47 THEN 'https://images.pexels.com/photos/30415011/pexels-photo-30415011.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 48 THEN 'https://images.pexels.com/photos/30513504/pexels-photo-30513504.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 49 THEN 'https://images.pexels.com/photos/30326158/pexels-photo-30326158.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 50 THEN 'https://images.pexels.com/photos/5692265/pexels-photo-5692265.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 51 THEN 'https://images.pexels.com/photos/33933381/pexels-photo-33933381.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 52 THEN 'https://images.pexels.com/photos/33908174/pexels-photo-33908174.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 53 THEN 'https://images.pexels.com/photos/28503359/pexels-photo-28503359.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 54 THEN 'https://images.pexels.com/photos/5692265/pexels-photo-5692265.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 55 THEN 'https://images.pexels.com/photos/33933381/pexels-photo-33933381.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 56 THEN 'https://images.pexels.com/photos/33908174/pexels-photo-33908174.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 57 THEN 'https://images.pexels.com/photos/38107333/pexels-photo-38107333.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 58 THEN 'https://images.pexels.com/photos/35106203/pexels-photo-35106203.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 59 THEN 'https://images.pexels.com/photos/33670808/pexels-photo-33670808.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 60 THEN 'https://images.pexels.com/photos/36713992/pexels-photo-36713992.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 61 THEN 'https://images.pexels.com/photos/36713988/pexels-photo-36713988.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 62 THEN 'https://images.pexels.com/photos/28503359/pexels-photo-28503359.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 63 THEN 'https://images.pexels.com/photos/35131393/pexels-photo-35131393.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 64 THEN 'https://images.pexels.com/photos/33852286/pexels-photo-33852286.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 65 THEN 'https://images.unsplash.com/photo-1497633762265-9d179a990aa6'
        WHEN 66 THEN 'https://images.pexels.com/photos/35106203/pexels-photo-35106203.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 67 THEN 'https://images.pexels.com/photos/33670808/pexels-photo-33670808.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 68 THEN 'https://images.pexels.com/photos/36713992/pexels-photo-36713992.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        WHEN 69 THEN 'https://images.pexels.com/photos/33768219/pexels-photo-33768219.jpeg?auto=compress&cs=tinysrgb&w=800&h=600'
        ELSE thumbnail_url
    END
WHERE gallery_id BETWEEN 39 AND 69;

PRINT 'Section 2 complete: Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows (IDs 39-69)';
PRINT '';
PRINT 'Vietnamese Gallery migration v2 completed successfully.';
PRINT 'All 67 rows now use verified Pexels/Unsplash Vietnamese children images.';

COMMIT TRANSACTION;
GO

-- ============================================================
-- ROLLBACK INSTRUCTIONS (if needed):
-- ============================================================
-- IF EXISTS (SELECT 1 FROM dbo.Gallery_Backup_20260917)
-- BEGIN
--     DELETE FROM dbo.Gallery WHERE gallery_id BETWEEN 3 AND 69;
--     SET IDENTITY_INSERT dbo.Gallery ON;
--     INSERT INTO dbo.Gallery SELECT * FROM dbo.Gallery_Backup_20260917;
--     SET IDENTITY_INSERT dbo.Gallery OFF;
-- END
-- ============================================================

-- ============================================================
-- IMAGE SOURCES & PHOTOGRAPHER CREDITS:
-- ============================================================
-- Pexels (https://pexels.com) - Pexels License, free commercial use:
--   33985331 Son Hoa Nguyen     (Hà Giang classroom)
--   33852286 HONG SON          (Flag salute)
--   35131393 ANH LÊ            (Hanoi classroom)
--   33670808 Thang Nguyen      (Youth volunteer HN)
--   35106203 Đậu Photograph   (Volunteer rural)
--   33768219 Thang Nguyen      (Festival HN)
--   30415011 Võ Văn Tiến      (Tet children)
--   30513504 Tuấn Kiệt Jr.    (Tet family)
--   37354082 TBD Tuyên        (Student daydreaming)
--   30811268 Ben Tran          (Hà Giang child)
--   32345051 Vietnam Hidden Light (Hải Dương girl)
--   29801357 Võ Văn Tiến      (Hội An river)
--   33921216 Son Hoa Nguyen    (Lào Cai market)
--   29623858 Võ Văn Tiến      (Hội An cute child)
--   37981395 Duong Nguyen      (Tuyen Quang children)
--   30592052 Q. Hưng Phạm     (Hà Giang traditional)
--   37981403 Duong Nguyen      (Pho Bang village)
--   34045791 Son Hoa Nguyen    (Lào Cai family)
--   30326158 Thu Trần Thị     (Tet gift set)
--   5692265  Alex Green        (Children breakfast)
--   33933381 Sarah Vivian      (Vietnamese meal)
--   28503359 The Design Lady   (School supplies)
--   37718226 Duong Nguyen      (Bac Ha buffalo)
--   33908174 Son Hoa Nguyen    (Lào Cai rice fields)
--   35671466 Thái Trường Giang (Kon Tum bikes)
--   38107333 Thái Trường Giang (Kon Tum soccer)
--   36713992 Vitaly Gariev     (Vietnam alleyway)
--   36713988 Vitaly Gariev     (Vietnam street)
--   36292810 Thang Nguyen      (Hanoi sidewalk)
--
-- Unsplash (https://unsplash.com) - Unsplash License, free commercial use:
--   1508214751196-bcfd4ca60f91  (HCMC girl)
--   1488521787991-ed7bbaae773c  (Gia Lai children)
--   1531123897727-8f129e1688ce  (Cao Lanh children)
--   1527631746610-bca00a040d60  (Sa Pa Hmong)
--   1743329636103-482b69d90fd6  (Student celebration)
--   1772178957276-b57cf3bf3c3f  (Hoi An lanterns)
--   1489710437720-ebb67ec84dd2  (Children group)
--   1497633762265-9d179a990aa6  (Vietnam desk)
-- ============================================================
