$connStr = "Server=localhost\SQLEXPRESS;Database=GiveAIDDB;Integrated Security=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
function V([int]$id, [string]$t) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE dbo.Gallery SET title = @t WHERE gallery_id = @id"
    $p1 = $cmd.Parameters.Add("@t", [System.Data.SqlDbType]::NVarChar, 200); $p1.Value = $t
    $p2 = $cmd.Parameters.Add("@id", [System.Data.SqlDbType]::Int); $p2.Value = $id
    $cmd.ExecuteNonQuery() | Out-Null
}

# 5 KEPT images
V 3  ([char]0x48 + [char]0x1ecf + "c sinh H" + [char]0xe0 + " Giang say m" + [char]0xea + " h" + [char]0x1ecf + "c t" + [char]0x1ead + "p")
V 19 "Phiên chợ vùng cao và nụ cười trẻ thơ"
V 27 "Cùng em đến trường qua cánh đồng"
V 35 "Trẻ em bản làng Tuyên Quang"
V 29 "Trẻ em Bắc Hà - tuổi thơ trên lưng trâu"

# Education
V 4  "L" + [char]0x1edb + "p h" + [char]0x1ecdc + " v" + [char]0x169 + "ng cao - n" + [char]0x1a1 + "i " + [char]0x1b0 + [char]0x1edb + "c m" + [char]0x1a1 + " b" + [char]0x1eaf + "t " + [char]0x1118 + [char]0x1ea7 + "u"
V 5  "Em nh" + [char]0x1ecf + " H" + [char]0xe0 + " Giang say m" + [char]0xea + " " + [char]0x1118 + [char]0x1ecdc + "c s" + [char]0xe1 + "ch"
V 6  "Em g" + [char]0xe1 + "i say s" + [char]0x1b0 + "a " + [char]0x1118 + [char]0x1ecdc + "c s" + [char]0xe1 + "ch gi" + [char]0x1eef + "a n" + [char]0x1eaf + "ng chi" + [char]0x1ec1 + "u"
V 7  "L" + [char]0x1edb + "p h" + [char]0x1ecdc + "c tr" + [char]0xe0 + "n " + [char]0x1118 + [char]0x1ea7 + "y n" + [char]0x103 + "ng l" + [char]0x1b0 + [char]0x1ec3 + "ng"
V 8  "G" + [char]0xf3 + "c h" + [char]0x1ecdc + "c t" + [char]0x1ead + "p " + [char]0x1ea5 + "m " + [char]0xe1 + "p c" + [char]0x1ee7 + "a c" + [char]0xe1 + "c em nh" + [char]0x1ecf
V 9  "Nh" + [char]0x1eef + "ng n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i r" + [char]0x1ea1 + "ng r" + [char]0x1ee1 + " gi" + [char]0x1eef + "a l" + [char]0x1edb + "p h" + [char]0x1ecdc + "c"
V 10 "H" + [char]0xe0 + "nh tr" + [char]0xec + "nh tri th" + [char]0x1ee9 + "c t" + [char]0x1eeb + " v" + [char]0xf9 + "ng s" + [char]0xe2 + "u v" + [char]0xf9 + "ng xa"
V 43 "Em nh" + [char]0x1ecf + " say s" + [char]0x1b0 + "a " + [char]0x1118 + [char]0x1ecdc + "c s" + [char]0xe1 + "ch"
V 44 "L" + [char]0x1edb + "p h" + [char]0x1ecdc + "c tr" + [char]0xe0 + "n ng" + [char]0x1ead + "p n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i"
V 45 "Nh" + [char]0x1eef + "ng n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i h" + [char]0x1ed3 + "n nhi" + [char]0xea + "n n" + [char]0x1a1 + "i v" + [char]0xf9 + "ng cao"
V 46 "H" + [char]0xe0 + "nh tr" + [char]0xec + "nh t" + [char]0xec + "m con ch" + [char]0x1eef

