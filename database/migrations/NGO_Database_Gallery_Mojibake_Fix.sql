-- ===================================================================
-- Gallery Mojibake Fix - Re-update titles with proper UTF-8
-- Generated: 2026-09-18
-- Purpose: Force-replace mojibake titles with proper Vietnamese Unicode
--          text. The previous migration used N'...' literals that were
--          stored correctly, but the data has somehow become double-encoded
--          (likely when it was initially imported with a wrong code page).
--          This script sets QUOTED_IDENTIFIER ON (required for tables
--          with indexed views) and runs the same UPDATE statements.
-- ===================================================================

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- Step 1: Update 5 KEPT images with correct Vietnamese titles.
UPDATE dbo.Gallery SET
    title = N'Học sinh Hà Giang say mê học tập',
    tags = N'Hà Giang,học sinh,dân tộc,điểm trường,giáo dục,vùng cao,trẻ em khó khăn'
WHERE gallery_id = 3;

UPDATE dbo.Gallery SET
    title = N'Phiên chợ vùng cao và nụ cười trẻ thơ',
    tags = N'chợ vùng cao,dân tộc,Lào Cai,trẻ em,nụ cười,đời thường'
WHERE gallery_id = 19;

UPDATE dbo.Gallery SET
    title = N'Cùng em đến trường qua cánh đồng',
    tags = N'đến trường,vùng cao,cánh đồng,Hà Giang,giáo dục,sinh viên,đường đi học'
WHERE gallery_id = 27;

UPDATE dbo.Gallery SET
    title = N'Trẻ em bản làng Tuyên Quang',
    tags = N'Tuyên Quang,bản làng,dân tộc,trẻ em,miền núi,hoàn cảnh khó khăn'
WHERE gallery_id = 35;

UPDATE dbo.Gallery SET
    title = N'Trẻ em Bắc Hà - tuổi thơ trên lưng trâu',
    tags = N'Bắc Hà,H''Mông,Lào Cai,trẻ em,chăn trâu,miền núi'
WHERE gallery_id = 29;
GO

-- Step 2: Education group (IDs: 4,5,6,7,8,9,10 + 43,44,45,46)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 4  THEN N'Lớp học vùng cao - nơi ước mơ bắt đầu'
        WHEN 5  THEN N'Em nhỏ Hà Giang say mê đọc sách'
        WHEN 6  THEN N'Em gái say sưa đọc sách giữa nắng chiều'
        WHEN 7  THEN N'Lớp học tràn đầy năng lượng'
        WHEN 8  THEN N'Góc học tập ấm áp của các em nhỏ'
        WHEN 9  THEN N'Những nụ cười rạng rỡ giữa lớp học'
        WHEN 10 THEN N'Hành trình tri thức từ vùng sâu vùng xa'
        WHEN 43 THEN N'Em nhỏ say sưa đọc sách'
        WHEN 44 THEN N'Lớp học tràn ngập nụ cười'
        WHEN 45 THEN N'Những nụ cười hồn nhiên nơi vùng cao'
        WHEN 46 THEN N'Hành trình tìm con chữ'
    END,
    tags = CASE gallery_id
        WHEN 4  THEN N'giáo dục,vùng cao,trẻ em,lớp học,ham học,sinh viên'
        WHEN 5  THEN N'Hà Giang,học sinh,dân tộc,đọc sách,giáo dục'
        WHEN 6  THEN N'học sinh,đọc sách,trẻ em,khát vọng,giáo dục'
        WHEN 7  THEN N'lớp học,vùng cao,trẻ em,năng động,giáo dục'
        WHEN 8  THEN N'góc học tập,sách,trẻ em,quyên góp,vùng cao'
        WHEN 9  THEN N'nụ cười,lớp học,trẻ em,vùng cao,niềm vui'
        WHEN 10 THEN N'hành trình,vùng sâu,con chữ,trẻ em,giáo dục'
        WHEN 43 THEN N'học sinh,đọc sách,trẻ em,khát vọng'
        WHEN 44 THEN N'lớp học,nụ cười,dân tộc,trẻ em'
        WHEN 45 THEN N'nụ cười,vùng cao,trẻ em,đáng yêu'
        WHEN 46 THEN N'giáo dục,trẻ em,vùng cao,con chữ'
    END,
    category = CASE
        WHEN gallery_id IN (43,44,45,46) THEN N'Education'
        ELSE category
    END
