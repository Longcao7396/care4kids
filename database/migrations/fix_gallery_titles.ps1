$connStr = "Server=localhost\SQLEXPRESS;Database=GiveAIDDB;Integrated Security=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()

# Vietnamese titles via [char]0xCÓDES so PowerShell never sees double-encoded
# bytes from the source file. Each entry: gallery_id -> title.

function V([int]$id, [string]$t) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE dbo.Gallery SET title = @t WHERE gallery_id = @id"
    $p1 = $cmd.Parameters.Add("@t", [System.Data.SqlDbType]::NVarChar, 200); $p1.Value = $t
    $p2 = $cmd.Parameters.Add("@id", [System.Data.SqlDbType]::Int); $p2.Value = $id
    $cmd.ExecuteNonQuery() | Out-Null
}

# 5 KEPT images
V 3  ([char]0x48 + [char]0x1ecf + "c sinh Hà Giang say mê học tập")
V 19 ("Phiên chợ vùng cao và nụ cười trẻ thơ")
V 27 ("Cùng em đến trường qua cánh đồng")
V 35 ("Trẻ em bản làng Tuyên Quang")
V 29 ("Trẻ em Bắc Hà - tuổi thơ trên lưng trâu")

# Education
V 4  ("Lớp học vùng cao - nơi ước mơ bắt đầu")
V 5  ("Em nhỏ Hà Giang say mê đọc sách")
V 6  ("Em gái say sưa đọc sách giữa nắng chiều")
V 7  ("Lớp học tràn đầy năng lượng")
V 8  ("Góc học tập ấm áp của các em nhỏ")
V 9  ("Những nụ cười rạng rỡ giữa lớp học")
V 10 ("Hành trình tri thức từ vùng sâu vùng xa")
V 43 ("Em nhỏ say sưa đọc sách")
V 44 ("Lớp học tràn ngập nụ cười")
V 45 ("Những nụ cười hồn nhiên nơi vùng cao")
V 46 ("Hành trình tìm con chữ")

# Meals
V 11 ("Chia sẻ bữa cơm giữa rừng")
V 12 ("Phiên chợ quê - nguồn thực phẩm cho em nhỏ")
V 13 ("Bữa cơm gia đình vùng quê")
V 14 ("Xe trái cây trao yêu thương")
V 15 ("Mekong - nguồn sống của người dân")
V 54 ("Tình mẫu tử nơi cánh đồng")
V 55 ("Mekong - dòng sông nuôi sống bao em nhỏ")
V 56 ("Phiên chợ quê và những bữa cơm gia đình")
V 57 ("Trái cây quê hương - nguồn dinh dưỡng cho em nhỏ")

# Healthcare
V 16 ("Ánh sáng học đường nơi vùng sâu vùng xa")
V 17 ("Hành trình đến trường của các em vùng cao")
V 18 ("Những nụ cười trẻ thơ giữa lớp học")
V 20 ("Các em nhỏ trên đường đến trường")
V 50 ("Ánh đèn học tập giữa đêm vùng cao")
V 51 ("Trẻ em vùng cao trong lớp học")
V 52 ("Đường đến trường qua đèo cao")
V 53 ("Các em nhỏ vùng cao trên đường đến lớp")

# Children's Homes
V 21 ("Mái ấm tình thương - nơi trẻ mồ côi được yêu thương")
V 22 ("Em gái H'Mông Sapa giữa núi rừng")
V 23 ("Điểm trường ấm áp cho các em nhỏ")
V 24 ("Phiên chợ Lào Cai và em nhỏ dân tộc")
V 39 ("Trang trại trẻ em - mái nhà thứ hai")
V 40 ("Em gái H'Mông - tuổi thơ vùng cao")
V 41 ("Em nhỏ H'Mông Lào Cai - đôi mắt trong veo")
V 42 ("Em gái Hà Giang với đóa hoa vàng")

# Volunteers
V 25 ("Tình nguyện viên - cầu nối yêu thương")
V 26 ("Chuyến đi thiện nguyện đầy ý nghĩa")
V 28 ("Chung tay xây dựng tương lai cho trẻ em")
V 66 ("Tình nguyện viên trao yêu thương")
V 67 ("Hành trình thiện nguyện đến vùng cao")
V 68 ("Các em nhỏ trên đường đến trường")
V 69 ("Những nụ cười hạnh phúc của trẻ em")

# Events
V 30 ("Ngày hội trẻ em - nụ cười rạng rỡ")
V 31 ("Không khí lớp học tràn đầy năng lượng")
V 32 ("Hành trình tri thức của các em nhỏ")
V 33 ("Lớp học hạnh phúc giữa vùng cao")
V 34 ("Cộng đồng cùng chung tay giúp đỡ trẻ em")
V 47 ("Tiệc vui cho các em nhỏ vùng cao")
V 48 ("Không khí lớp học vùng cao")
V 49 ("Ngày hội trao yêu thương")

# Our Impact / School Supplies
V 36 ("Em nhỏ vùng cao - tuổi thơ bình dị")
V 37 ("Em gái H'Mông - vẻ đẹp vùng cao")
V 38 ("Các em nhỏ trên đường đến trường")
V 58 ("Tác động của chương trình - Những nụ cười hạnh phúc")
V 59 ("Câu chuyện thay đổi cuộc sống - Hành trình tri thức")
V 60 ("Những nụ cười hồn nhiên nơi vùng cao")
V 61 ("Lớp học hạnh phúc - tương lai tươi sáng")
V 62 ("Em nhỏ say sưa đọc sách - Thành quả giáo dục")
V 63 ("Lớp học tràn đầy năng lượng - Trẻ em vùng cao")
V 64 ("Hành trình tìm con chữ - Em nhỏ Hà Giang")
V 65 ("Hỗ trợ dụng cụ học tập - Ánh sáng học đường")

Write-Host "Updated titles for 67 rows."
$conn.Close()
