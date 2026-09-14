# 🌟 Care4Kids - Children Welfare & Donation Platform

**Version 2.0** - Full-stack campaign-based donation management system focused on children's welfare.

## 📋 Project Overview

**Care4Kids** (formerly Give-AID) is a comprehensive platform that enables NGOs to:
- ✅ Manage specific campaigns with goals and deadlines
- ✅ Accept and track donations with full transparency
- ✅ Generate detailed financial reports for completed campaigns
- ✅ Organize community programmes and events
- ✅ Admin dashboard with real-time statistics
- ✅ Role-based access control (User, Admin, SuperAdmin)

### 🎯 Core Focus

Campaigns for children welfare including:
- 📚 Education support (school supplies, books, computers)
- 🍲 Nutritional meals
- 🏥 Healthcare services
- 🎁 Gift programmes
- 🏠 Shelter support

---

## 🏗️ Architecture

### Technology Stack

**Frontend:**
- React 18.2
- React Router v6
- React Bootstrap 5
- Axios for API calls
- Context API for state management

**Backend:**
- ASP.NET MVC 5 / Web API 2
- Entity Framework 6 (Code First)
- SQL Server 2019+
- JWT Authentication
- BCrypt for password hashing
- RESTful API

**Database:**
- SQL Server with 16 optimized tables
- Foreign key relationships
- Indexed columns for performance
- JSON fields for flexible data

---

## 🆕 What's New in v2.0

### Major Changes

1. **Campaign System** 🎯
   - Specific campaigns with goals, timelines, and beneficiaries
   - Progress tracking with percentage and days remaining
   - Featured campaigns on homepage

2. **Admin Dashboard** 📊
   - Real-time statistics
   - Donations by month/campaign/cause
   - Top campaigns tracking
   - Recent users monitoring

3. **Campaign Reports** 📋
   - Transparent financial reporting
   - Total received vs. spent
   - Beneficiaries reached
   - Expense breakdown with photos

4. **Enhanced Donations** 💰
   - Link donations to specific campaigns
   - Payment status tracking (Pending, Completed, Failed)
   - Donation breakdown by amount ranges

5. **Improved UI/UX** 🎨
   - Modern design with teal/sky accents
   - Responsive campaign cards
   - Progress bars with animations
   - Admin-specific navigation

See [CHANGES_OVERVIEW.md](CHANGES_OVERVIEW.md) for detailed changes.

---

## 📁 Project Structure

```
project/
├── GiveAID.Client/              # React Frontend
│   ├── public/
│   ├── src/
│   │   ├── components/          # Reusable components
│   │   │   ├── Navbar.js
│   │   │   ├── Footer.js
│   │   ├── contexts/            # React Context
│   │   │   └── AuthContext.js
│   │   ├── pages/               # Page components
│   │   │   ├── HomePage.js
│   │   │   ├── LoginPage.js
│   │   │   ├── RegisterPage.js
│   │   │   ├── CampaignsPage.js          ← NEW
│   │   │   ├── CampaignDetailPage.js     ← NEW
│   │   │   ├── DonatePage.js
│   │   │   ├── ProgrammesPage.js
│   │   │   ├── DashboardPage.js
│   │   │   ├── admin/
│   │   │   │   └── AdminDashboard.js     ← NEW
│   │   │   └── ...
│   │   ├── services/            # API services
│   │   │   ├── api.js
│   │   │   └── index.js
│   │   ├── styles/              # CSS files
│   │   ├── config.js            # Configuration
│   │   ├── App.js
│   │   └── index.js
│   ├── package.json
│   └── README.md
│
├── GiveAID.Web/                 # ASP.NET Backend
│   ├── App_Start/
│   │   ├── WebApiConfig.cs
│   │   ├── RouteConfig.cs
│   │   └── FilterConfig.cs
│   ├── Controllers/             # API Controllers
│   │   ├── AuthController.cs
│   │   ├── CausesController.cs
│   │   ├── CampaignsController.cs        ← NEW
│   │   ├── AdminDashboardController.cs   ← NEW
│   │   ├── DonationsController.cs
│   │   └── ProgrammesController.cs
│   ├── Models/                  # Entity models
│   │   ├── EntityModels.cs      # Core entities
│   │   ├── ViewModels.cs        # DTOs
│   ├── Data/                    # DbContext
│   │   └── GiveAIDContext.cs
│   ├── Helpers/                 # Utilities
│   │   └── JwtHelper.cs
│   ├── Web.config
│   └── Global.asax.cs
│
├── Database/                    # SQL Scripts
│   ├── NGO_Database_Schema_V2.sql
│   └── NGO_Database_ERD.md
│
└── Documentation/               # Project docs
    ├── README.md                # This file
    ├── UPDATE_SUMMARY.md        # Detailed v2.0 changes
    ├── CHANGES_OVERVIEW.md      # Quick overview
    ├── PROJECT_SUMMARY.md       # Full project summary
    └── VERIFICATION_CHECKLIST.md
```