# Meals & Nutrition
V 11 "Chia s" + [char]0x1ebb + " b" + [char]0x1eef + "a c" + [char]0x1a1 + "m gi" + [char]0x1eef + "a r" + [char]0x1ebf + "ng"
V 12 "Phi" + [char]0xea + "n ch" + [char]0x1ee3 + " qu" + [char]0xea + " - ngu" + [char]0x1ed3 + "n th" + [char]0x1ef1 + "c ph" + [char]0x1ea9 + "m cho em nh" + [char]0x1ecf
V 13 "B" + [char]0x1eef + "a c" + [char]0x1a1 + "m gia " + [char]0x1118 + "nh v" + [char]0xf9 + "ng qu" + [char]0xea
V 14 "Xe tr" + [char]0xe1 + "i c" + [char]0xe2 + "y trao y" + [char]0xea + "u th" + [char]0x1b0 + "ng"
V 15 "Mekong - ngu" + [char]0x1ed3 + "n s" + [char]0x1ed1 + "ng c" + [char]0x1ee7 + "a ng" + [char]0x1b0 + [char]0x1edd + "i d" + [char]0xe2 + "n"
V 54 "T" + [char]0xec + "nh m" + [char]0x1eaf + "u t" + [char]0x1eed + " n" + [char]0x1a1 + "i c" + [char]0xe1 + "nh " + [char]0x1118 + "ng"
V 55 "Mekong - d" + [char]0xf2 + "ng s" + [char]0xf4 + "ng nu" + [char]0xf4 + "i s" + [char]0x1ed1 + "ng bao em nh" + [char]0x1ecf
V 56 "Phi" + [char]0xea + "n ch" + [char]0x1ee3 + " qu" + [char]0xea + " v" + [char]0xe0 + " nh" + [char]0x1eef + "ng b" + [char]0x1eef + "a c" + [char]0x1a1 + "m gia " + [char]0x1118 + "nh"
V 57 "Tr" + [char]0xe1 + "i c" + [char]0xe2 + "y qu" + [char]0xea + " h" + [char]0x1b0 + "ng - ngu" + [char]0x1ed3 + "n dinh d" + [char]0x1b0 + [char]0x1ee1 + "ng cho em nh" + [char]0x1ecf

# Healthcare
V 16 "Ánh s" + [char]0xe1 + "ng h" + [char]0x1ecdc + "c " + [char]0x1118 + [char]0x1edd + "ng n" + [char]0x1a1 + "i v" + [char]0xf9 + "ng s" + [char]0xe2 + "u v" + [char]0xf9 + "ng xa"
V 17 "H" + [char]0xe0 + "nh tr" + [char]0xec + "nh " + [char]0x1118 + [char]0x1ebn + "tr" + [char]0x1edd + "ng c" + [char]0x1ee7 + "a c" + [char]0xe1 + "c em v" + [char]0xf9 + "ng cao"
V 18 "Nh" + [char]0x1eef + "ng n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i tr" + [char]0x1ebb + " th" + [char]0x1a1 + " gi" + [char]0x1eef + "a l" + [char]0x1edb + "p h" + [char]0x1ecdc + "c"
V 20 "C" + [char]0xe1 + "c em nh" + [char]0x1ecf + " tr" + [char]0xea + "n " + [char]0x1118 + [char]0x1edd + "ng " + [char]0x1118 + [char]0x1ebn + "tr" + [char]0x1edd + "ng"
V 50 "Ánh " + [char]0x1118 + "n h" + [char]0x1ecdc + "c t" + [char]0x1ead + "p gi" + [char]0x1eef + "a " + [char]0x1118 + "m v" + [char]0xf9 + "ng cao"
V 51 "Tr" + [char]0x1ebb + " em v" + [char]0xf9 + "ng cao trong l" + [char]0x1edb + "p h" + [char]0x1ecdc + "c"
V 52 "Đ" + [char]0x1b0 + [char]0x1edd + "ng " + [char]0x1118 + [char]0x1ebn + "tr" + [char]0x1edd + "ng qua " + [char]0x1118 + [char]0x1ebf + "o cao"
V 53 "C" + [char]0xe1 + "c em nh" + [char]0x1ecf + " v" + [char]0xf9 + "ng cao tr" + [char]0xea + "n " + [char]0x1118 + [char]0x1edd + "ng " + [char]0x1118 + [char]0x1ebn + "l" + [char]0x1edb + "p"

