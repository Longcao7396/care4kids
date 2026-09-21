# SMTP Setup Guide — Care4Kids Email Delivery

This document explains how to configure the Care4Kids backend (`GiveAID.Web`) to send real emails via SMTP. It covers common providers, security best practices, and local development tools.

---

## Table of Contents

1. [How the Email System Works](#1-how-the-email-system-works)
2. [Web.config Settings Reference](#2-webconfig-settings-reference)
3. [Gmail SMTP](#3-gmail-smtp)
4. [SendGrid](#4-sendgrid)
5. [Mailgun](#5-mailgun)
6. [Local Development: smtp4dev](#6-local-development-smtp4dev)
7. [Local Development: MailHog](#7-local-development-mailhog)
8. [Security: Never Commit Secrets](#8-security-never-commit-secrets)
9. [Troubleshooting](#9-troubleshooting)
10. [Testing the Setup](#10-testing-the-setup)

---

## 1. How the Email System Works

The `EmailService` class (`GiveAID.Web/Helpers/EmailService.cs`) acts as a single gateway for all outbound emails:

| Setting | Behaviour |
|---------|-----------|
| `SmtpEnabled=false` (default) | Mock mode — every send is **logged** to the `EmailLogs` DB table with `Status=MockSent`. Nothing is transmitted over the network. |
| `SmtpEnabled=true` | Real SMTP delivery. Every attempt is **logged** before sending, and the status is updated to `Sent` or `Failed` after. |

The `EmailLogs` table gives you full audibility regardless of which mode is active — you can always verify what emails were (or would have been) sent from the **Admin → Email Logs** page (`/admin/emails`).

---

## 2. Web.config Settings Reference

All settings live in `<appSettings>` in `GiveAID.Web/Web.config`:

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `SmtpEnabled` | Yes | `false` | Set to `true` to enable real SMTP delivery. |
| `SmtpHost` | Yes | `localhost` | SMTP relay hostname. |
| `SmtpPort` | Yes | `587` | SMTP submission port. |
| `SmtpUseSsl` | No | `true` | `true` = implicit TLS (port 465); `false` = STARTTLS (port 587). |
| `SmtpUsername` | For auth | *(empty)* | Username or `apikey` (SendGrid). |
| `SmtpPassword` | For auth | *(empty)* | Password or API key. |
| `SmtpFrom` | Yes | `no-reply@care4kids.org` | RFC 5321 From: address. Must be verified with your provider. |
| `SmtpFromName` | No | `Care4Kids` | Human-readable sender name. |
| `PublicSiteUrl` | No | `https://care4kids.org` | Base URL for links in emails. |

---

## 3. Gmail SMTP

**Limits:** 500 emails/day (Google Workspace), 100/day (free accounts).

### Steps

1. Enable 2-Factor Authentication on your Google Account.
2. Go to **Google Account → Security → App passwords**.
3. Generate a new app password for "Mail" (or "Other (Custom name)" → "Care4Kids").
4. Copy the 16-character password.
5. Add/update `Web.config`:

```xml
<add key="SmtpEnabled"   value="true" />
<add key="SmtpHost"       value="smtp.gmail.com" />
<add key="SmtpPort"       value="587" />
<add key="SmtpUseSsl"     value="false" />
<add key="SmtpUsername"   value="your@gmail.com" />
<add key="SmtpPassword"   value="YOUR_APP_PASSWORD" />
<add key="SmtpFrom"       value="your@gmail.com" />
<add key="SmtpFromName"   value="Care4Kids" />
```

> **Note:** On 2022-05-30 Google tightened security. If you see `535-5.7.8` errors, you must use an **App Password**, not your account password. Regular account passwords no longer work with SMTP.

---

## 4. SendGrid

**Limits:** 100 emails/day on Free tier; 100,000/day on paid plans.

### Steps

1. Create a SendGrid account at [sendgrid.com](https://sendgrid.com).
2. Go to **Settings → API Keys → Create API Key**.
3. Choose **Full Access** (or **Restricted Access → Mail Send**).
4. Copy the API key (starts with `SG.`).
5. Verify a sender identity: **Settings → Sender Authentication → Verify a Single Sender**.
6. Add/update `Web.config`:

```xml
<add key="SmtpEnabled"   value="true" />
<add key="SmtpHost"       value="smtp.sendgrid.net" />
<add key="SmtpPort"       value="587" />
<add key="SmtpUseSsl"     value="false" />
<!-- For SendGrid, the username is always "apikey" -->
<add key="SmtpUsername"   value="apikey" />
<add key="SmtpPassword"   value="SG.YOUR_API_KEY_HERE" />
<add key="SmtpFrom"       value="hello@care4kids.org" />
<add key="SmtpFromName"   value="Care4Kids" />
```

> **Tip:** Set `SmtpFrom` to the verified sender address from Step 5. SendGrid will reject emails from unverified addresses.

---

## 5. Mailgun

**Limits:** 5,000 emails/month on free tier.

### Steps

1. Sign up at [mailgun.com](https://mailgun.com).
2. Add and verify your domain (Settings → Domains).
3. Note your **SMTP credentials** from the domain dashboard.
4. Add/update `Web.config`:

```xml
<add key="SmtpEnabled"   value="true" />
<add key="SmtpHost"       value="smtp.mailgun.org" />
<add key="SmtpPort"       value="587" />
<add key="SmtpUseSsl"     value="false" />
<add key="SmtpUsername"   value="postmaster@yourdomain.com" />
<add key="SmtpPassword"   value="YOUR_SMTP_PASSWORD" />
<add key="SmtpFrom"       value="hello@yourdomain.com" />
<add key="SmtpFromName"   value="Care4Kids" />
```

---

## 6. Local Development: smtp4dev

[smtp4dev](https://github.com/rnwood/smtp4dev) is a zero-config SMTP server that captures emails in a web UI — perfect for local development. Emails are never sent to real addresses.

### Installation

```bash
# .NET tool
dotnet tool install -g rnwood.smtp4dev

# Docker
docker run --rm -p 8080:80 -p 1025:25 rnwood/smtp4dev
```

### Running

```bash
smtp4dev --smtpport 1025 --httpport 8080
```

- SMTP listens on **localhost:1025**
- Web UI at **http://localhost:8080**

### Web.config for smtp4dev

```xml
<add key="SmtpEnabled"   value="true" />
<add key="SmtpHost"       value="localhost" />
<add key="SmtpPort"       value="1025" />
<add key="SmtpUseSsl"     value="false" />
<!-- No auth needed for smtp4dev -->
<add key="SmtpUsername"   value="" />
<add key="SmtpPassword"   value="" />
<add key="SmtpFrom"       value="no-reply@care4kids.org" />
```

### Sending a test email via curl

```bash
curl -v --mail-from test@localhost \
  --mail-rcpt recipient@example.com \
  --smtp-server localhost:1025 \
  --upload-file - <<EOF
From: no-reply@care4kids.org
To: recipient@example.com
Subject: Test email

This is a test.
EOF
```

---

## 7. Local Development: MailHog

[MailHog](https://github.com/mailhog/MailHog) is another popular local SMTP catcher, built in Go.

### Installation

```bash
# macOS
brew install mailhog

# Docker
docker run --rm -p 8025:8025 -p 1025:1025 mailhog/mailhog
```

### Running

```bash
mailhog
# SMTP: localhost:1025
# Web UI: http://localhost:8025
```

### Web.config for MailHog

```xml
<add key="SmtpEnabled"   value="true" />
<add key="SmtpHost"       value="localhost" />
<add key="SmtpPort"       value="1025" />
<add key="SmtpUseSsl"     value="false" />
<add key="SmtpUsername"   value="" />
<add key="SmtpPassword"   value="" />
<add key="SmtpFrom"       value="no-reply@care4kids.org" />
```

---

## 8. Security: Never Commit Secrets

**Critical:** Real SMTP credentials must never be committed to source control. Use one of these approaches:

### Option A — Environment Variables

Store credentials in environment variables and read them at runtime:

```xml
<add key="SmtpUsername"
     value="$(SMTP_USERNAME)" />
<add key="SmtpPassword"
     value="$(SMTP_PASSWORD)" />
```

On Azure App Service, set them in **Configuration → Application settings**.
On IIS, set them in **Environment Variables** on the Application Pool.

### Option B — Web.config Transformations

Create `Web.Release.config` (or `Web.Staging.config`) that overrides values during deployment:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  <appSettings>
    <add key="SmtpEnabled"
         value="true"
         xdt:Transform="SetAttributes" xdt:Locator="Match(key)" />
    <add key="SmtpHost"
         value="smtp.sendgrid.net"
         xdt:Transform="SetAttributes" xdt:Locator="Match(key)" />
    <add key="SmtpUsername"
         value="apikey"
         xdt:Transform="SetAttributes" xdt:Locator="Match(key)" />
    <add key="SmtpPassword"
         value="SG.REPLACE_WITH_REAL_KEY"
         xdt:Transform="SetAttributes" xdt:Locator="Match(key)" />
  </appSettings>
</configuration>
```

Apply with: `dotnet msbuild /p:Configuration=Release`

### Option C — User-Secrets (Development Only)

```bash
dotnet user-secrets set SmtpPassword "dev-password"
```

> **Never** put real credentials in `Web.config` or any committed file.

---

## 9. Troubleshooting

### `535-5.7.8` or `Authentication credentials invalid` (Gmail)

Your app password is wrong, or you used your account password instead of an App Password. See [Gmail SMTP](#3-gmail-smtp) Step 3.

### `530 5.7.0` Must issue a STARTTLS command first

`SmtpUseSsl` is set to `true` (implicit TLS) but you're using port 587, which expects STARTTLS. Set `SmtpUseSsl=false` for port 587.

### `The operation has timed out.`

- Firewall blocking outbound port 25/465/587.
- SMTP host or port is wrong.
- Try a different port (587 instead of 465).
- In corporate networks, port 587 may be blocked; try port 465 or a tunneling tool like [ngrok](https://ngrok.com).

### Emails go to spam

- Use a reputable SMTP provider (SendGrid, Mailgun, etc.) rather than Gmail for bulk mail.
- Set `Return-Path` / `SPF` / `DKIM` records for your sending domain.
- Use a consistent From: address that matches your domain.
- Care4Kids emails use a branded HTML template and a clear From: identity to improve deliverability.

### `EmailService` returns Success but email never arrives

1. Check the **Admin → Email Logs** page — was it logged as `Sent`?
2. If in mock mode (`SmtpEnabled=false`), all sends return success but nothing is transmitted.
3. Check spam/junk folders.
4. For Gmail, ensure the recipient hasn't blocked you and that the From: address is verified.

---

## 10. Testing the Setup

### 10.1 Verify mock mode is logging

1. Leave `SmtpEnabled=false`.
2. Send an invitation or make a donation in the app.
3. Go to **Admin → Email Logs** (`/admin/emails`).
4. You should see a row with `Status = MockSent`.

### 10.2 Verify real SMTP delivery

1. Set `SmtpEnabled=true` and configure your provider.
2. Start **smtp4dev** or **MailHog** locally on port 1025.
3. Perform an action in the app (invite a friend, donate).
4. Open smtp4dev/MailHog web UI and confirm the email appears.
5. Check **Admin → Email Logs** — the row should now show `Status = Sent` with a `SentAt` timestamp.

### 10.3 Test retry logic

1. With SMTP misconfigured (wrong port), cause an email send.
2. The row in **Email Logs** shows `Status = Failed` with an error message.
3. Click **Resend** on that row — it re-attempts immediately.
4. Alternatively click **Retry All Failed** to batch-retry all failed rows (up to 3 attempts per row by default).

### 10.4 Automated retry with Windows Task Scheduler

Create a PowerShell script `RetryEmails.ps1`:

```powershell
# RetryEmails.ps1
$base = "http://localhost:44300/api/admin/emails/retry-all"
$headers = @{ "Authorization" = "Bearer YOUR_ADMIN_TOKEN" }
$body = @{ maxAttempts = 3 } | ConvertTo-Json
Invoke-RestMethod -Uri $base -Method Post -Headers $headers -Body $body -ContentType "application/json"
```

Schedule it:

```powershell
# Run every 15 minutes
$trigger = New-ScheduledTaskTrigger -Once -At "09:00" -RepetitionInterval (New-TimeSpan -Minutes 15)
Register-ScheduledTask -Trigger $trigger -Action $action -TaskName "Care4Kids-EmailRetry" -RunLevel Highest
```

> For production, consider a hosted email service with built-in retry (SendGrid, Mailgun) so you don't need a background scheduler.