WHERE gallery_id IN (4,5,6,7,8,9,10,43,44,45,46);
GO

-- Step 3: Meals & Nutrition (IDs: 11,12,13,14,15 + 54,55,56,57)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 11 THEN N'Chia sẻ bữa cơm giữa rừng'
        WHEN 12 THEN N'Phiên chợ quê - nguồn thực phẩm cho em nhỏ'
        WHEN 13 THEN N'Bữa cơm gia đình vùng quê'
        WHEN 14 THEN N'Xe trái cây trao yêu thương'
        WHEN 15 THEN N'Mekong - nguồn sống của người dân'
        WHEN 54 THEN N'Tình mẫu tử nơi cánh đồng'
        WHEN 55 THEN N'Mekong - dòng sông nuôi sống bao em nhỏ'
        WHEN 56 THEN N'Phiên chợ quê và những bữa cơm gia đình'
        WHEN 57 THEN N'Trái cây quê hương - nguồn dinh dưỡng cho em nhỏ'
    END,
    tags = CASE gallery_id
        WHEN 11 THEN N'bữa ăn,chia sẻ,trẻ em,nông thôn,tình bạn'
        WHEN 12 THEN N'chợ quê,thực phẩm,gia đình,trẻ em,nghèo'
        WHEN 13 THEN N'gia đình,bữa cơm,nông thôn,trẻ em,tình yêu thương'
        WHEN 14 THEN N'trái cây,bán hàng rong,nông thôn,thực phẩm'
        WHEN 15 THEN N'Mekong,đồng bằng,ngư dân,trẻ em,cuộc sống'
        WHEN 54 THEN N'mẹ con,cánh đồng,nông dân,tình mẫu tử'
        WHEN 55 THEN N'Mekong,đồng bằng,sông nước,gia đình,trẻ em'
        WHEN 56 THEN N'chợ quê,thực phẩm,trẻ em,gia đình'
        WHEN 57 THEN N'trái cây,quê hương,dinh dưỡng,trẻ em'
    END,
    category = CASE
        WHEN gallery_id IN (54,55,56,57) THEN N'Meals & Nutrition'
        ELSE category
    END
WHERE gallery_id IN (11,12,13,14,15,54,55,56,57);
GO

-- Step 4: Healthcare (IDs: 16,17,18,20 + 50,51,52,53)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 16 THEN N'Ánh sáng học đường nơi vùng sâu vùng xa'
        WHEN 17 THEN N'Hành trình đến trường của các em vùng cao'
        WHEN 18 THEN N'Những nụ cười trẻ thơ giữa lớp học'
        WHEN 20 THEN N'Các em nhỏ trên đường đến trường'
        WHEN 50 THEN N'Ánh đèn học tập giữa đêm vùng cao'
        WHEN 51 THEN N'Trẻ em vùng cao trong lớp học'
        WHEN 52 THEN N'Đường đến trường qua đèo cao'
        WHEN 53 THEN N'Các em nhỏ vùng cao trên đường đến lớp'
    END,
    tags = CASE gallery_id
        WHEN 16 THEN N'giáo dục,ánh sáng,trẻ em,vùng cao,học đường'
        WHEN 17 THEN N'hành trình,đến trường,vùng cao,trẻ em,đèo dốc'
        WHEN 18 THEN N'nụ cười,lớp học,trẻ em,vùng cao,hồn nhiên'
        WHEN 20 THEN N'đến trường,nụ cười,trẻ em,niềm vui'
        WHEN 50 THEN N'ánh đèn,đêm vùng cao,trẻ em,khát vọng'
        WHEN 51 THEN N'lớp học,vùng cao,trẻ em,đơn sơ'
        WHEN 52 THEN N'đèo cao,đường trường,trẻ em,gian nan'
        WHEN 53 THEN N'vùng cao,đến lớp,trẻ em,hy vọng'
    END,
    category = CASE
        WHEN gallery_id IN (50,51,52,53) THEN N'Healthcare'
        ELSE category
    END
WHERE gallery_id IN (16,17,18,20,50,51,52,53);
GO

