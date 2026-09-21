-- =====================================================
-- GALLERY SEED — Add 30 more project images (batch 2)
-- Categories: Children's Homes, Education, Gifts & Events,
--             Healthcare, Meals & Nutrition, Our Impact,
--             School Supplies, Volunteers
-- =====================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_NAME() <> N'GiveAIDDB'
    THROW 50000, 'Run this migration against GiveAIDDB only.', 1;
GO

PRINT 'Seeding 30 more Gallery items (batch 2)...';

DECLARE @MaxId INT;
SELECT @MaxId = ISNULL(MAX(gallery_id), 0) FROM dbo.Gallery;

DECLARE @SampleOrgId INT = NULL;
SELECT TOP 1 @SampleOrgId = organization_id FROM dbo.Organizations;

SET IDENTITY_INSERT dbo.Gallery ON;

-- ============== CHILDREN'S HOMES (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 1, N'Trang trại trẻ em Care4Kids', '/assets/gallery/gallery-11-childrens-homes-01.jpg', '/assets/gallery/gallery-11-childrens-homes-01.jpg', N'Children''s Homes', N'trang trại,trẻ em,nhà tình thương,chăm sóc', NULL, @SampleOrgId, @MaxId + 1, 0, GETDATE()),
    (@MaxId + 2, N'Khu vui chơi cho trẻ em', '/assets/gallery/gallery-12-childrens-homes-02.jpg', '/assets/gallery/gallery-12-childrens-homes-02.jpg', N'Children''s Homes', N'vui chơi,trẻ em,khu vui chơi,gia đình', NULL, @SampleOrgId, @MaxId + 2, 0, GETDATE()),
    (@MaxId + 3, N'Cán bộ chăm sóc trẻ em', '/assets/gallery/gallery-13-childrens-homes-03.jpg', '/assets/gallery/gallery-13-childrens-homes-03.jpg', N'Children''s Homes', N'cán bộ,chăm sóc,trẻ em,tình nguyện', NULL, @SampleOrgId, @MaxId + 3, 0, GETDATE()),
    (@MaxId + 4, N'Gia đình Care4Kids', '/assets/gallery/gallery-14-childrens-homes-04.jpg', '/assets/gallery/gallery-14-childrens-homes-04.jpg', N'Children''s Homes', N'gia đình,trẻ em,chăm sóc,tình yêu', NULL, @SampleOrgId, @MaxId + 4, 0, GETDATE());

-- ============== EDUCATION (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 5, N'Lớp học Care4Kids', '/assets/gallery/gallery-15-education-01.jpg', '/assets/gallery/gallery-15-education-01.jpg', N'Education', N'lớp học,giáo dục,trẻ em,học tập', NULL, @SampleOrgId, @MaxId + 5, 1, GETDATE()),
    (@MaxId + 6, N'Giáo viên tình nguyện', '/assets/gallery/gallery-16-education-02.jpg', '/assets/gallery/gallery-16-education-02.jpg', N'Education', N'giáo viên,tình nguyện,giảng dạy,chia sẻ', NULL, @SampleOrgId, @MaxId + 6, 0, GETDATE()),
    (@MaxId + 7, N'Buổi học nhóm', '/assets/gallery/gallery-17-education-03.jpg', '/assets/gallery/gallery-17-education-03.jpg', N'Education', N'học nhóm,cộng tác,giáo dục,học sinh', NULL, @SampleOrgId, @MaxId + 7, 0, GETDATE()),
    (@MaxId + 8, N'Tài liệu học tập', '/assets/gallery/gallery-18-education-04.jpg', '/assets/gallery/gallery-18-education-04.jpg', N'Education', N'tài liệu,sách,vở,giáo dục', NULL, @SampleOrgId, @MaxId + 8, 0, GETDATE());