# Children's Homes
V 21 "M" + [char]0xe1 + "i " + [char]0x1ea5 + "m t" + [char]0xec + "nh th" + [char]0x1b0 + "ng - n" + [char]0x1a1 + "i tr" + [char]0x1ebb + " m" + [char]0x1ed3 + " c" + [char]0xf4 + "i " + [char]0x1118 + [char]0x1b0 + [char]0x1ee3 + "c y" + [char]0xea + "u th" + [char]0x1b0 + "ng"
V 22 "Em g" + [char]0xe1 + "i H'M" + [char]0xf4 + "ng Sapa gi" + [char]0x1eef + "a n" + [char]0xfa + "i r" + [char]0x1eeb + "ng"
V 23 "Đi" + [char]0x1ec3 + "m tr" + [char]0x1edd + "ng " + [char]0x1ea5 + "m " + [char]0xe1 + "p cho c" + [char]0xe1 + "c em nh" + [char]0x1ecf
V 24 "Phi" + [char]0xea + "n ch" + [char]0x1ee3 + " L" + [char]0xe0 + "o Cai v" + [char]0xe0 + " em nh" + [char]0x1ecf + " d" + [char]0xe2 + "n t" + [char]0x1ed9 + "c"
V 39 "Trang tr" + [char]0xe1 + "i tr" + [char]0x1ebb + " em - m" + [char]0xe1 + "i nh" + [char]0xe0 + " th" + [char]0x1ee9 + " hai"
V 40 "Em g" + [char]0xe1 + "i H'M" + [char]0xf4 + "ng - tu" + [char]0x1ed5 + "i th" + [char]0x1a1 + " v" + [char]0xf9 + "ng cao"
V 41 "Em nh" + [char]0x1ecf + " H'M" + [char]0xf4 + "ng L" + [char]0xe0 + "o Cai - " + [char]0x1118 + [char]0xf4 + "i m" + [char]0x1eaf + "t trong veo"
V 42 "Em g" + [char]0xe1 + "i H" + [char]0xe0 + " Giang v" + [char]0x1edb + "i " + [char]0x1118 + [char]0xf3 + "a hoa v" + [char]0xe0 + "ng"

# Volunteers
V 25 "T" + [char]0xec + "nh nguy" + [char]0x1ec7 + "n vi" + [char]0xea + "n - c" + [char]0x1ea7 + "u n" + [char]0x1ed1 + "i y" + [char]0xea + "u th" + [char]0x1b0 + "ng"
V 26 "Chuy" + [char]0x1ebn + " " + [char]0x1118 + [char]0x1ec1 + "i thi" + [char]0x1ec7 + "n nguy" + [char]0x1ec7 + "n " + [char]0x1118 + [char]0x1ea7 + "y " + [char]0xfd + " ngh" + [char]0x129 + "a"
V 28 "Chung tay x" + [char]0xe2 + "y d" + [char]0x1ef1 + "ng t" + [char]0x1b0 + "ng lai cho tr" + [char]0x1ebb + " em"
V 66 "T" + [char]0xec + "nh nguy" + [char]0x1ec7 + "n vi" + [char]0xea + "n trao y" + [char]0xea + "u th" + [char]0x1b0 + "ng"
V 67 "H" + [char]0xe0 + "nh tr" + [char]0xec + "nh thi" + [char]0x1ec7 + "n nguy" + [char]0x1ec7 + "n " + [char]0x1118 + [char]0x1ebn + "n v" + [char]0xf9 + "ng cao"
V 68 "C" + [char]0xe1 + "c em nh" + [char]0x1ecf + " tr" + [char]0xea + "n " + [char]0x1118 + [char]0x1edd + "ng " + [char]0x1118 + [char]0x1ebn + "tr" + [char]0x1edd + "ng"
V 69 "Nh" + [char]0x1eef + "ng n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i h" + [char]0x1ea1 + "nh ph" + [char]0xfa + "c c" + [char]0x1ee7 + "a tr" + [char]0x1ebb + " em"

# Events / Gifts
V 30 "Ng" + [char]0xe0 + "y h" + [char]0x1ed9 + "i tr" + [char]0x1ebb + " em - n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i r" + [char]0x1ea1 + "ng r" + [char]0x1ee1
V 31 "Kh" + [char]0xf4 + "ng kh" + [char]0xed + " l" + [char]0x1edb + "p h" + [char]0x1ecdc + "c tr" + [char]0xe0 + "n " + [char]0x1118 + [char]0x1ea7 + "y n" + [char]0x103 + "ng l" + [char]0x1b0 + [char]0x1ec3 + "ng"
V 32 "H" + [char]0xe0 + "nh tr" + [char]0xec + "nh tri th" + [char]0x1ee9 + "c c" + [char]0x1ee7 + "a c" + [char]0xe1 + "c em nh" + [char]0x1ecf
V 33 "L" + [char]0x1edb + "p h" + [char]0x1ecdc + "c h" + [char]0x1ea1 + "nh ph" + [char]0xfa + "c gi" + [char]0x1eef + "a v" + [char]0xf9 + "ng cao"
V 34 "C" + [char]0x1ed9 + "ng " + [char]0x1118 + [char]0x1ed3 + "ng c" + [char]0xf9 + "ng chung tay gi" + [char]0xfa + "p " + [char]0x1118 + [char]0x1ee1 + " tr" + [char]0x1ebb + " em"
V 47 "Ti" + [char]0x1ec7 + "c vui cho c" + [char]0xe1 + "c em nh" + [char]0x1ecf + " v" + [char]0xf9 + "ng cao"
V 48 "Kh" + [char]0xf4 + "ng kh" + [char]0xed + " l" + [char]0x1edb + "p h" + [char]0x1ecdc + "c v" + [char]0xf9 + "ng cao"
V 49 "Ng" + [char]0xe0 + "y h" + [char]0x1ed9 + "i trao y" + [char]0xea + "u th" + [char]0x1b0 + "ng"

