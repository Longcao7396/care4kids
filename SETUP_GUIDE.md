# 🚀 SETUP GUIDE - Care4Kids NGO Platform

> **Hướng dẫn cài đặt và chạy dự án từ A-Z cho người mới**  
> Ngay cả khi bạn chưa biết gì về lập trình, hãy làm theo từng bước và dự án sẽ chạy thành công.

---

## 📑 MỤC LỤC

1. [Tech Stack là gì?](#-1-tech-stack-là-gì)
2. [Những thứ cần cài đặt](#-2-những-thứ-cần-cài-đặt)
3. [Cài đặt từng phần mềm](#-3-cài-đặt-từng-phần-mềm)
4. [Cài đặt và chạy dự án](#-4-cài-đặt-và-chạy-dự-án)
5. [Tài khoản demo](#-5-tài-khoản-demo)
6. [Các lỗi thường gặp](#-6-các-lỗi-thường-gặp)
7. [Câu lệnh hữu ích](#-7-câu-lệnh-hữu-ích)

---

## 🎯 1. Tech Stack là gì?

**Tech Stack** = tập hợp các công nghệ được sử dụng để xây dựng dự án. Hãy hình dung như xây nhà:

| Thành phần | Công nghệ | Giải thích đơn giản | Ví dụ đời thường |
|---|---|---|---|
| **Frontend** (phần hiển thị trên web) | React 18 | Công nghệ tạo giao diện web | Giống phần sơn, cửa, nội thất |
| **UI Library** | React Bootstrap 5 + Bootstrap Icons | Bộ công cụ có sẵn nút, bảng, icon | Giống bộ đồ nội thất có sẵn |
| **Backend** (phần xử lý logic) | ASP.NET MVC 5 / Web API 2 (.NET Framework 4.7.2) | Nơi xử lý yêu cầu, tính toán | Giống hệ thống điện, nước trong nhà |
| **ORM** (cầu nối với DB) | Entity Framework 6 | Giúp backend nói chuyện với database | Phiên dịch viên |
| **Database** (nơi lưu dữ liệu) | SQL Server 2019+ | Nơi lưu user, donation, campaign | Kho chứa đồ |
| **Auth** (xác thực) | JWT (JSON Web Token) | Cơ chế đăng nhập | Chìa khóa và thẻ ra vào |
| **Build tool** | MSBuild + npm | Công cụ build code | Máy ép gạch |

### 📊 Sơ đồ tổng quan

```
┌─────────────────────────────────────────────────┐
│          Người dùng truy cập bằng Browser        │
└──────────────────────┬──────────────────────────┘
                       │
        ┌──────────────┴──────────────┐
        ▼                             ▼
┌──────────────────┐         ┌──────────────────────┐
│   Frontend       │         │   Backend            │
│   React 18       │  HTTP   │   ASP.NET Web API 2  │
│   Port: 3000     │ ──────► │   Port: 44300        │
│   (npm start)    │  JSON   │   (IIS Express)      │
└──────────────────┘         └──────────┬───────────┘
                                         │
                                         ▼
                              ┌──────────────────────┐
                              │   Database           │
                              │   SQL Server Express │
                              │   GiveAIDDB          │
                              └──────────────────────┘
```

---

## 🛠️ 2. Những thứ cần cài đặt

### ✅ Checklist phần mềm cần có

Đánh dấu ✅ vào từng mục khi đã cài xong:

| # | Phần mềm | Phiên bản | Bắt buộc? | Dung lượng |
|---|---|---|---|---|
| 1 | **Node.js** (đã bao gồm npm) | 18 LTS hoặc 20 LTS | ✅ Có | ~30 MB |
| 2 | **.NET Framework** | 4.7.2 | ✅ Có | ~70 MB |
| 3 | **SQL Server Express** | 2019 trở lên | ✅ Có | ~1.5 GB |
| 4 | **SQL Server Management Studio** (SSMS) | 2019 trở lên | ⚪ Tùy chọn | ~900 MB |
| 5 | **Visual Studio** HOẶC **MSBuild** | 2019/2022 | ✅ Có (chọn 1) | ~3-7 GB |
| 6 | **IIS Express** | 10+ | ✅ Có | ~30 MB |
| 7 | **Git** | Bản mới nhất | ⚪ Tùy chọn | ~50 MB |

> 💡 **Mẹo:** Visual Studio đã bao gồm IIS Express và MSBuild. Nếu cài VS thì không cần cài riêng 2 cái đó.

### 💻 Yêu cầu hệ thống

- **OS:** Windows 10/11 (64-bit)
- **RAM:** tối thiểu 8 GB (khuyến nghị 16 GB)
- **Ổ cứng:** trống ~10 GB
- **Quyền Admin:** bắt buộc (để cài SQL Server, IIS Express)

---

## 📥 3. Cài đặt từng phần mềm

### 📦 Bước 3.1 — Cài Node.js

1. Truy cập: <https://nodejs.org/en/download>
2. Tải bản **Windows Installer (.msi)** — chọn phiên bản **20 LTS** (khuyến nghị)
3. Chạy file cài đặt, nhấn **Next** liên tục, **đảm bảo tick vào "Add to PATH"**
4. Sau khi cài xong, mở **PowerShell** và kiểm tra:

```powershell
node -v
npm -v
```

> ✅ Kết quả mong đợi: `v20.x.x` và `10.x.x`

---

### 📦 Bước 3.2 — Cài .NET Framework 4.7.2

**Cách 1 — Qua Visual Studio Installer** (khuyến nghị):
- Khi cài VS, chọn workload **"ASP.NET and web development"** — VS sẽ tự cài .NET Framework.

**Cách 2 — Cài thủ công** (nếu không dùng VS):
1. Tải từ: <https://dotnet.microsoft.com/download/dotnet-framework/net472>
2. Chạy file `ndp472-kb4054530-x86-x64-allos-enu.exe`
3. Nhấn **Install** và đợi hoàn tất

**Kiểm tra:**
```powershell
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release
```
> ✅ Nếu `Release` ≥ `461808` là OK

---

### 📦 Bước 3.3 — Cài SQL Server Express

1. Truy cập: <https://www.microsoft.com/en-us/sql-server/sql-server-downloads>
2. Chọn **Express** (miễn phí)
3. Tải và chạy file `SQLEXPR_x64_ENU.exe`
4. Chọn **Custom** installation
5. Tick các feature:
   - ✅ **Database Engine Services**
   - ✅ **SQL Server Replication** (tùy chọn)
6. **Instance configuration:**
   - Chọn **Named instance:** `SQLEXPRESS`
   - Hoặc **Default instance** — tùy thích
7. **Server Configuration:**
   - **Authentication mode:** chọn **Mixed Mode** (khuyến nghị cho dev)
   - Đặt mật khẩu cho user `sa` (ghi nhớ mật khẩu này!)
8. Hoàn tất cài đặt

**Kiểm tra SQL Server đã chạy:**
```powershell
# Mở Services (services.msc) tìm "SQL Server (SQLEXPRESS)" → Status: Running
# Hoặc chạy lệnh:
Get-Service "MSSQL`$SQLEXPRESS"
```

> ✅ Kết quả mong đợi: `Status: Running`

---

### 📦 Bước 3.4 — Cài SQL Server Management Studio (SSMS) — Tùy chọn

SSMS giúp quản lý database trực quan (dễ hơn command line).

1. Tải từ: <https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms>
2. Chạy file `SSMS-Setup-ENU.exe`
3. Cài đặt mặc định

---

### 📦 Bước 3.5 — Cài Visual Studio HOẶC MSBuild

#### 🅰️ Cách A — Cài Visual Studio (khuyến nghị cho người mới)

1. Tải **Visual Studio Community** (miễn phí): <https://visualstudio.microsoft.com/downloads/>
2. Chạy installer, chọn workload:
   - ✅ **ASP.NET and web development**
   - ✅ **.NET desktop development** (tùy chọn)
   - ✅ **Data storage and processing** (tùy chọn)
3. Nhấn **Install** (mất 30-60 phút)

#### 🅱️ Cách B — Chỉ cài MSBuild (nhẹ hơn)

1. Tải **Build Tools for Visual Studio**: <https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio>
2. Trong installer, chọn:
   - ✅ **MSBuild**
   - ✅ **.NET Framework build tools**
   - ✅ **Windows 10/11 SDK**

**Kiểm tra MSBuild:**
```powershell
where.exe MSBuild
```
> ✅ Nếu ra đường dẫn như `C:\Program Files\Microsoft Visual Studio\...\MSBuild.exe` là OK

---

### 📦 Bước 3.6 — Cài IIS Express

**Nếu đã cài Visual Studio** → IIS Express đã có sẵn, **bỏ qua bước này**.

Nếu không cài VS:
1. Tải từ: <https://www.iis.net/downloads/microsoft-iis-express>
2. Cài đặt mặc định vào `C:\Program Files\IIS Express\`

**Kiểm tra:**
```powershell
Test-Path "C:\Program Files\IIS Express\iisexpress.exe"
```
> ✅ Kết quả: `True`

---

### 📦 Bước 3.7 — Cài Git (Tùy chọn)

Nếu cần clone dự án từ GitHub:
1. Tải từ: <https://git-scm.com/download/win>
2. Cài mặc định

---

## 🚀 4. Cài đặt và chạy dự án

### 📋 Tổng quan các bước

```
1. Mở project trong VS / folder
2. Cài đặt Database (chạy SQL scripts)
3. Cài đặt Backend dependencies (NuGet)
4. Cài đặt Frontend dependencies (npm)
5. Chạy Backend (IIS Express)
6. Chạy Frontend (React dev server)
7. Mở trình duyệt và xem
```

> ⏱️ **Thời gian ước tính:** 15-30 phút cho lần đầu tiên

---

### 🔧 Bước 4.1 — Mở project

Mở **PowerShell** tại thư mục gốc dự án:

```powershell
cd "C:\Users\admin\Desktop\project NGO"
```

> 💡 Nếu thư mục của bạn khác, thay đổi đường dẫn cho phù hợp.

---

### 🗄️ Bước 4.2 — Cài đặt Database

#### **Cách A — Dùng SSMS (dễ nhất, khuyến nghị cho người mới)**

1. Mở **SQL Server Management Studio**
2. Kết nối đến server:
   - **Server name:** `localhost\SQLEXPRESS` (hoặc `.\SQLEXPRESS`)
   - **Authentication:** Windows Authentication
3. Mở file `NGO_Database_Schema_V2.sql` (trong thư mục dự án)
   - Từ menu: **File → Open → File** → chọn file
4. Nhấn **Execute** (hoặc F5) để chạy script
5. Đợi vài giây, nếu thành công sẽ thấy: `Command(s) completed successfully.`
6. **(Tùy chọn)** Chạy tiếp các file seed:
   - `Campaigns_DataSeed.sql` — dữ liệu mẫu campaigns
   - `Campaigns_Fixup.sql` — bổ sung seed

#### **Cách B — Dùng command line (sqlcmd)**

```powershell
# 1. Tạo database
sqlcmd -S .\SQLEXPRESS -Q "IF DB_ID('GiveAIDDB') IS NULL CREATE DATABASE GiveAIDDB"

# 2. Chạy schema
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Schema_V2.sql"

# 3. Chạy các migration (theo thứ tự)
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\DB_Patch_Combined.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Causes_Restructure_Migration.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_CampaignProgramme_Merge.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Invitations_Migration.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\Campaigns_DataSeed.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\Campaigns_Fixup.sql"
```

**🔍 Kiểm tra database đã tạo:**

Mở SSMS → chọn database `GiveAIDDB` → mở rộng **Tables**. Bạn sẽ thấy ~16 bảng:

- `Users`, `Causes`, `Campaigns`, `CampaignReports`, `Donations`
- `Programmes`, `ProgrammeRegistrations`, `Gallery`
- `Conversations`, `ConversationMessages`, `CmsPages`
- `Careers`, `CareerApplications`, `ContactMessages`
- `TeamMembers`, `Achievements`, `Faqs`, `Invitations`
- `Organizations`

> ✅ Nếu thấy đủ các bảng → database OK!

---

### ⚙️ Bước 4.3 — Cấu hình connection string (nếu cần)

Mở file `GiveAID.Web\Web.config` bằng Notepad, kiểm tra dòng:

```xml
<connectionStrings>
  <add name="GiveAIDContext"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=GiveAIDDB;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=15"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Các tình huống cần sửa:**
- Nếu SQL Server của bạn tên khác `SQLEXPRESS` → đổi `.\SQLEXPRESS` thành tên instance của bạn.
- Nếu dùng SQL authentication → đổi thành:
  ```xml
  connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=GiveAIDDB;User ID=sa;Password=YOUR_PASSWORD;..."
  ```

---

### 📦 Bước 4.4 — Cài đặt Backend (.NET)

#### **Cách A — Visual Studio (dễ nhất)**

1. Mở file `GiveAID.Web\GiveAID.Web.sln` bằng Visual Studio
2. Visual Studio sẽ **tự động restore NuGet packages** khi mở project
3. Nếu không tự động, click phải vào Solution → **Restore NuGet Packages**
4. Nhấn **F5** để build + chạy (hoặc click nút ▶️ **IIS Express**)
5. IIS Express sẽ mở browser tại `https://localhost:44300`

#### **Cách B — Command line (không cần VS)**

```powershell
# 1. Restore NuGet packages
nuget restore ".\GiveAID.Web\GiveAID.Web.sln"

# 2. Tìm đường dẫn MSBuild
$msbuild = Get-ChildItem 'C:\Program Files' -Recurse -Filter MSBuild.exe |
  Where-Object { $_.FullName -match 'Visual Studio' } |
  Select-Object -First 1 -ExpandProperty FullName

# 3. Build project
& $msbuild ".\GiveAID.Web\GiveAID.Web.csproj" /p:Configuration=Debug /verbosity:minimal

# 4. Chạy bằng IIS Express
& "C:\Program Files\IIS Express\iisexpress.exe" /site:GiveAID.Web /userhome:"$env:USERPROFILE" /clr:4.0
```

---

### 📦 Bước 4.5 — Cài đặt Frontend (React)

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm install
```

> ⏱️ Quá trình này mất **3-10 phút** tùy tốc độ mạng.  
> 💡 Nếu lỗi, thử `npm install --legacy-peer-deps`

---

### 🎬 Bước 4.6 — Chạy cả Backend + Frontend (cùng lúc)

Đây là cách **dễ nhất và khuyến nghị nhất**:

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm start
```

Lệnh này sẽ tự động:
1. ✅ Kill các process cũ chiếm cổng 44300, 61508, 44301, 3000
2. ✅ Build backend (.NET) nếu cần
3. ✅ Khởi động IIS Express cho backend ở cổng **44300**
4. ✅ Khởi động React dev server ở cổng **3000**
5. ✅ Mở browser tại **http://localhost:3000**

**Kết quả trong PowerShell sẽ trông như:**

```
[start-backend] Checking ports 44300 / 61508 / 44301...
[start-backend] DLL up-to-date, skipping build
[start-backend] IIS Express started, PID 12345
[start-backend] Backend URLs: http://localhost:44300  http://localhost:61508
[start-backend] HEALTHY after 8s
[start-frontend] Checking port 3000...
[start-frontend] Starting React dev server on port 3000...
[start-frontend] react-scripts PID 67890
Compiled successfully!
You can now view giveaid-client in the browser.
  Local:            http://localhost:3000
  On Your Network:  http://192.168.1.6:3000
```

### 🎬 Cách chạy riêng (nếu muốn)

**Terminal 1 — Backend:**
```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run start:backend
```

**Terminal 2 — Frontend:**
```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run start:frontend
```

### 🛑 Dừng dự án

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run stop
```

Hoặc nhấn **Ctrl + C** trong terminal đang chạy `npm start`.

---

### ✅ Bước 4.7 — Kiểm tra dự án đã chạy

Mở **browser** (Chrome/Edge) và truy cập:

| URL | Mô tả |
|---|---|
| **http://localhost:3000** | Giao diện chính của dự án (React) |
| **http://localhost:44300** | Backend API (banner JSON) |
| **http://localhost:44300/api/health** | Health check API |
| **http://localhost:44300/api/causes** | Danh sách Causes |

**Test trong PowerShell:**
```powershell
# Backend health
curl http://localhost:44300/health
# Kết quả mong đợi: {"status":"healthy","timestamp":"..."}

# Causes
curl http://localhost:44300/api/causes
# Kết quả: JSON array of causes
```

> 🎉 **Nếu bạn thấy trang web hiển thị → Chúc mừng, dự án đã chạy thành công!**

---

## 👤 5. Tài khoản demo

Database mặc định đã có sẵn các tài khoản sau:

| Role | Email | Password | Quyền |
|---|---|---|---|
| **SuperAdmin** | `admin@give-aid.org` | `Admin@123` | Toàn quyền quản trị |
| **User** | `user@example.com` | `User@123` | Người dùng thường |

### Truy cập trang Admin

1. Đăng nhập với tài khoản SuperAdmin
2. Truy cập: <http://localhost:3000/admin>
3. Hoặc click vào avatar góc phải → **Admin Dashboard**

### Nếu quên mật khẩu Admin

Cách 1 — Reset database (đơn giản nhất):
```powershell
# Xóa và tạo lại database (mất hết dữ liệu cũ)
sqlcmd -S .\SQLEXPRESS -Q "USE master; ALTER DATABASE GiveAIDDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE GiveAIDDB; CREATE DATABASE GiveAIDDB"
# Chạy lại schema + seed
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Schema_V2.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\Campaigns_DataSeed.sql"
```

Cách 2 — Tự tạo lại admin qua API:
1. Đăng ký user mới qua `/register`
2. Vào database, chạy:
   ```sql
   UPDATE Users SET role = 'SuperAdmin' WHERE email = 'your-email@example.com';
   ```

---

## 🐛 6. Các lỗi thường gặp

### ❌ Lỗi 1: "Cannot connect to SQL Server"

**Triệu chứng:** Backend báo lỗi `Cannot open database "GiveAIDDB" requested by the login`

**Nguyên nhân:** SQL Server chưa chạy, hoặc tên instance sai

**Cách sửa:**
```powershell
# Kiểm tra SQL Server có chạy không
Get-Service "MSSQL`$SQLEXPRESS"

# Nếu Stopped, khởi động lại:
Start-Service "MSSQL`$SQLEXPRESS"

# Nếu tên instance khác, sửa Web.config:
# Data Source=.\TEN_INSTANCE_CUA_BAN
```

---

### ❌ Lỗi 2: "Port 44300 already in use"

**Triệu chứng:** Backend không khởi động được, lỗi port conflict

**Cách sửa 1 — Tự động (script sẽ tự kill):**
Script `start-backend.ps1` đã có sẵn logic tự động kill process chiếm port, chỉ cần chạy lại `npm start`.

**Cách sửa 2 — Thủ công:**
```powershell
# Tìm process chiếm cổng 44300
netstat -ano | findstr :44300
# → Lấy PID ở cột cuối

# Kill process
taskkill /PID <PID> /F
```

---

### ❌ Lỗi 3: "npm install" bị lỗi

**Triệu chứng:** Lỗi permission, EACCES, EPERM, hoặc timeout

**Cách sửa:**
```powershell
# 1. Chạy PowerShell với quyền Admin
# 2. Xóa cache npm
npm cache clean --force

# 3. Xóa node_modules cũ và cài lại
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
Remove-Item -Recurse -Force node_modules -ErrorAction SilentlyContinue
Remove-Item -Force package-lock.json -ErrorAction SilentlyContinue
npm install --legacy-peer-deps
```

---

### ❌ Lỗi 4: Frontend hiển thị "Network Error"

**Triệu chứng:** Browser mở trang React nhưng gọi API bị lỗi

**Cách sửa:**
```powershell
# 1. Kiểm tra backend có chạy không
curl http://localhost:44300/health

# 2. Kiểm tra proxy trong package.json
# Mở GiveAID.Client/package.json → dòng "proxy" phải là:
# "proxy": "http://localhost:44300"

# 3. Kiểm tra CORS trong Web.config (đã cấu hình sẵn cho localhost:3000)
```

---

### ❌ Lỗi 5: "MSBuild not found"

**Triệu chứng:** Backend không build được, lỗi "MSBuild.exe not found"

**Cách sửa:**
1. Cài **Build Tools for Visual Studio** (xem Bước 3.5 Cách B)
2. Hoặc tìm MSBuild thủ công:
   ```powershell
   Get-ChildItem 'C:\Program Files' -Recurse -Filter MSBuild.exe |
     Where-Object { $_.FullName -match 'Visual Studio' }
   ```

---

### ❌ Lỗi 6: "The build failed. Check backend.log"

**Triệu chứng:** Backend script báo build fail

**Cách sửa:**
```powershell
# Xem log chi tiết
Get-Content ".\GiveAID.Client\scripts\backend.log" -Tail 50

# Thường là lỗi thiếu file, dependency
# Xóa thư mục build cũ và build lại:
Remove-Item -Recurse -Force ".\GiveAID.Web\bin" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".\GiveAID.Web\obj" -ErrorAction SilentlyContinue
nuget restore ".\GiveAID.Web\GiveAID.Web.sln"
& $msbuild ".\GiveAID.Web\GiveAID.Web.csproj" /p:Configuration=Debug /verbosity:detailed
```

---

### ❌ Lỗi 7: "IIS Express not found"

**Triệu chứng:** `start-backend.ps1` báo IIS Express not found

**Cách sửa:**
- Nếu dùng Visual Studio → IIS Express đã có sẵn ở `C:\Program Files\IIS Express\`
- Nếu không có VS → cài IIS Express (xem Bước 3.6)

---

### ❌ Lỗi 8: Login báo "Invalid email or password" với tài khoản mặc định

**Cách sửa:** Database chưa được seed đúng. Chạy lại:
```powershell
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Schema_V2.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\Campaigns_DataSeed.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\Campaigns_Fixup.sql"
```

Hoặc reset toàn bộ DB:
```powershell
sqlcmd -S .\SQLEXPRESS -Q "USE master; DROP DATABASE GiveAIDDB; CREATE DATABASE GiveAIDDB"
# Chạy lại schema + seeds như Bước 4.2
```

---

## 🛠️ 7. Câu lệnh hữu ích

### 🚀 Chạy dự án

| Câu lệnh | Mô tả |
|---|---|
| `npm start` | Chạy cả backend + frontend |
| `npm run start:backend` | Chỉ chạy backend |
| `npm run start:frontend` | Chỉ chạy frontend |
| `npm run stop` | Dừng cả backend + frontend |
| `npm run stop:backend` | Chỉ dừng backend |
| `npm run stop:frontend` | Chỉ dừng frontend |
| `npm run build` | Build production frontend (output: `build/`) |

### 🗄️ Database

```powershell
# Mở SSMS
ssms

# Xem danh sách tables
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"

# Đếm số user
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -Q "SELECT COUNT(*) FROM Users"

# Reset admin password (set role SuperAdmin cho 1 user)
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -Q "UPDATE Users SET role='SuperAdmin' WHERE email='your@email.com'"
```

### 🔍 Kiểm tra hệ thống

```powershell
# Kiểm tra Node.js
node -v
npm -v

# Kiểm tra .NET Framework
$PSVersionTable.PSVersion
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release

# Kiểm tra SQL Server
sqlcmd -S .\SQLEXPRESS -Q "SELECT @@VERSION"

# Kiểm tra MSBuild
where.exe MSBuild

# Kiểm tra IIS Express
Test-Path "C:\Program Files\IIS Express\iisexpress.exe"

# Kiểm tra ports đang dùng
netstat -ano | findstr :44300
netstat -ano | findstr :3000
```

### 🧹 Dọn dẹp & Reset

```powershell
# Xóa cache webpack (giải phóng ~170 MB)
Remove-Item -Recurse -Force ".\GiveAID.Client\node_modules\.cache"

# Xóa build production (giải phóng ~5 MB)
Remove-Item -Recurse -Force ".\GiveAID.Client\build"

# Xóa build artifacts backend
Remove-Item -Recurse -Force ".\GiveAID.Web\bin"
Remove-Item -Recurse -Force ".\GiveAID.Web\obj"

# Force reinstall frontend
cd ".\GiveAID.Client"
Remove-Item -Recurse -Force node_modules
Remove-Item -Force package-lock.json
npm install

# Force restore backend NuGet
nuget restore ".\GiveAID.Web\GiveAID.Web.sln" -Force
```

---

## 📂 Cấu trúc dự án (tham khảo nhanh)

```
C:\Users\admin\Desktop\project NGO\
├── GiveAID.Client/          ← Frontend (React)
│   ├── public/              ← HTML tĩnh
│   ├── src/                 ← Source code React
│   │   ├── components/      ← Components tái sử dụng (Navbar, Footer...)
│   │   ├── pages/           ← Các trang (HomePage, CampaignsPage...)
│   │   ├── pages/admin/     ← Trang quản trị
│   │   ├── services/        ← API client (axios)
│   │   ├── contexts/        ← React Context (auth)
│   │   └── App.js           ← Root component + routing
│   ├── scripts/             ← PowerShell scripts (start, stop)
│   ├── package.json         ← Dependencies + scripts
│   └── node_modules/        ← (tự động tạo bởi npm install)
│
├── GiveAID.Web/             ← Backend (ASP.NET)
│   ├── Controllers/         ← API endpoints (Auth, Campaigns, Donations...)
│   ├── Models/              ← Entity + DTO classes
│   ├── Data/                ← DbContext (Entity Framework)
│   ├── Helpers/             ← JWT, Email, Sanitizer...
│   ├── Views/               ← (nếu có MVC views)
│   ├── Web.config           ← Cấu hình app + connection string
│   ├── GiveAID.Web.csproj   ← Project file (.NET)
│   ├── GiveAID.Web.sln      ← Solution file (Visual Studio)
│   ├── bin/                 ← Compiled DLLs (tự động)
│   ├── obj/                 ← Build cache (tự động)
│   └── packages/            ← NuGet local cache
│
├── database/                ← SQL scripts
│   └── seeds/               ← PowerShell scripts seed data
│
├── docs/                    ← Tài liệu nội bộ
│   ├── ARCHITECTURE.md      ← Kiến trúc hệ thống
│   ├── API_REFERENCE.md     ← Tài liệu API
│   ├── DATABASE.md          ← Schema database
│   └── RUNBOOK.md           ← Hướng dẫn vận hành (tương tự file này)
│
├── scripts/                 ← PowerShell scripts tiện ích
│
├── *.sql                    ← Database migration scripts
├── *.md                     ← Tài liệu markdown
├── README.md                ← Tổng quan dự án
├── QUICK_START.md           ← Hướng dẫn nhanh
└── package.json             ← (chỉ để chạy start scripts)
```

---

## 🌐 Các cổng (Ports) quan trọng

| Port | Service | URL |
|---|---|---|
| **3000** | React Dev Server | http://localhost:3000 |
| **44300** | Backend API (IIS Express) | http://localhost:44300 |
| **61508** | Backend API (fallback) | http://localhost:61508 |
| **1433** | SQL Server (default) | (chỉ truy cập nội bộ) |

Nếu đổi port, phải cập nhật:
- `GiveAID.Client/package.json` → `"proxy": "http://localhost:NEW_PORT"`
- `GiveAID.Web/.vs/GiveAID.Web/config/applicationhost.config`

---

## 🔗 Tài liệu tham khảo

| Tài liệu | Đường dẫn | Mô tả |
|---|---|---|
| **README chính** | `README.md` | Tổng quan dự án |
| **Quick Start** | `QUICK_START.md` | Hướng dẫn nhanh (10 phút) |
| **Runbook** | `docs/RUNBOOK.md` | Hướng dẫn vận hành chi tiết |
| **Architecture** | `docs/ARCHITECTURE.md` | Kiến trúc hệ thống |
| **API Reference** | `docs/API_REFERENCE.md` | Tài liệu API endpoints |
| **Database Schema** | `NGO_Database_V2_Documentation.md` | Cấu trúc database |
| **Deployment** | `DEPLOYMENT.md` | Hướng dẫn deploy production |

---

## 📞 Hỗ trợ

Nếu gặp lỗi không có trong file này:

1. **Kiểm tra log:**
   - Backend: `GiveAID.Client/scripts/backend.log`
   - Frontend: `GiveAID.Client/scripts/frontend.log`
   - IIS Express: `GiveAID.Web/backend.out.log` và `backend.err.log`

2. **Đọc tài liệu chi tiết:**
   - `docs/RUNBOOK.md` — Hướng dẫn vận hành có giải thích sâu hơn
   - `docs/ARCHITECTURE.md` — Hiểu kiến trúc trước khi sửa code

3. **Các bước debug cơ bản:**
   ```powershell
   # Test từng layer riêng biệt
   # 1. Test database
   sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -Q "SELECT 1"
   
   # 2. Test backend
   curl http://localhost:44300/health
   
   # 3. Test frontend (mở browser tới localhost:3000)
   Start-Process http://localhost:3000
   ```

---

## ✅ Checklist cuối cùng

Sau khi cài xong, đánh dấu các mục sau:

- [ ] Node.js đã cài (`node -v` thành công)
- [ ] .NET Framework 4.7.2 đã cài
- [ ] SQL Server Express đã cài và đang chạy
- [ ] Visual Studio HOẶC MSBuild đã cài
- [ ] IIS Express đã có (thường đi kèm VS)
- [ ] Database `GiveAIDDB` đã tạo và có 16 tables
- [ ] Đã `npm install` trong thư mục `GiveAID.Client`
- [ ] Đã restore NuGet packages trong `GiveAID.Web`
- [ ] `npm start` chạy thành công (không lỗi đỏ)
- [ ] Truy cập được http://localhost:3000 (thấy trang chủ)
- [ ] Đăng nhập được với `admin@give-aid.org` / `Admin@123`
- [ ] Truy cập được http://localhost:3000/admin

🎉 **Chúc mừng! Bạn đã cài đặt và chạy thành công dự án Care4Kids!**

---

**Tác giả:** Care4Kids Dev Team  
**Cập nhật lần cuối:** Tháng 9/2026  
**Phiên bản tài liệu:** 1.0
