-- =====================================================
-- GALLERY SEED — Merged migration file
-- Merged from 3 files:
--   - NGO_Database_Gallery_Migration.sql (12 items)
--   - NGO_Database_Gallery_ProjectImages_Migration.sql (10 items)
--   - NGO_Database_Gallery_Batch2_Migration.sql (31 items)
-- Total: 53 unique gallery items (IDs 1-53)
-- Idempotent: Safe to run multiple times
-- =====================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Chỉ chạy trên database đúng
IF DB_NAME() <> N'GiveAIDDB'
    THROW 50000, 'Run this migration against GiveAIDDB only.', 1;
GO

-- IDEMPOTENT GUARD: Nếu đã có data thì bỏ qua
IF EXISTS (SELECT 1 FROM dbo.Gallery)
BEGIN
    PRINT 'Gallery table already seeded — skipping.';
    RETURN;
END
GO

-- Lấy sample IDs nếu có
DECLARE @SampleProgrammeId INT = NULL;
SELECT TOP 1 @SampleProgrammeId = ProgrammeId FROM dbo.Programmes WHERE Status = 'Ongoing';

DECLARE @SampleOrgId INT = NULL;
SELECT TOP 1 @SampleOrgId = OrganizationId FROM dbo.Organizations WHERE IsActive = 1;

SET IDENTITY_INSERT dbo.Gallery ON;

PRINT 'Seeding Gallery items...';

