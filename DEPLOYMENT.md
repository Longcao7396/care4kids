# 🌐 DEPLOYMENT GUIDE - Give-AID Platform

## Overview

Hướng dẫn này giúp bạn deploy Give-AID platform lên môi trường production.

---

## 🎯 Deployment Options

### Option 1: Azure (Recommended)
- **Backend:** Azure App Service
- **Frontend:** Azure Static Web Apps hoặc App Service
- **Database:** Azure SQL Database

### Option 2: Traditional Hosting
- **Backend:** IIS trên Windows Server
- **Frontend:** Netlify, Vercel, hoặc IIS
- **Database:** SQL Server 2019+

### Option 3: Hybrid
- **Backend:** IIS on-premise
- **Frontend:** CDN (Netlify/Vercel)
- **Database:** On-premise SQL Server

---

## 📦 Option 1: Deploy to Azure

### Step 1: Setup Azure SQL Database

```bash
# Create resource group
az group create --name GiveAID-RG --location eastus

# Create SQL Server
az sql server create \
  --name giveaid-sql-server \
  --resource-group GiveAID-RG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password YourSecurePassword123!

# Create database
az sql db create \
  --resource-group GiveAID-RG \
  --server giveaid-sql-server \
  --name GiveAIDDB \
  --service-objective S0

# Get connection string
az sql db show-connection-string \
  --client ado.net \
  --server giveaid-sql-server \
  --name GiveAIDDB
```

**Update Web.config:**
```xml
<connectionStrings>
  <add name="GiveAIDContext" 
       connectionString="Server=tcp:giveaid-sql-server.database.windows.net,1433;Initial Catalog=GiveAIDDB;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Step 2: Deploy Backend to Azure App Service

**Via Visual Studio:**

1. Right-click project → **Publish**
2. Target: **Azure**
3. Specific target: **Azure App Service (Windows)**
4. Create new App Service:
   - Name: `giveaid-api`
   - Subscription: Your subscription
   - Resource Group: `GiveAID-RG`
   - Hosting Plan: Create new (S1 or higher)
5. Click **Publish**

**Via Azure CLI:**

```bash
# Create App Service Plan
az appservice plan create \
  --name GiveAID-Plan \
  --resource-group GiveAID-RG \
  --sku S1

# Create Web App
az webapp create \
  --resource-group GiveAID-RG \
  --plan GiveAID-Plan \
  --name giveaid-api \
  --runtime "ASPNET|V4.7"

# Deploy from ZIP
cd GiveAID.Web
msbuild /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=Azure
az webapp deployment source config-zip \
  --resource-group GiveAID-RG \
  --name giveaid-api \
  --src publish.zip
```

**Configure CORS:**
```bash
az webapp cors add \
  --resource-group GiveAID-RG \
  --name giveaid-api \
  --allowed-origins "https://your-frontend-url.azurestaticapps.net"
```

### Step 3: Deploy Frontend to Azure Static Web Apps

```bash
# Build React app
cd GiveAID.Client
npm run build

# Create Static Web App
az staticwebapp create \
  --name giveaid-frontend \
  --resource-group GiveAID-RG \
  --source . \
  --location eastus \
  --branch main \
  --app-location "GiveAID.Client" \
  --output-location "build"
```

**Update config.js:**
```javascript
export const API_BASE_URL = 'https://giveaid-api.azurewebsites.net/api';
```

### Step 4: Configure Environment Variables in Azure

```bash
# Backend App Settings
az webapp config appsettings set \
  --resource-group GiveAID-RG \
  --name giveaid-api \
  --settings \
    JwtSecret="YourProductionSecretKey123456789012345678901234567890" \
    JwtIssuer="GiveAID" \
    JwtAudience="GiveAIDUsers" \
    JwtExpiryMinutes="1440"

# Frontend App Settings
az staticwebapp appsettings set \
  --name giveaid-frontend \
  --setting-names \
    REACT_APP_API_URL="https://giveaid-api.azurewebsites.net/api"