-- ============== GIFTS & EVENTS (3 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 9, N'Tặng quà trẻ em', '/assets/gallery/gallery-19-gifts-events-01.jpg', '/assets/gallery/gallery-19-gifts-events-01.jpg', N'Gifts & Events', N'tặng quà,quà tặng,trẻ em,quan tâm', NULL, @SampleOrgId, @MaxId + 9, 1, GETDATE()),
    (@MaxId + 10, N'Đội ngũ tổ chức sự kiện', '/assets/gallery/gallery-20-gifts-events-02.jpg', '/assets/gallery/gallery-20-gifts-events-02.jpg', N'Gifts & Events', N'tổ chức,sự kiện,đội nhóm,chương trình', NULL, @SampleOrgId, @MaxId + 10, 0, GETDATE()),
    (@MaxId + 11, N'Chiến dịch thiện nguyện', '/assets/gallery/gallery-21-gifts-events-03.jpg', '/assets/gallery/gallery-21-gifts-events-03.jpg', N'Gifts & Events', N'chiến dịch,thiện nguyện,tình nguyện,quan hệ', NULL, @SampleOrgId, @MaxId + 11, 0, GETDATE());

-- ============== HEALTHCARE (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 12, N'Chăm sóc sức khỏe cộng đồng', '/assets/gallery/gallery-22-healthcare-01.jpg', '/assets/gallery/gallery-22-healthcare-01.jpg', N'Healthcare', N'sức khỏe,y tế,chăm sóc,cộng đồng', NULL, @SampleOrgId, @MaxId + 12, 1, GETDATE()),
    (@MaxId + 13, N'Bác sĩ tình nguyện', '/assets/gallery/gallery-23-healthcare-02.jpg', '/assets/gallery/gallery-23-healthcare-02.jpg', N'Healthcare', N'bác sĩ,y tế,tình nguyện,khám bệnh', NULL, @SampleOrgId, @MaxId + 13, 0, GETDATE()),
    (@MaxId + 14, N'Nhóm chăm sóc sức khỏe', '/assets/gallery/gallery-24-healthcare-03.jpg', '/assets/gallery/gallery-24-healthcare-03.jpg', N'Healthcare', N'nhóm,y tế,chăm sóc,sức khỏe', NULL, @SampleOrgId, @MaxId + 14, 0, GETDATE()),
    (@MaxId + 15, N'Phòng khám di động', '/assets/gallery/gallery-25-healthcare-04.jpg', '/assets/gallery/gallery-25-healthcare-04.jpg', N'Healthcare', N'phòng khám,y tế,di động,cộng đồng', NULL, @SampleOrgId, @MaxId + 15, 0, GETDATE());

-- ============== MEALS & NUTRITION (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 16, N'Bếp ăn từ thiện', '/assets/gallery/gallery-26-meals-nutrition-01.jpg', '/assets/gallery/gallery-26-meals-nutrition-01.jpg', N'Meals & Nutrition', N'bếp ăn,từ thiện,thực phẩm,dinh dưỡng', NULL, @SampleOrgId, @MaxId + 16, 1, GETDATE()),
    (@MaxId + 17, N'Chương trình nuôi dưỡng', '/assets/gallery/gallery-27-meals-nutrition-02.jpg', '/assets/gallery/gallery-27-meals-nutrition-02.jpg', N'Meals & Nutrition', N'nuôi dưỡng,trẻ em,dinh dưỡng,sức khỏe', NULL, @SampleOrgId, @MaxId + 17, 0, GETDATE()),
    (@MaxId + 18, N'Phát thực phẩm cho người cần', '/assets/gallery/gallery-28-meals-nutrition-03.jpg', '/assets/gallery/gallery-28-meals-nutrition-03.jpg', N'Meals & Nutrition', N'thực phẩm,phát,người cần,quan tâm', NULL, @SampleOrgId, @MaxId + 18, 1, GETDATE()),
    (@MaxId + 19, N'Trang trại cung cấp thực phẩm', '/assets/gallery/gallery-29-meals-nutrition-04.webp', '/assets/gallery/gallery-29-meals-nutrition-04.webp', N'Meals & Nutrition', N'trang trại,nông nghiệp,thực phẩm,tự cung cấp', NULL, @SampleOrgId, @MaxId + 19, 0, GETDATE());