-- =====================================================
-- SECTION 1: Original 12 items (IDs 1-12)
-- Source: NGO_Database_Gallery_Migration.sql
-- Categories: Education, Healthcare, Community, Events
-- Images: picsum.photos (royalty-free placeholders)
-- =====================================================
INSERT INTO dbo.Gallery
    (gallery_id, title, photo_url, thumbnail_url, category, tags,
     programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    -- Education
    (1,
     N'Children Learning Together',
     N'https://picsum.photos/seed/edu1/800/600',
     N'https://picsum.photos/seed/edu1/400/300',
     N'Education',
     N'children,learning,village,classroom',
     @SampleProgrammeId, @SampleOrgId, 1, 1, GETDATE()),

    (2,
     N'Teacher Workshop Session',
     N'https://picsum.photos/seed/edu2/800/600',
     N'https://picsum.photos/seed/edu2/400/300',
     N'Education',
     N'teachers,workshop,training,skills',
     @SampleProgrammeId, NULL, 2, 0, GETDATE()),

    (3,
     N'Books Distribution Day',
     N'https://picsum.photos/seed/edu3/800/600',
     N'https://picsum.photos/seed/edu3/400/300',
     N'Education',
     N'books,distribution,school,community',
     NULL, @SampleOrgId, 3, 0, GETDATE()),

    -- Healthcare
    (4,
     N'Mobile Health Clinic',
     N'https://picsum.photos/seed/health1/800/600',
     N'https://picsum.photos/seed/health1/400/300',
     N'Healthcare',
     N'clinic,health,mobile,rural',
     @SampleProgrammeId, NULL, 4, 1, GETDATE()),

    (5,
     N'Vaccination Drive',
     N'https://picsum.photos/seed/health2/800/600',
     N'https://picsum.photos/seed/health2/400/300',
     N'Healthcare',
     N'vaccination,children,prevention,community',
     NULL, @SampleOrgId, 5, 0, GETDATE()),

    (6,
     N'Health Check-up Camp',
     N'https://picsum.photos/seed/health3/800/600',
     N'https://picsum.photos/seed/health3/400/300',
     N'Healthcare',
     N'checkup,screening,medical,rural',
     @SampleProgrammeId, NULL, 6, 0, GETDATE()),

    -- Community
    (7,
     N'Community Cleanup Initiative',
     N'https://picsum.photos/seed/comm1/800/600',
     N'https://picsum.photos/seed/comm1/400/300',
     N'Community',
     N'cleanup,environment,volunteers,local',
     NULL, @SampleOrgId, 7, 0, GETDATE()),

    (8,
     N'Village Meeting',
     N'https://picsum.photos/seed/comm2/800/600',
     N'https://picsum.photos/seed/comm2/400/300',
     N'Community',
     N'village,meeting,leaders,engagement',
     @SampleProgrammeId, NULL, 8, 0, GETDATE()),

    (9,
     N'Women Empowerment Workshop',
     N'https://picsum.photos/seed/women1/800/600',
     N'https://picsum.photos/seed/women1/400/300',
     N'Community',
     N'women,empowerment,skills,workshop',
     @SampleProgrammeId, @SampleOrgId, 9, 1, GETDATE()),

    -- Events
    (10,
     N'Annual Fundraising Gala',
     N'https://picsum.photos/seed/event1/800/600',
     N'https://picsum.photos/seed/event1/400/300',
     N'Events',
     N'gala,fundraising,donors,annual',
     NULL, @SampleOrgId, 10, 1, GETDATE()),

    (11,
     N'Charity Walk 2025',
     N'https://picsum.photos/seed/event2/800/600',
     N'https://picsum.photos/seed/event2/400/300',
     N'Events',
     N'walk,charity,running,awareness',
     NULL, @SampleOrgId, 11, 0, GETDATE()),

    (12,
     N'Impact Awards Ceremony',
     N'https://picsum.photos/seed/event3/800/600',
     N'https://picsum.photos/seed/event3/400/300',
     N'Events',
     N'awards,impact,recognition,ceremony',
     NULL, @SampleOrgId, 12, 0, GETDATE());

-- =====================================================
-- SECTION 2: Project Images 10 items (IDs 13-22)
-- Source: NGO_Database_Gallery_ProjectImages_Migration.sql
-- Categories: Activities, Volunteers, Education
-- Images: /assets/gallery/gallery-01 to gallery-10
-- =====================================================
INSERT INTO dbo.Gallery
    (gallery_id, title, photo_url, thumbnail_url, category, tags,
     programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    -- Activities - Kids Sports
    (13,
     N'Trẻ em vui chơi thể thao',
     N'/assets/gallery/gallery-01-kids-sports.jpg',
     N'/assets/gallery/gallery-01-kids-sports.jpg',
     N'Activities',
     N'thiếu nhi,thể thao,vui chơi,năng động',
     NULL, @SampleOrgId, 13, 1, GETDATE()),

    -- Volunteers - Team Photo
    (14,
     N'Đội ngũ tình nguyện viên Care4Kids',
     N'/assets/gallery/gallery-02-volunteer-team.jpg',
     N'/assets/gallery/gallery-02-volunteer-team.jpg',
     N'Volunteers',
     N'tình nguyện viên,đội nhóm,Care4Kids,nhiệt huyết',
     NULL, @SampleOrgId, 14, 1, GETDATE()),

    -- Volunteers - Summer Campaign
    (15,
     N'Chiến dịch tình nguyện mùa hè 2024',
     N'/assets/gallery/gallery-03-summer-volunteer.webp',
     N'/assets/gallery/gallery-03-summer-volunteer.webp',
     N'Volunteers',
     N'mùa hè,chiến dịch,tình nguyện,hè 2024',
     NULL, @SampleOrgId, 15, 1, GETDATE()),

    -- Education - Anti-Bullying Program
    (16,
     N'Chương trình giáo dục phòng chống bắt nạt',
     N'/assets/gallery/gallery-04-anti-bullying.jpg',
     N'/assets/gallery/gallery-04-anti-bullying.jpg',
     N'Education',
     N'bắt nạt,giáo dục,phòng chống,trường học',
     NULL, @SampleOrgId, 16, 1, GETDATE()),

    -- Activities - Community Impact
    (17,
     N'Hoạt động cộng đồng Care4Kids',
     N'/assets/gallery/gallery-05-community.jpg',
     N'/assets/gallery/gallery-05-community.jpg',
     N'Activities',
     N'cộng đồng,hoạt động,Care4Kids,quan hệ',
     NULL, @SampleOrgId, 17, 0, GETDATE()),

    -- Education - School Building
    (18,
     N'Ngôi trường khang trang',
     N'/assets/gallery/gallery-06-school-welcome.jpg',
     N'/assets/gallery/gallery-06-school-welcome.jpg',
     N'Education',
     N'trường học,kiến trúc,cơ sở vật chất,giáo dục',
     NULL, @SampleOrgId, 18, 1, GETDATE()),

    -- Education - Life Skills Classroom
    (19,
     N'Lớp học kỹ năng sống cho trẻ em',
     N'/assets/gallery/gallery-07-life-skills.jpg',
     N'/assets/gallery/gallery-07-life-skills.jpg',
     N'Education',
     N'kỹ năng sống,lớp học,trẻ em,phát triển',
     NULL, @SampleOrgId, 19, 0, GETDATE()),

    -- Activities - Children's Day
    (20,
     N'Ngày hội thiếu nhi Care4Kids',
     N'/assets/gallery/gallery-08-children-day.webp',
     N'/assets/gallery/gallery-08-children-day.webp',
     N'Activities',
     N'thiếu nhi,ngày hội,sự kiện,vui chơi',
     NULL, @SampleOrgId, 20, 1, GETDATE()),

    -- Volunteers - Active Volunteers
    (21,
     N'Tình nguyện viên năng động',
     N'/assets/gallery/gallery-09-active-volunteers.jpg',
     N'/assets/gallery/gallery-09-active-volunteers.jpg',
     N'Volunteers',
     N'tình nguyện viên,năng động,tích cực,sáng tạo',
     NULL, @SampleOrgId, 21, 0, GETDATE()),

    -- Education - School Friends
    (22,
     N'Bạn bè cùng trường Care4Kids',
     N'/assets/gallery/gallery-10-school-friends.jpg',
     N'/assets/gallery/gallery-10-school-friends.jpg',
     N'Education',
     N'bạn bè,trường học,tình bạn,Care4Kids',
     NULL, @SampleOrgId, 22, 0, GETDATE());

-- =====================================================
-- SECTION 3: Batch 2 - 31 items (IDs 23-53)
-- Source: NGO_Database_Gallery_Batch2_Migration.sql
-- Categories: Children's Homes, Education, Gifts & Events,
--             Healthcare, Meals & Nutrition, Our Impact,
--             School Supplies, Volunteers
-- Images: /assets/gallery/gallery-11 to gallery-41
-- =====================================================
INSERT INTO dbo.Gallery
    (gallery_id, title, photo_url, thumbnail_url, category, tags,
     programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    -- ============== CHILDREN'S HOMES (4 items: 23-26) ==============
    (23,
     N'Trang trại trẻ em Care4Kids',
     N'/assets/gallery/gallery-11-childrens-homes-01.jpg',
     N'/assets/gallery/gallery-11-childrens-homes-01.jpg',
     N'Children''s Homes',
     N'trang trại,trẻ em,nhà tình thương,chăm sóc',
     NULL, @SampleOrgId, 23, 0, GETDATE()),

    (24,
     N'Khu vui chơi cho trẻ em',
     N'/assets/gallery/gallery-12-childrens-homes-02.jpg',
     N'/assets/gallery/gallery-12-childrens-homes-02.jpg',
     N'Children''s Homes',
     N'vui chơi,trẻ em,khu vui chơi,gia đình',
     NULL, @SampleOrgId, 24, 0, GETDATE()),

    (25,
     N'Cán bộ chăm sóc trẻ em',
     N'/assets/gallery/gallery-13-childrens-homes-03.jpg',
     N'/assets/gallery/gallery-13-childrens-homes-03.jpg',
     N'Children''s Homes',
     N'cán bộ,chăm sóc,trẻ em,tình nguyện',
     NULL, @SampleOrgId, 25, 0, GETDATE()),

    (26,
     N'Gia đình Care4Kids',
     N'/assets/gallery/gallery-14-childrens-homes-04.jpg',
     N'/assets/gallery/gallery-14-childrens-homes-04.jpg',
     N'Children''s Homes',
     N'gia đình,trẻ em,chăm sóc,tình yêu',
     NULL, @SampleOrgId, 26, 0, GETDATE()),

    -- ============== EDUCATION (4 items: 27-30) ==============
    (27,
     N'Lớp học Care4Kids',
     N'/assets/gallery/gallery-15-education-01.jpg',
     N'/assets/gallery/gallery-15-education-01.jpg',
     N'Education',
     N'lớp học,giáo dục,trẻ em,học tập',
     NULL, @SampleOrgId, 27, 1, GETDATE()),

    (28,
     N'Giáo viên tình nguyện',
     N'/assets/gallery/gallery-16-education-02.jpg',
     N'/assets/gallery/gallery-16-education-02.jpg',
     N'Education',
     N'giáo viên,tình nguyện,giảng dạy,chia sẻ',
     NULL, @SampleOrgId, 28, 0, GETDATE()),

    (29,
     N'Buổi học nhóm',
     N'/assets/gallery/gallery-17-education-03.jpg',
     N'/assets/gallery/gallery-17-education-03.jpg',
     N'Education',
     N'học nhóm,cộng tác,giáo dục,học sinh',
     NULL, @SampleOrgId, 29, 0, GETDATE()),

    (30,
     N'Tài liệu học tập',
     N'/assets/gallery/gallery-18-education-04.jpg',
     N'/assets/gallery/gallery-18-education-04.jpg',
     N'Education',
     N'tài liệu,sách,vở,giáo dục',
     NULL, @SampleOrgId, 30, 0, GETDATE()),

    -- ============== GIFTS & EVENTS (3 items: 31-33) ==============
    (31,
     N'Tặng quà trẻ em',
     N'/assets/gallery/gallery-19-gifts-events-01.jpg',
     N'/assets/gallery/gallery-19-gifts-events-01.jpg',
     N'Gifts & Events',
     N'tặng quà,quà tặng,trẻ em,quan tâm',
     NULL, @SampleOrgId, 31, 1, GETDATE()),

    (32,
     N'Đội ngũ tổ chức sự kiện',
     N'/assets/gallery/gallery-20-gifts-events-02.jpg',
     N'/assets/gallery/gallery-20-gifts-events-02.jpg',
     N'Gifts & Events',
     N'tổ chức,sự kiện,đội nhóm,chương trình',
     NULL, @SampleOrgId, 32, 0, GETDATE()),

    (33,
     N'Chiến dịch thiện nguyện',
     N'/assets/gallery/gallery-21-gifts-events-03.jpg',
     N'/assets/gallery/gallery-21-gifts-events-03.jpg',
     N'Gifts & Events',
     N'chiến dịch,thiện nguyện,tình nguyện,quan hệ',
     NULL, @SampleOrgId, 33, 0, GETDATE()),

    -- ============== HEALTHCARE (4 items: 34-37) ==============
    (34,
     N'Chăm sóc sức khỏe cộng đồng',
     N'/assets/gallery/gallery-22-healthcare-01.jpg',
     N'/assets/gallery/gallery-22-healthcare-01.jpg',
     N'Healthcare',
     N'sức khỏe,y tế,chăm sóc,cộng đồng',
     NULL, @SampleOrgId, 34, 1, GETDATE()),

    (35,
     N'Bác sĩ tình nguyện',
     N'/assets/gallery/gallery-23-healthcare-02.jpg',
     N'/assets/gallery/gallery-23-healthcare-02.jpg',
     N'Healthcare',
     N'bác sĩ,y tế,tình nguyện,khám bệnh',
     NULL, @SampleOrgId, 35, 0, GETDATE()),

    (36,
     N'Nhóm chăm sóc sức khỏe',
     N'/assets/gallery/gallery-24-healthcare-03.jpg',
     N'/assets/gallery/gallery-24-healthcare-03.jpg',
     N'Healthcare',
     N'nhóm,y tế,chăm sóc,sức khỏe',
     NULL, @SampleOrgId, 36, 0, GETDATE()),

    (37,
     N'Phòng khám di động',
     N'/assets/gallery/gallery-25-healthcare-04.jpg',
     N'/assets/gallery/gallery-25-healthcare-04.jpg',
     N'Healthcare',
     N'phòng khám,y tế,di động,cộng đồng',
     NULL, @SampleOrgId, 37, 0, GETDATE()),

    -- ============== MEALS & NUTRITION (4 items: 38-41) ==============
    (38,
     N'Bếp ăn từ thiện',
     N'/assets/gallery/gallery-26-meals-nutrition-01.jpg',
     N'/assets/gallery/gallery-26-meals-nutrition-01.jpg',
     N'Meals & Nutrition',
     N'bếp ăn,từ thiện,thực phẩm,dinh dưỡng',
     NULL, @SampleOrgId, 38, 1, GETDATE()),

    (39,
     N'Chương trình nuôi dưỡng',
     N'/assets/gallery/gallery-27-meals-nutrition-02.jpg',
     N'/assets/gallery/gallery-27-meals-nutrition-02.jpg',
     N'Meals & Nutrition',
     N'nuôi dưỡng,trẻ em,dinh dưỡng,sức khỏe',
     NULL, @SampleOrgId, 39, 0, GETDATE()),

    (40,
     N'Phát thực phẩm cho người cần',
     N'/assets/gallery/gallery-28-meals-nutrition-03.jpg',
     N'/assets/gallery/gallery-28-meals-nutrition-03.jpg',
     N'Meals & Nutrition',
     N'thực phẩm,phát,người cần,quan tâm',
     NULL, @SampleOrgId, 40, 1, GETDATE()),

    (41,
     N'Trang trại cung cấp thực phẩm',
     N'/assets/gallery/gallery-29-meals-nutrition-04.webp',
     N'/assets/gallery/gallery-29-meals-nutrition-04.webp',
     N'Meals & Nutrition',
     N'trang trại,nông nghiệp,thực phẩm,tự cung cấp',
     NULL, @SampleOrgId, 41, 0, GETDATE()),

    -- ============== OUR IMPACT (4 items: 42-45) ==============
    (42,
     N'Tác động của Care4Kids',
     N'/assets/gallery/gallery-30-our-impact-01.jpg',
     N'/assets/gallery/gallery-30-our-impact-01.jpg',
     N'Our Impact',
     N'tác động,thành tựu,thay đổi,cuộc sống',
     NULL, @SampleOrgId, 42, 1, GETDATE()),

    (43,
     N'Câu chuyện thay đổi cuộc sống',
     N'/assets/gallery/gallery-31-our-impact-02.jpg',
     N'/assets/gallery/gallery-31-our-impact-02.jpg',
     N'Our Impact',
     N'câu chuyện,thay đổi,cuộc sống,niềm tin',
     NULL, @SampleOrgId, 43, 0, GETDATE()),

    (44,
     N'Những nụ cười hạnh phúc',
     N'/assets/gallery/gallery-32-our-impact-03.jpg',
     N'/assets/gallery/gallery-32-our-impact-03.jpg',
     N'Our Impact',
     N'nụ cười,hạnh phúc,trẻ em,tương lai',
     NULL, @SampleOrgId, 44, 1, GETDATE()),

    (45,
     N'Cột mốc đạt được',
     N'/assets/gallery/gallery-33-our-impact-04.jpg',
     N'/assets/gallery/gallery-33-our-impact-04.jpg',
     N'Our Impact',
     N'cột mốc,đạt được,thành công,đóng góp',
     NULL, @SampleOrgId, 45, 0, GETDATE()),

    -- ============== SCHOOL SUPPLIES (4 items: 46-49) ==============
    (46,
     N'Phát quà tặng học sinh',
     N'/assets/gallery/gallery-34-school-supplies-01.jpg',
     N'/assets/gallery/gallery-34-school-supplies-01.jpg',
     N'School Supplies',
     N'quà,tặng,học sinh,quan tâm',
     NULL, @SampleOrgId, 46, 1, GETDATE()),

    (47,
     N'Các em học sinh nhận đồ dùng',
     N'/assets/gallery/gallery-35-school-supplies-02.jpg',
     N'/assets/gallery/gallery-35-school-supplies-02.jpg',
     N'School Supplies',
     N'học sinh,đồ dùng,nhận quà,niềm vui',
     NULL, @SampleOrgId, 47, 0, GETDATE()),

    (48,
     N'Đội ngũ phát đồ dùng học tập',
     N'/assets/gallery/gallery-36-school-supplies-03.jpg',
     N'/assets/gallery/gallery-36-school-supplies-03.jpg',
     N'School Supplies',
     N'đội ngũ,phát đồ,tình nguyện,quan tâm',
     NULL, @SampleOrgId, 48, 0, GETDATE()),

    (49,
     N'Hỗ trợ dụng cụ học tập',
     N'/assets/gallery/gallery-37-school-supplies-04.jpg',
     N'/assets/gallery/gallery-37-school-supplies-04.jpg',
     N'School Supplies',
     N'dụng cụ,học tập,hỗ trợ,giáo dục',
     NULL, @SampleOrgId, 49, 0, GETDATE()),

    -- ============== VOLUNTEERS (4 items: 50-53) ==============
    (50,
     N'Tình nguyện viên Care4Kids',
     N'/assets/gallery/gallery-38-volunteers-01.jpg',
     N'/assets/gallery/gallery-38-volunteers-01.jpg',
     N'Volunteers',
     N'tình nguyện viên,Care4Kids,nhiệt huyết,đam mê',
     NULL, @SampleOrgId, 50, 1, GETDATE()),

    (51,
     N'Đội nhóm thiện nguyện',
     N'/assets/gallery/gallery-39-volunteers-02.jpg',
     N'/assets/gallery/gallery-39-volunteers-02.jpg',
     N'Volunteers',
     N'đội nhóm,thiện nguyện,cùng nhau,hỗ trợ',
     NULL, @SampleOrgId, 51, 0, GETDATE()),

    (52,
     N'Chia sẻ yêu thương cộng đồng',
     N'/assets/gallery/gallery-40-volunteers-03.jpg',
     N'/assets/gallery/gallery-40-volunteers-03.jpg',
     N'Volunteers',
     N'chia sẻ,yêu thương,tình cảm,quan tâm',
     NULL, @SampleOrgId, 52, 0, GETDATE()),

    (53,
     N'Chiến dịch tình nguyện',
     N'/assets/gallery/gallery-41-volunteers-04.jpg',
     N'/assets/gallery/gallery-41-volunteers-04.jpg',
     N'Volunteers',
     N'chiến dịch,tình nguyện,hoạt động,thành công',
     NULL, @SampleOrgId, 53, 0, GETDATE());

SET IDENTITY_INSERT dbo.Gallery OFF;

-- =====================================================
-- CmsPages entry cho Gallery page (idempotent)
-- Source: NGO_Database_Gallery_Migration.sql
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM dbo.CmsPages WHERE page_key = 'gallery')
BEGIN
    INSERT INTO dbo.CmsPages (page_key, page_title, page_slug, content, display_order, is_in_menu)
    VALUES (
        'gallery',
        'Gallery',
        'gallery',
        N'<h2>Our Gallery</h2><p>Explore moments of impact, community, and change captured through our work. From education to healthcare, see how your support makes a real difference.</p>',
        8,
        1
    );
END
GO

PRINT 'Gallery seed migration completed. Total: 53 items across 12 categories.';
GO