-- Step 5: Children's Homes / Shelter (IDs: 21,22,23,24 + 39,40,41,42)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 21 THEN N'Mái ấm tình thương - nơi trẻ mồ côi được yêu thương'
        WHEN 22 THEN N'Em gái H''Mông Sapa giữa núi rừng'
        WHEN 23 THEN N'Điểm trường ấm áp cho các em nhỏ'
        WHEN 24 THEN N'Phiên chợ Lào Cai và em nhỏ dân tộc'
        WHEN 39 THEN N'Trang trại trẻ em - mái nhà thứ hai'
        WHEN 40 THEN N'Em gái H''Mông - tuổi thơ vùng cao'
        WHEN 41 THEN N'Em nhỏ H''Mông Lào Cai - đôi mắt trong veo'
        WHEN 42 THEN N'Em gái Hà Giang với đóa hoa vàng'
    END,
    tags = CASE gallery_id
        WHEN 21 THEN N'mái ấm,trẻ mồ côi,tình thương,chăm sóc,hoàn cảnh khó khăn'
        WHEN 22 THEN N'H''Mông,Sapa,trẻ em,núi rừng,vùng cao,dân tộc'
        WHEN 23 THEN N'điểm trường,trẻ em,vùng cao,học tập'
        WHEN 24 THEN N'chợ Lào Cai,dân tộc,trẻ em,phiên chợ'
        WHEN 39 THEN N'trang trại trẻ em,mái nhà,hoàn cảnh khó khăn,chăm sóc'
        WHEN 40 THEN N'H''Mông,vùng cao,trẻ em,đôi mắt,núi rừng'
        WHEN 41 THEN N'H''Mông,Lào Cai,trẻ em,đôi mắt,hồn nhiên'
        WHEN 42 THEN N'Hà Giang,hoa vàng,em gái,vùng cao'
    END,
    category = CASE
        WHEN gallery_id IN (39,40,41,42) THEN N'Children''s Homes'
        ELSE category
    END
WHERE gallery_id IN (21,22,23,24,39,40,41,42);
GO

-- Step 6: Volunteers (IDs: 25,26,28 + 66,67,68,69)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 25 THEN N'Tình nguyện viên - cầu nối yêu thương'
        WHEN 26 THEN N'Chuyến đi thiện nguyện đầy ý nghĩa'
        WHEN 28 THEN N'Chung tay xây dựng tương lai cho trẻ em'
        WHEN 66 THEN N'Tình nguyện viên trao yêu thương'
        WHEN 67 THEN N'Hành trình thiện nguyện đến vùng cao'
        WHEN 68 THEN N'Các em nhỏ trên đường đến trường'
        WHEN 69 THEN N'Những nụ cười hạnh phúc của trẻ em'
    END,
    tags = CASE gallery_id
        WHEN 25 THEN N'tình nguyện viên,yêu thương,trẻ em,vùng cao'
        WHEN 26 THEN N'thiện nguyện,tình nguyện,trẻ em,vùng cao'
        WHEN 28 THEN N'tương lai,trẻ em,tình nguyện,vùng cao'
        WHEN 66 THEN N'tình nguyện viên,yêu thương,trẻ em'
        WHEN 67 THEN N'thiện nguyện,vùng cao,hành trình,trẻ em'
        WHEN 68 THEN N'đến trường,trẻ em,khát vọng'
        WHEN 69 THEN N'nụ cười,hạnh phúc,trẻ em,niềm vui'
    END,
    category = CASE
        WHEN gallery_id IN (66,67,68,69) THEN N'Volunteers'
        ELSE category
    END
WHERE gallery_id IN (25,26,28,66,67,68,69);
GO