---

## 🗄️ Database Schema (V2.0)

### Core Tables (16 total)

1. **Users** - User accounts with RBAC (Role-Based Access Control)
2. **Organizations** - NGOs, Partners, and Supporters
3. **Causes** - Donation categories (Education, Health, Child Welfare)
4. **Campaigns** ← NEW - Specific campaigns with goals and timelines
5. **CampaignReports** ← NEW - Financial transparency reports
6. **Donations** - Donation transactions (now links to Campaigns)
7. **Programmes** - Events and activities
8. **ProgrammePhotos** - Programme image gallery
9. **ProgrammeRegistrations** - User registrations for programmes
10. **Conversations** - Support and inquiry conversations
11. **ConversationMessages** - Individual messages in conversations
12. **CmsPages** - Static content management
13. **Careers** - Job postings
14. **CareerApplications** - Job applications
15. **Gallery** - General image gallery
16. **ContactMessages** - Contact form submissions

### Campaign Structure

```
Causes (Category)
    ↓ has many
Campaigns (Specific campaign with goal/deadline)
    ↓ has many
Donations (Individual donations)
    ↓ after campaign ends
CampaignReports (Transparent financial report)
```

**Example:**
```
Cause: "Education"
  ├─ Campaign: "Cặp sách đến trường" (30M VNĐ, 500 kids, ends 30/09/2026)
  │   └─ Donations: 18.5M raised from 428 donors
  ├─ Campaign: "1000 bộ sách cho em" (50M VNĐ, 1000 kids)
  └─ Campaign: "Máy tính cho tương lai" (80M VNĐ, 200 kids)
```

### Key Features:
- ✅ RBAC with 3 roles: SuperAdmin, Admin, User
- ✅ Campaign-based donations with progress tracking
- ✅ Financial transparency with CampaignReports
- ✅ Payment status tracking (Pending, Completed, Failed, Refunded)
- ✅ Conversation system for support
- ✅ SEO-friendly CMS pages
- ✅ Comprehensive analytics for admins

---

## 🚀 Getting Started

### Prerequisites

- **Node.js** 16+ and npm
- **Visual Studio 2019/2022** or **VS Code** with C# extension
- **SQL Server 2019+** (Express/Developer/Standard)
- **.NET Framework 4.7.2+**
- **IIS Express** (included with Visual Studio)

### Installation Steps

#### 1. Clone/Navigate to Project

```bash
cd "C:\Users\admin\Desktop\project NGO"
```

#### 2. Setup Database

**Development/test only — reset database**

```sql
-- This reset deletes existing GiveAID data. Back up production data and use a migration instead.
-- 1. Create database
CREATE DATABASE GiveAIDDB;
GO

-- 2. Run schema creation script
-- Open and execute: NGO_Database_Schema_V2.sql
```

**Option B: Using Command Line (development/test only)**

```bash
sqlcmd -S .\SQLEXPRESS -E -d GiveAIDDB -i NGO_Database_Schema_V2.sql
```

#### 3. Configure Backend (ASP.NET)

**Update Connection String** in `GiveAID.Web\Web.config`:

```xml
<connectionStrings>
  <add name="GiveAIDContext" 
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=GiveAIDDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Install NuGet Packages:**

```bash
cd GiveAID.Web
# Restore packages using the repository NuGet.config
nuget restore GiveAID.Web.sln

# Build
```

**Required NuGet Packages:**
- EntityFramework 6.4.4
- Microsoft.AspNet.WebApi 5.2.9
- Microsoft.AspNet.WebApi.Cors 5.2.9
- Newtonsoft.Json 13.0.3
- System.IdentityModel.Tokens.Jwt 6.21.0

#### 4. Configure Frontend (React)

```bash
cd GiveAID.Client

# Install dependencies
npm install

# Bootstrap Icons are already declared in package.json.
```

**Update API URL** in `src/config.js` if needed:

```javascript
export const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:44300/api';
```

---

## ▶️ Running the Application

### Method 1: Visual Studio (Recommended)

1. **Start Backend:**
   - Open `GiveAID.Web\GiveAID.Web.sln` in Visual Studio
   - Press `F5` or click "IIS Express"
   - Backend will run at `https://localhost:44300`

2. **Start Frontend:**
   ```bash
   cd GiveAID.Client
   npm start
   ```
   - Frontend will run at `http://localhost:3000`