```

---

## 🖥️ Option 2: Deploy to IIS (On-Premise)

### Prerequisites
- Windows Server 2019+
- IIS 10+
- .NET Framework 4.7.2+
- SQL Server 2019+

### Step 1: Setup SQL Server

```sql
-- Create database
CREATE DATABASE GiveAIDDB;
GO

-- Run schema script
USE GiveAIDDB;
GO
-- Execute NGO_Database_Schema_V2.sql
```

### Step 2: Publish Backend

**Via Visual Studio:**

1. Right-click project → **Publish**
2. Target: **Folder**
3. Folder location: `C:\inetpub\wwwroot\GiveAID-API`
4. Configuration: **Release**
5. Target Framework: **net472**
6. Click **Publish**

**Configure IIS:**

```powershell
# Open IIS Manager
inetmgr

# Create Application Pool
New-WebAppPool -Name "GiveAID-Pool" -ManagedRuntimeVersion "v4.0"

# Create Website
New-Website -Name "GiveAID-API" `
  -PhysicalPath "C:\inetpub\wwwroot\GiveAID-API" `
  -ApplicationPool "GiveAID-Pool" `
  -Port 443 `
  -Ssl

# Bind SSL Certificate
# IIS Manager → Site → Bindings → Add HTTPS with SSL cert
```

**Update Web.config:**
```xml
<connectionStrings>
  <add name="GiveAIDContext" 
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=GiveAIDDB;Integrated Security=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

### Step 3: Deploy Frontend

**Option A: Host on IIS**

```bash
# Build
cd GiveAID.Client
npm run build

# Copy to IIS
xcopy /E /I build "C:\inetpub\wwwroot\GiveAID-Web"

# Create IIS site
New-Website -Name "GiveAID-Web" `
  -PhysicalPath "C:\inetpub\wwwroot\GiveAID-Web" `
  -ApplicationPool "DefaultAppPool" `
  -Port 80
```

**Option B: Use Node.js serve**

```bash
npm install -g serve
serve -s build -l 3000
```

---

## 🌐 Option 3: Deploy Frontend to Netlify/Vercel

### Netlify Deployment

```bash
# Install Netlify CLI
npm install -g netlify-cli

# Build
cd GiveAID.Client
npm run build

# Deploy
netlify deploy --prod --dir=build
```

**Create `netlify.toml`:**
```toml
[build]
  command = "npm run build"
  publish = "build"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

### Vercel Deployment

```bash
# Install Vercel CLI
npm install -g vercel

# Deploy
cd GiveAID.Client
vercel --prod
```

**Create `vercel.json`:**
```json
{
  "rewrites": [
    { "source": "/(.*)", "destination": "/index.html" }
  ]
}
```

---

## 🔒 Security Checklist

### Backend Security

- [ ] Change JWT Secret key in production
  ```xml
  <add key="JwtSecret" value="USE-STRONG-64-CHARACTER-KEY-HERE" />
  ```

- [ ] Enable HTTPS only
  ```xml
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="HTTPS Redirect" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="off" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
  ```

- [ ] Update CORS to production URL
- [ ] Enable SQL Server encryption
- [ ] Use strong database passwords
- [ ] Remove or secure appsettings in Web.config

### Frontend Security

- [ ] Update API URL to production
- [ ] Remove console.log statements
- [ ] Enable CSP headers
- [ ] Implement rate limiting
- [ ] Add security headers

---

## 📊 Post-Deployment Checklist

### Testing

- [ ] Test user registration
- [ ] Test login/logout
- [ ] Test donation flow
- [ ] Test programme registration
- [ ] Test admin features
- [ ] Test API endpoints
- [ ] Load testing
- [ ] Mobile responsiveness

### Monitoring

- [ ] Setup Application Insights (Azure)
- [ ] Configure error logging
- [ ] Setup email notifications
- [ ] Monitor database performance
- [ ] Setup backup strategy

### Optimization

- [ ] Enable CDN for static assets
- [ ] Enable gzip compression
- [ ] Minify JS/CSS
- [ ] Optimize images
- [ ] Enable browser caching
- [ ] Database indexing review

---

## 🔄 CI/CD Pipeline (Optional)

### GitHub Actions - Backend

Create `.github/workflows/backend.yml`:

```yaml
name: Deploy Backend

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Setup NuGet
      uses: NuGet/setup-nuget@v1
    
    - name: Restore NuGet packages
      run: nuget restore GiveAID.Web/GiveAID.Web.sln
    
    - name: Build
      run: msbuild GiveAID.Web/GiveAID.Web.sln /p:Configuration=Release
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'giveaid-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: 'GiveAID.Web/bin/Release'
```

### GitHub Actions - Frontend

Create `.github/workflows/frontend.yml`:

```yaml
name: Deploy Frontend

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup Node.js
      uses: actions/setup-node@v2
      with:
        node-version: '16'
    
    - name: Install dependencies
      run: |
        cd GiveAID.Client
        npm ci
    
    - name: Build
      run: |
        cd GiveAID.Client
        npm run build
      env:
        REACT_APP_API_URL: ${{ secrets.API_URL }}
    
    - name: Deploy to Netlify
      uses: netlify/actions/cli@master
      with:
        args: deploy --prod --dir=GiveAID.Client/build
      env:
        NETLIFY_SITE_ID: ${{ secrets.NETLIFY_SITE_ID }}
        NETLIFY_AUTH_TOKEN: ${{ secrets.NETLIFY_AUTH_TOKEN }}
```

---

## 🗄️ Database Backup Strategy

### Automated Backups (SQL Server)

```sql
-- Create maintenance plan for daily backups
USE master;
GO

-- Full backup daily at 2 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Daily_Full_Backup',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

-- Transaction log backup every hour
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'Hourly_Log_Backup',
    @freq_type = 4,
    @freq_interval = 1,
    @freq_subday_type = 8,
    @freq_subday_interval = 1;
```

### Azure SQL Automated Backups

```bash
# Configure retention period (7-35 days)
az sql db str-policy set \
  --resource-group GiveAID-RG \
  --server giveaid-sql-server \
  --name GiveAIDDB \
  --retention-days 14
```

---

## 📈 Performance Optimization

### IIS Configuration

```xml
<!-- Web.config -->
<system.webServer>
  <!-- Enable compression -->
  <urlCompression doStaticCompression="true" doDynamicCompression="true" />
  
  <!-- Enable output caching -->
  <caching>
    <profiles>
      <add extension=".jpg" policy="CacheUntilChange" varyByHeaders="User-Agent" />
      <add extension=".png" policy="CacheUntilChange" varyByHeaders="User-Agent" />
      <add extension=".js" policy="CacheUntilChange" varyByHeaders="User-Agent" />
      <add extension=".css" policy="CacheUntilChange" varyByHeaders="User-Agent" />
    </profiles>
  </caching>
  
  <!-- Set cache headers -->
  <staticContent>
    <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="7.00:00:00" />
  </staticContent>
</system.webServer>
```

### Database Optimization

```sql
-- Update statistics
EXEC sp_updatestats;

-- Rebuild indexes
ALTER INDEX ALL ON Users REBUILD;
ALTER INDEX ALL ON Donations REBUILD;
ALTER INDEX ALL ON Programmes REBUILD;

-- Update query execution plan
UPDATE STATISTICS Users WITH FULLSCAN;
UPDATE STATISTICS Donations WITH FULLSCAN;
```

---

## 🆘 Rollback Plan

### Quick Rollback Steps

1. **Database Rollback:**
   ```sql
   -- Restore from backup
   RESTORE DATABASE GiveAIDDB 
   FROM DISK = 'C:\Backups\GiveAIDDB_backup.bak'
   WITH REPLACE, RECOVERY;
   ```

2. **Backend Rollback:**
   ```bash
   # Redeploy previous version
   az webapp deployment slot swap \
     --resource-group GiveAID-RG \
     --name giveaid-api \
     --slot staging \
     --target-slot production
   ```

3. **Frontend Rollback:**
   ```bash
   # Netlify - rollback to previous deploy
   netlify rollback
   
   # Vercel
   vercel rollback
   ```

---

## 📞 Support Contacts

- **DevOps:** devops@give-aid.org
- **Database Admin:** dba@give-aid.org
- **Emergency:** +1-800-GIVEAID

---

**🎉 Deployment Complete! Monitor your application and enjoy!**
