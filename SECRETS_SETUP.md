# Secrets Management Guide

## Overview

This document explains how to configure secrets for GiveAID v2.0 in development, staging, and production environments.

**NEVER commit secrets to source control.** Use environment variables or secret management services.

---

## Development (Local)

For local development, secrets are configured in `src/WebApi/appsettings.Development.json` (gitignored in production/staging variants only).

### Required Configuration

1. **JWT Secret** (already set in appsettings.Development.json)
   - 64-character random secret generated on 2026-09-28
   - Safe for local development only
   - Production MUST override this

2. **Admin/Demo Passwords** (set in START.bat)
   ```bat
   set "ADMIN_PASSWORD=DevAdmin@123"
   set "DEMO_PASSWORD=Demo@123"
   ```

3. **Database Connection String**
   - Already configured for `(localdb)\MSSQLLocalDB`
   - No secrets needed (Integrated Security)

4. **Email (Optional - Dev)**
   - `RequireVerification: false` in appsettings.Development.json
   - Use smtp4dev or Papercut for local testing:
     ```powershell
     # Install smtp4dev (fake SMTP server)
     dotnet tool install -g Rnwood.Smtp4dev
     smtp4dev
     # Opens web UI at http://localhost:5000
     ```
   - Update appsettings.Development.json:
     ```json
     "Smtp": {
       "SmtpEnabled": true,
       "SmtpHost": "localhost",
       "SmtpPort": 25,
       "SmtpUsername": "",
       "SmtpPassword": "",
       "FromEmail": "dev@giveaid.local",
       "FromName": "GiveAID Dev"
     }
     ```

5. **Cloudinary (Optional - Dev)**
   - Sign up at https://cloudinary.com (free tier: 25 credits/month)
   - Set environment variables:
     ```powershell
     $env:CLOUDINARY_CLOUD_NAME="your_cloud_name"
     $env:CLOUDINARY_API_KEY="your_api_key"
     $env:CLOUDINARY_API_SECRET="your_api_secret"
     ```

---

## Production

### Required Environment Variables

Set these before running the application:

```bash
# JWT (REQUIRED - min 32 chars)
Jwt__Secret="<64-char-random-secret-generate-new>"

# Database (REQUIRED)
ConnectionStrings__DefaultConnection="Server=<prod-server>;Database=GiveAIDDB;User Id=<user>;Password=<pwd>;Encrypt=True;TrustServerCertificate=False"

# Seed Passwords (REQUIRED for first run)
ADMIN_PASSWORD="<strong-password-min-8-chars>"
DEMO_PASSWORD="<strong-password-min-8-chars>"

# Email (REQUIRED)
Smtp__SmtpEnabled=true
Smtp__SmtpHost="smtp.gmail.com"
Smtp__SmtpPort=587
Smtp__SmtpUsername="<email>"
Smtp__SmtpPassword="<app-password>"
Smtp__FromEmail="no-reply@yourdomain.com"
Smtp__FromName="GiveAID"

# Cloudinary (REQUIRED for image uploads)
CLOUDINARY_CLOUD_NAME="<your-cloud-name>"
CLOUDINARY_API_KEY="<your-api-key>"
CLOUDINARY_API_SECRET="<your-api-secret>"

# Stripe (if using real payments)
PaymentGateway__Type="Stripe"
PaymentGateway__StripeSecretKey="sk_live_..."
PaymentGateway__StripeWebhookSecret="whsec_..."

# CORS (REQUIRED)
Cors__AllowedOrigins="https://yourdomain.com,https://www.yourdomain.com"
```

### Security Checklist

- [ ] JWT secret is at least 32 characters (64+ recommended)
- [ ] JWT secret is NOT the dev secret from appsettings.Development.json
- [ ] Connection string uses encrypted connection (`Encrypt=True`)
- [ ] Connection string does NOT use `TrustServerCertificate=True`
- [ ] ADMIN_PASSWORD is strong (12+ chars, mixed case, numbers, symbols)
- [ ] SMTP credentials use app-specific passwords (not main account password)
- [ ] Cloudinary API secret is rotated if ever committed to git
- [ ] Stripe uses live keys (sk_live_..., not test keys)
- [ ] CORS origins list ONLY includes production domains (no wildcards, no localhost)
- [ ] SeedData__Enabled is set to false after first deployment

### Generate New JWT Secret

```powershell
# PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

```bash
# Linux/Mac (with openssl)
openssl rand -base64 48
```

---

## Azure App Service

Set environment variables in **Configuration → Application Settings**:

1. Add each variable as a new application setting
2. Use double underscores for nested config (e.g., `Jwt__Secret`)
3. Mark sensitive settings as "Slot Setting" to prevent accidental overwrites

Alternative: Use **Azure Key Vault** and reference secrets:
```
@Microsoft.KeyVault(SecretUri=https://<vault-name>.vault.azure.net/secrets/<secret-name>/)
```

---

## Docker

Pass environment variables via docker run:

```bash
docker run -e Jwt__Secret="..." \
           -e ConnectionStrings__DefaultConnection="..." \
           -e ADMIN_PASSWORD="..." \
           -p 5000:8080 \
           giveaid:latest
```

Or use a `.env` file (DO NOT commit this):

```bash
docker run --env-file .env.production -p 5000:8080 giveaid:latest
```

---

## Validation

The application validates secrets at startup:

- **JWT Secret**: Throws exception if empty or < 32 chars in production
- **Admin Password**: Throws exception if empty or < 8 chars when seeding
- **Connection String**: Throws exception if empty

Test your production configuration locally:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:Jwt__Secret="<your-prod-secret>"
$env:ConnectionStrings__DefaultConnection="<your-prod-connection>"
# ... set all required vars ...
dotnet run --project src/WebApi
```

Expected: Application starts without throwing `InvalidOperationException`.

---

## Rotating Secrets

If a secret is compromised:

1. **JWT Secret**: Generate new secret → update all environments → restart app → all users must re-login
2. **Database Password**: Update in Azure SQL → update connection string → restart app
3. **Cloudinary**: Rotate in Cloudinary dashboard → update env vars → restart app
4. **Stripe**: Rotate in Stripe dashboard → update webhook secret → restart app

---

## M-06 Fix: JWT Secret Length Enforcement

**Issue**: Previous JWT secret was only 16 chars (weak).

**Fix Applied**:
- Dev secret is now 64 chars (base64 random)
- Production enforces minimum 32 chars at startup (see `Program.cs:48`)
- Old 16-char secret should be considered compromised if ever pushed to GitHub

**Action Required**:
- Production deployment MUST use a NEW 64-char secret (generate fresh, do not reuse dev secret)