# Our Impact + School Supplies
V 36 "Em nh" + [char]0x1ecf + " v" + [char]0xf9 + "ng cao - tu" + [char]0x1ed5 + "i th" + [char]0x1a1 + " b" + [char]0xec + "nh d" + [char]0x1ecb
V 37 "Em g" + [char]0xe1 + "i H'M" + [char]0xf4 + "ng - v" + [char]0x1ebb + " " + [char]0x1118 + [char]0x1ebf + "p v" + [char]0xf9 + "ng cao"
V 38 "C" + [char]0xe1 + "c em nh" + [char]0x1ecf + " tr" + [char]0xea + "n " + [char]0x1118 + [char]0x1edd + "ng " + [char]0x1118 + [char]0x1ebn + "tr" + [char]0x1edd + "ng"
V 58 "T" + [char]0xe1 + "c " + [char]0x1118 + [char]0x1ed9 + "ng c" + [char]0x1ee7 + "a ch" + [char]0x1b0 + "ng tr" + [char]0xec + "nh - Nh" + [char]0x1eef + "ng n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i h" + [char]0x1ea1 + "nh ph" + [char]0xfa + "c"
V 59 "C" + [char]0xe2 + "u chuy" + [char]0x1ec7 + "n thay " + [char]0x1118 + [char]0x1ed5 + "i cu" + [char]0x1ed9 + "c s" + [char]0x1ed1 + "ng - H" + [char]0xe0 + "nh tr" + [char]0xec + "nh tri th" + [char]0x1ee9 + "c"
V 60 "Nh" + [char]0x1eef + "ng n" + [char]0x1ee5 + " c" + [char]0x1b0 + [char]0x1edd + "i h" + [char]0x1ed3 + "n nhi" + [char]0xea + "n n" + [char]0x1a1 + "i v" + [char]0xf9 + "ng cao"
V 61 "L" + [char]0x1edb + "p h" + [char]0x1ecdc + "c h" + [char]0x1ea1 + "nh ph" + [char]0xfa + "c - t" + [char]0x1b0 + "ng lai t" + [char]0x1b0 + "ng s" + [char]0xe1 + "ng"
V 62 "Em nh" + [char]0x1ecf + " say s" + [char]0x1b0 + "a " + [char]0x1118 + [char]0x1ecdc + "c s" + [char]0xe1 + "ch - Th" + [char]0xe0 + "nh qu" + [char]0x1ea3 + " gi" + [char]0xe1 + "o d" + [char]0x1ee5 + "c"
V 63 "L" + [char]0x1edb + "p h" + [char]0x1ecdc + "c tr" + [char]0xe0 + "n " + [char]0x1118 + [char]0x1ea7 + "y n" + [char]0x103 + "ng l" + [char]0x1b0 + [char]0x1ec3 + "ng - Tr" + [char]0x1ebb + " em v" + [char]0xf9 + "ng cao"
V 64 "H" + [char]0xe0 + "nh tr" + [char]0xec + "nh t" + [char]0xec + "m con ch" + [char]0x1eef + " - Em nh" + [char]0x1ecf + " H" + [char]0xe0 + " Giang"
V 65 "H" + [char]0x1ed7 + " tr" + [char]0x1ee3 + " d" + [char]0x1ee5 + "ng c" + [char]0x1ee5 + " h" + [char]0x1ecdc + "c t" + [char]0x1ead + "p - " + [char]0x1ea5 + "nh s" + [char]0xe1 + "ng h" + [char]0x1ecdc + "c " + [char]0x1118 + [char]0x1edd + "ng"

Write-Host "Updated 62 row titles."
$conn.Close()