### Method 2: Command Line

**Terminal 1 - Backend:**
```bash
cd GiveAID.Web
# Build and run (requires IIS Express in PATH)
"C:\Program Files\IIS Express\iisexpress.exe" /path:C:\Users\admin\Desktop\project\GiveAID.Web /port:44300
```

**Terminal 2 - Frontend:**
```bash
cd GiveAID.Client
npm start
```

---

## 👤 Demo Accounts

### Admin Account
- **Email:** admin@give-aid.org
- **Password:** Admin@123
- **Role:** SuperAdmin

### Regular User Account
- **Email:** user@example.com
- **Password:** User@123
- **Role:** User

---

## 🎯 Key Features & Pages

### Public Pages
- ✅ **Home** (`/`) - Hero section, featured causes, featured campaigns, programmes
- ✅ **Causes** (`/causes`) - Browse all donation causes
- ✅ **Campaigns** (`/campaigns`) ← NEW - Active campaigns with progress tracking
- ✅ **Campaign Detail** (`/campaigns/:id`) ← NEW - Full campaign info + donate
- ✅ **Programmes** (`/programmes`) - View and filter programmes
- ✅ **Programme Detail** (`/programmes/:id`) - Programme info + registration
- ✅ **About** (`/about`) - Organization information
- ✅ **Contact** (`/contact`) - Contact form

### User Dashboard (Protected)
- ✅ **My Dashboard** (`/dashboard`) - User overview with stats
- ✅ **Donate** (`/donate`) - Donation form with campaign selection
- ✅ **My Donations** (`/my-donations`) - Donation history
- ✅ **My Registrations** (`/my-registrations`) - Programme registrations
- ✅ **Profile Settings** - Edit profile information

### Admin Dashboard (Admin Only)
- ✅ **Admin Dashboard** (`/admin`) ← NEW - Comprehensive statistics
  - Total donations, donors, campaigns stats
  - Donations by month (6-month chart)
  - Top campaigns table
  - Donations by cause breakdown
  - Recent users list

### Campaign Features (NEW in v2.0)
- ✅ Campaign cards with progress bars
- ✅ Days remaining countdown
- ✅ Donor count display
- ✅ Recent donations feed
- ✅ Donation breakdown by amount range
- ✅ Featured badge for priority campaigns
- ✅ Filter by cause and status
- ✅ Social media sharing buttons

---

## 📊 Admin Dashboard Metrics

The admin dashboard provides real-time insights:

### Overview Cards
```
┌────────────────────────────────────────────┐
│ 💰 Total Donations    👥 Total Donors     │
│    125,000,000 ₫           850            │
│                                            │
│ 📊 Active Campaigns   ✅ Completed        │
│         12                   8             │
└────────────────────────────────────────────┘
```

### Charts & Analytics
- **Donations by Month** - Line/bar chart for last 6 months
- **Top Campaigns** - Table with progress bars
- **Donations by Cause** - Pie/donut chart
- **Recent Users** - Latest registrations

Access: Only users with `Admin` or `SuperAdmin` role can view this page.

---

## 🗺️ Site Map

```
Homepage (/)
├── Causes (/causes)
├── Campaigns (/campaigns) ← NEW
│   └── Campaign Detail (/campaigns/:id) ← NEW
├── Programmes (/programmes)
│   └── Programme Detail (/programmes/:id)
├── About (/about)
├── Contact (/contact)
│
├── Auth
│   ├── Login (/login)
│   └── Register (/register)
│
├── User Dashboard (Protected)
│   ├── Dashboard (/dashboard)
│   ├── Donate (/donate)
│   ├── My Donations (/my-donations)
│   └── My Registrations (/my-registrations)
│
└── Admin (Admin Only)
    └── Dashboard (/admin) ← NEW
```
- ✅ **About** - Organization information
- ✅ **Contact** - Contact form

### Authenticated Pages
- ✅ **Login/Register** - User authentication
- ✅ **Donate** - Donation form with payment
- ✅ **Dashboard** - User overview
- ✅ **My Donations** - Donation history
- ✅ **My Registrations** - Programme participations

### Admin Pages (Coming Soon)
- 📋 Manage Causes
- 📋 Manage Programmes
- 📋 View Donations
- 📋 User Management
- 📋 Reports & Analytics

---

## 🔧 Configuration

### Frontend Environment Variables

Create `.env` file in `GiveAID.Client/`:

```env
REACT_APP_API_URL=https://localhost:44300/api
REACT_APP_NAME=Give-AID
```

### Backend App Settings

In `Web.config`:

```xml
<appSettings>
  <add key="JwtSecret" value="YourSuperSecretKey123!@#$%^&*()" />
  <add key="JwtIssuer" value="GiveAID" />
  <add key="JwtAudience" value="GiveAIDUsers" />
  <add key="JwtExpiryMinutes" value="1440" />
</appSettings>
```

---

## 📦 Build for Production

### Frontend Build

```bash
cd GiveAID.Client
npm run build
```

Output will be in `build/` folder. Deploy to:
- Netlify
- Vercel
- Azure Static Web Apps
- Or serve from IIS

### Backend Build

1. In Visual Studio: `Build > Publish...`
2. Choose target:
   - **IIS** (on-premise)
   - **Azure App Service**
   - **Folder** (for manual deployment)

---

## 🧪 Testing

### Frontend Tests

```bash
cd GiveAID.Client
npm test
```

### Backend Tests

```bash
# In Visual Studio
Test > Run All Tests
```

---

## 🐛 Troubleshooting

### CORS Issues

If you get CORS errors, check `Web.config`:

```xml
<system.webServer>
  <httpProtocol>
    <customHeaders>
      <add name="Access-Control-Allow-Origin" value="http://localhost:3000" />
      <add name="Access-Control-Allow-Methods" value="GET, POST, PUT, DELETE, OPTIONS" />
      <add name="Access-Control-Allow-Headers" value="Content-Type, Authorization" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```

### Database Connection Issues

1. Check SQL Server is running:
   ```bash
   # Open Services (services.msc)
   # Ensure "SQL Server (MSSQLSERVER)" is Running
   ```

2. Test connection string:
   ```bash
   sqlcmd -S localhost -E
   ```

3. Check firewall rules for SQL Server port (1433)

### Port Already in Use

```bash
# Frontend (3000)
npx kill-port 3000

# Backend (44300)
netstat -ano | findstr :44300
taskkill /PID <PID> /F
```

---

## 📚 API Documentation

### Base URL
```
https://localhost:44300/api
```

### Authentication Endpoints

**POST** `/api/auth/register`
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Pass@123",
  "fullName": "John Doe"
}
```

**POST** `/api/auth/login`
```json
{
  "email": "john@example.com",
  "password": "Pass@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGc...",
    "user": { "userId": 1, "username": "johndoe", "role": "User" }
  }
}
```

### Causes Endpoints

**GET** `/api/causes` - Get all causes

**GET** `/api/causes/{id}` - Get cause by ID

**POST** `/api/causes` - Create cause (Admin only)

**PUT** `/api/causes/{id}` - Update cause (Admin only)

**DELETE** `/api/causes/{id}` - Delete cause (Admin only)

### Donations Endpoints

**GET** `/api/donations` - Get user donations

**POST** `/api/donations` - Create donation
```json
{
  "causeId": 1,
  "amount": 100.00,
  "paymentMethod": "CreditCard",
  "cardHolderName": "John Doe",
  "cardNumber": "1234567890123456",
  "expiryDate": "12/25",
  "cvv": "123"
}
```

### Programmes Endpoints

**GET** `/api/programmes` - Get all programmes

**GET** `/api/programmes/{id}` - Get programme details

**POST** `/api/programmes/{id}/register` - Register for programme

---

## 🎨 Design System

### Color Palette

```css
/* Primary Colors */
--primary-dark: #0B0E14
--primary-navy: #0F172A
--primary-slate: #1E293B

/* Accent Colors */
--accent-sky: #38BDF8     /* Primary CTA */
--accent-cyan: #22D3EE    /* Hover states */
--accent-orange: #F97316  /* Secondary CTA */

/* Text Colors */
--text-light: #F8FAFC
--text-gray: #CBD5E1
```

### Typography

- **Font Family:** Inter (Google Fonts)
- **Headings:** 700-800 weight
- **Body:** 400-500 weight
- **Buttons:** 600-700 weight

---

## 📄 License

This project is licensed under the MIT License.

---

## 👥 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

## 📞 Support

For issues and questions:
- 📧 Email: support@give-aid.org
- 🐛 GitHub Issues: [Create an issue]
- 📖 Documentation: See `/docs` folder

---

## 🗺️ Roadmap

### Phase 1 (Current - MVP) ✅
- User authentication
- Donation system
- Programme management
- Basic dashboard

### Phase 2 (Coming Soon)
- Admin panel
- Report generation
- Email notifications
- Payment gateway integration

### Phase 3 (Future)
- Mobile app (React Native)
- Real-time notifications
- Volunteer management
- Advanced analytics

---

## 🙏 Acknowledgments

- Bootstrap team for UI components
- React team for the framework
- Entity Framework team
- All NGO partners and contributors

---

**Built with ❤️ for making a difference**
