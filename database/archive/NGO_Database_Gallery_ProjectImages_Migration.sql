-- =====================================================
-- GALLERY SEED — Add 10 real project images
-- Images are stored in GiveAID.Client/public/assets/gallery/
-- =====================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_NAME() <> N'GiveAIDDB'
    THROW 50000, 'Run this migration against GiveAIDDB only.', 1;
GO

PRINT 'Seeding 10 new Gallery items from project images...';

-- Get a sample OrganizationId if available
DECLARE @SampleOrgId INT = NULL;
SELECT TOP 1 @SampleOrgId = OrganizationId FROM dbo.Organizations WHERE IsActive = 1;

SET IDENTITY_INSERT dbo.Gallery ON;

-- Find the current max gallery_id to continue from
DECLARE @MaxId INT;
SELECT @MaxId = ISNULL(MAX(gallery_id), 0) FROM dbo.Gallery;

INSERT INTO dbo.Gallery
    (gallery_id, title, photo_url, thumbnail_url, category, tags,
     programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    -- Activities - Kids Sports
    (@MaxId + 1,
     N'Trẻ em vui chơi thể thao',
     N'/assets/gallery/gallery-01-kids-sports.jpg',
     N'/assets/gallery/gallery-01-kids-sports.jpg',
     N'Activities',
     N'thiếu nhi,thể thao,vui chơi,năng động',
     NULL, @SampleOrgId, @MaxId + 1, 1, GETDATE()),

    -- Volunteers - Team Photo
    (@MaxId + 2,
     N'Đội ngũ tình nguyện viên Care4Kids',
     N'/assets/gallery/gallery-02-volunteer-team.jpg',
     N'/assets/gallery/gallery-02-volunteer-team.jpg',
     N'Volunteers',
     N'tình nguyện viên,đội nhóm,Care4Kids,nhiệt huyết',
     NULL, @SampleOrgId, @MaxId + 2, 1, GETDATE()),

    -- Volunteers - Summer Campaign
    (@MaxId + 3,
     N'Chiến dịch tình nguyện mùa hè 2024',
     N'/assets/gallery/gallery-03-summer-volunteer.webp',
     N'/assets/gallery/gallery-03-summer-volunteer.webp',
     N'Volunteers',
     N'mùa hè,chiến dịch,tình nguyện,hè 2024',
     NULL, @SampleOrgId, @MaxId + 3, 1, GETDATE()),

    -- Education - Anti-Bullying Program
    (@MaxId + 4,
     N'Chương trình giáo dục phòng chống bắt nạt',
     N'/assets/gallery/gallery-04-anti-bullying.jpg',
     N'/assets/gallery/gallery-04-anti-bullying.jpg',
     N'Education',
     N'bắt nạt,giáo dục,phòng chống,trường học',
     NULL, @SampleOrgId, @MaxId + 4, 1, GETDATE()),

    -- Activities - Community Impact
    (@MaxId + 5,
     N'Hoạt động cộng đồng Care4Kids',
     N'/assets/gallery/gallery-05-community.jpg',
     N'/assets/gallery/gallery-05-community.jpg',
     N'Activities',
     N'cộng đồng,hoạt động,Care4Kids,quan hệ',
     NULL, @SampleOrgId, @MaxId + 5, 0, GETDATE()),

    -- Education - School Building
    (@MaxId + 6,
     N'Ngôi trường khang trang',
     N'/assets/gallery/gallery-06-school-welcome.jpg',
     N'/assets/gallery/gallery-06-school-welcome.jpg',
     N'Education',
     N'trường học,kiến trúc,cơ sở vật chất,giáo dục',
     NULL, @SampleOrgId, @MaxId + 6, 1, GETDATE()),

    -- Education - Life Skills Classroom
    (@MaxId + 7,
     N'Lớp học kỹ năng sống cho trẻ em',
     N'/assets/gallery/gallery-07-life-skills.jpg',
     N'/assets/gallery/gallery-07-life-skills.jpg',
     N'Education',
     N'kỹ năng sống,lớp học,trẻ em,phát triển',
     NULL, @SampleOrgId, @MaxId + 7, 0, GETDATE()),

    -- Activities - Children's Day
    (@MaxId + 8,
     N'Ngày hội thiếu nhi Care4Kids',
     N'/assets/gallery/gallery-08-children-day.webp',
     N'/assets/gallery/gallery-08-children-day.webp',
     N'Activities',
     N'thiếu nhi,ngày hội,sự kiện,vui chơi',
     NULL, @SampleOrgId, @MaxId + 8, 1, GETDATE()),

    -- Volunteers - Active Volunteers
    (@MaxId + 9,
     N'Tình nguyện viên năng động',
     N'/assets/gallery/gallery-09-active-volunteers.jpg',
     N'/assets/gallery/gallery-09-active-volunteers.jpg',
     N'Volunteers',
     N'tình nguyện viên,năng động,tích cực,sáng tạo',
     NULL, @SampleOrgId, @MaxId + 9, 0, GETDATE()),

    -- Education - School Friends
    (@MaxId + 10,
     N'Bạn bè cùng trường Care4Kids',
     N'/assets/gallery/gallery-10-school-friends.jpg',
     N'/assets/gallery/gallery-10-school-friends.jpg',
     N'Education',
     N'bạn bè,trường học,tình bạn,Care4Kids',
     NULL, @SampleOrgId, @MaxId + 10, 0, GETDATE());

SET IDENTITY_INSERT dbo.Gallery OFF;

PRINT 'Gallery seeded 10 new items successfully.';
GO