-- Step 7: Events / Gifts (IDs: 30,31,32,33,34 + 47,48,49)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 30 THEN N'Ngày hội trẻ em - nụ cười rạng rỡ'
        WHEN 31 THEN N'Không khí lớp học tràn đầy năng lượng'
        WHEN 32 THEN N'Hành trình tri thức của các em nhỏ'
        WHEN 33 THEN N'Lớp học hạnh phúc giữa vùng cao'
        WHEN 34 THEN N'Cộng đồng cùng chung tay giúp đỡ trẻ em'
        WHEN 47 THEN N'Tiệc vui cho các em nhỏ vùng cao'
        WHEN 48 THEN N'Không khí lớp học vùng cao'
        WHEN 49 THEN N'Ngày hội trao yêu thương'
    END,
    tags = CASE gallery_id
        WHEN 30 THEN N'ngày hội,trẻ em,nụ cười,vùng cao,sự kiện'
        WHEN 31 THEN N'lớp học,năng lượng,trẻ em,vùng cao'
        WHEN 32 THEN N'tri thức,hành trình,trẻ em,khát vọng'
        WHEN 33 THEN N'lớp học,hạnh phúc,vùng cao,trẻ em'
        WHEN 34 THEN N'cộng đồng,chung tay,trẻ em,yêu thương'
        WHEN 47 THEN N'tiệc vui,trẻ em,vùng cao,quà tặng'
        WHEN 48 THEN N'lớp học,vùng cao,trẻ em'
        WHEN 49 THEN N'ngày hội,yêu thương,trẻ em,khó khăn'
    END,
    category = CASE
        WHEN gallery_id IN (47,48,49) THEN N'Gifts & Events'
        ELSE category
    END
WHERE gallery_id IN (30,31,32,33,34,47,48,49);
GO

-- Step 8: Our Impact + School Supplies + misc (IDs: 36,37,38 + 58,59,60,61,62,63,64,65)
UPDATE dbo.Gallery SET
    title = CASE gallery_id
        WHEN 36 THEN N'Em nhỏ vùng cao - tuổi thơ bình dị'
        WHEN 37 THEN N'Em gái H''Mông - vẻ đẹp vùng cao'
        WHEN 38 THEN N'Các em nhỏ trên đường đến trường'
        WHEN 58 THEN N'Tác động của chương trình - Những nụ cười hạnh phúc'
        WHEN 59 THEN N'Câu chuyện thay đổi cuộc sống - Hành trình tri thức'
        WHEN 60 THEN N'Những nụ cười hồn nhiên nơi vùng cao'
        WHEN 61 THEN N'Lớp học hạnh phúc - tương lai tươi sáng'
        WHEN 62 THEN N'Em nhỏ say sưa đọc sách - Thành quả giáo dục'
        WHEN 63 THEN N'Lớp học tràn đầy năng lượng - Trẻ em vùng cao'
        WHEN 64 THEN N'Hành trình tìm con chữ - Em nhỏ Hà Giang'
        WHEN 65 THEN N'Hỗ trợ dụng cụ học tập - Ánh sáng học đường'
    END,
    tags = CASE gallery_id
        WHEN 36 THEN N'trẻ em,vùng cao,tuổi thơ,bình dị'
        WHEN 37 THEN N'H''Mông,vẻ đẹp,Lào Cai,trẻ em,dân tộc'
        WHEN 38 THEN N'đến trường,trẻ em,khát vọng'
        WHEN 58 THEN N'tác động,hạnh phúc,trẻ em,chương trình'
        WHEN 59 THEN N'thay đổi,cuộc sống,tri thức,trẻ em'
        WHEN 60 THEN N'nụ cười,vùng cao,trẻ em,hồn nhiên'
        WHEN 61 THEN N'lớp học,hạnh phúc,tương lai,trẻ em'
        WHEN 62 THEN N'đọc sách,giáo dục,thành quả,trẻ em'
        WHEN 63 THEN N'lớp học,năng lượng,vùng cao,trẻ em'
        WHEN 64 THEN N'con chữ,Hà Giang,hành trình,trẻ em'
        WHEN 65 THEN N'dụng cụ học tập,ánh sáng,vùng cao'
    END,
    category = CASE
        WHEN gallery_id IN (58,59,60,61) THEN N'Our Impact'
        WHEN gallery_id IN (62,63,64,65) THEN N'School Supplies'
        ELSE category
    END
WHERE gallery_id IN (36,37,38,58,59,60,61,62,63,64,65);
GO

-- Verification: list any rows that still contain mojibake markers.
SELECT gallery_id, title
FROM dbo.Gallery
WHERE title LIKE N'%Há»%'
   OR title LIKE N'%Ã¡%'
   OR title LIKE N'%Ã©%'
   OR title LIKE N'%Ã¨%'
   OR title LIKE N'%Ã³%'
   OR title LIKE N'%Ã²%'
   OR title LIKE N'%Ã±%'
   OR title LIKE N'%Â%';
GO