-- ============== OUR IMPACT (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 20, N'Tác động của Care4Kids', '/assets/gallery/gallery-30-our-impact-01.jpg', '/assets/gallery/gallery-30-our-impact-01.jpg', N'Our Impact', N'tác động,thành tựu,thay đổi,cuộc sống', NULL, @SampleOrgId, @MaxId + 20, 1, GETDATE()),
    (@MaxId + 21, N'Câu chuyện thay đổi cuộc sống', '/assets/gallery/gallery-31-our-impact-02.jpg', '/assets/gallery/gallery-31-our-impact-02.jpg', N'Our Impact', N'câu chuyện,thay đổi,cuộc sống,niềm tin', NULL, @SampleOrgId, @MaxId + 21, 0, GETDATE()),
    (@MaxId + 22, N'Những nụ cười hạnh phúc', '/assets/gallery/gallery-32-our-impact-03.jpg', '/assets/gallery/gallery-32-our-impact-03.jpg', N'Our Impact', N'nụ cười,hạnh phúc,trẻ em,tương lai', NULL, @SampleOrgId, @MaxId + 22, 1, GETDATE()),
    (@MaxId + 23, N'Cột mốc đạt được', '/assets/gallery/gallery-33-our-impact-04.jpg', '/assets/gallery/gallery-33-our-impact-04.jpg', N'Our Impact', N'cột mốc,đạt được,thành công,đóng góp', NULL, @SampleOrgId, @MaxId + 23, 0, GETDATE());

-- ============== SCHOOL SUPPLIES (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 24, N'Phát quà tặng học sinh', '/assets/gallery/gallery-34-school-supplies-01.jpg', '/assets/gallery/gallery-34-school-supplies-01.jpg', N'School Supplies', N'quà,tặng,học sinh,quan tâm', NULL, @SampleOrgId, @MaxId + 24, 1, GETDATE()),
    (@MaxId + 25, N'Các em học sinh nhận đồ dùng', '/assets/gallery/gallery-35-school-supplies-02.jpg', '/assets/gallery/gallery-35-school-supplies-02.jpg', N'School Supplies', N'học sinh,đồ dùng,nhận quà,niềm vui', NULL, @SampleOrgId, @MaxId + 25, 0, GETDATE()),
    (@MaxId + 26, N'Đội ngũ phát đồ dùng học tập', '/assets/gallery/gallery-36-school-supplies-03.jpg', '/assets/gallery/gallery-36-school-supplies-03.jpg', N'School Supplies', N'đội ngũ,phát đồ,tình nguyện,quan tâm', NULL, @SampleOrgId, @MaxId + 26, 0, GETDATE()),
    (@MaxId + 27, N'Hỗ trợ dụng cụ học tập', '/assets/gallery/gallery-37-school-supplies-04.jpg', '/assets/gallery/gallery-37-school-supplies-04.jpg', N'School Supplies', N'dụng cụ,học tập,hỗ trợ,giáo dục', NULL, @SampleOrgId, @MaxId + 27, 0, GETDATE());

-- ============== VOLUNTEERS (4 items) ==============
INSERT INTO dbo.Gallery (gallery_id, title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_at)
VALUES
    (@MaxId + 28, N'Tình nguyện viên Care4Kids', '/assets/gallery/gallery-38-volunteers-01.jpg', '/assets/gallery/gallery-38-volunteers-01.jpg', N'Volunteers', N'tình nguyện viên,Care4Kids,nhiệt huyết,đam mê', NULL, @SampleOrgId, @MaxId + 28, 1, GETDATE()),
    (@MaxId + 29, N'Đội nhóm thiện nguyện', '/assets/gallery/gallery-39-volunteers-02.jpg', '/assets/gallery/gallery-39-volunteers-02.jpg', N'Volunteers', N'đội nhóm,thiện nguyện,cùng nhau,hỗ trợ', NULL, @SampleOrgId, @MaxId + 29, 0, GETDATE()),
    (@MaxId + 30, N'Chia sẻ yêu thương cộng đồng', '/assets/gallery/gallery-40-volunteers-03.jpg', '/assets/gallery/gallery-40-volunteers-03.jpg', N'Volunteers', N'chia sẻ,yêu thương,tình cảm,quan tâm', NULL, @SampleOrgId, @MaxId + 30, 0, GETDATE()),
    (@MaxId + 31, N'Chiến dịch tình nguyện', '/assets/gallery/gallery-41-volunteers-04.jpg', '/assets/gallery/gallery-41-volunteers-04.jpg', N'Volunteers', N'chiến dịch,tình nguyện,hoạt động,thành công', NULL, @SampleOrgId, @MaxId + 31, 0, GETDATE());

SET IDENTITY_INSERT dbo.Gallery OFF;

PRINT 'Gallery seeded 30 more items (batch 2) across 8 categories.';
GO
