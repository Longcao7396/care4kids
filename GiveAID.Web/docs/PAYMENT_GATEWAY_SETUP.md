# Payment Gateway Setup Guide

> **Care4Kids NGO — GiveAID Platform**
> Last updated: 2026-09-17

This document covers how to configure payment gateways for the GiveAID donation platform.

---

## Table of Contents

1. [Quick Start (Mock Mode)](#1-quick-start-mock-mode)
2. [Stripe Setup](#2-stripe-setup)
3. [VNPay Setup (Skeleton)](#3-vnpay-setup-skeleton)
4. [MoMo Setup (Skeleton)](#4-momo-setup-skeleton)
5. [Web.config Keys](#5-webconfig-keys)
6. [Database Migration](#6-database-migration)
7. [Local Testing with Stripe CLI](#7-local-testing-with-stripe-cli)
8. [Webhook Security](#8-webhook-security)
9. [Switching Between Gateways](#9-switching-between-gateways)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. Quick Start (Mock Mode)

Mock mode simulates a payment gateway without any real credentials. Useful for development and demos.

**Web.config** — add or update:

```xml
<appSettings>
  <!-- Mock gateway is active when Default=mock AND Enabled=true -->
  <add key="PaymentGateway__Default" value="mock" />
  <add key="PaymentGateway__Enabled" value="true" />

  <!-- Optional: simulate processing delay (ms) -->
  <add key="MockGateway__DelayMs" value="500" />

  <!-- Optional: probability 0-1 of random payment failure (for testing error flows) -->
  <add key="MockGateway__FailRate" value="0" />
</appSettings>
```

The mock gateway:
- Auto-approves every donation
- Returns a fake `clientSecret` and `transactionId`
- Webhook simulation is available via `POST /api/admin/payments/test-webhook`

**Frontend**: Mock mode shows a "Demo Mode" banner on the donation page. No Stripe.js is loaded.

---

## 2. Stripe Setup

Stripe is the primary payment gateway, supporting credit/debit cards and Vietnamese currency (VND).

### 2.1 Create a Stripe Account

1. Go to [https://dashboard.stripe.com](https://dashboard.stripe.com)
2. Register / sign in
3. Switch to **Test Mode** using the toggle in the top-right

### 2.2 Get API Keys

1. Navigate to **Developers → API Keys**
2. Copy the following:
   - **Publishable key** — starts with `pk_test_...` (for frontend `Stripe.js`)
   - **Secret key** — starts with `sk_test_...` (for backend only, NEVER expose to frontend)

### 2.3 Get Webhook Signing Secret

1. Navigate to **Developers → Webhooks**
2. Click **Add endpoint**
3. Endpoint URL: `https://your-domain.com/api/donations/webhook`
   - For local dev with Stripe CLI, use `http://localhost:3000/api/donations/webhook`
4. Select events to listen for:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `charge.refunded`
5. Click **Add endpoint**
6. Copy the **Signing secret** — starts with `whsec_...`

### 2.4 Add to Web.config

```xml
<appSettings>
  <!-- Stripe keys -->
  <add key="Stripe__ApiKey"          value="sk_test_YOUR_SECRET_KEY_HERE" />
  <add key="Stripe__WebhookSecret"   value="whsec_YOUR_WEBHOOK_SECRET_HERE" />
  <add key="Stripe__PublishableKey"  value="pk_test_YOUR_PUBLISHABLE_KEY_HERE" />

  <!-- Switch to Stripe gateway -->
  <add key="PaymentGateway__Default" value="stripe" />
  <add key="PaymentGateway__Enabled" value="true" />
</appSettings>
```

### 2.5 Stripe Test Cards

Use Stripe's [test card numbers](https://stripe.com/docs/testing#cards):

| Card Number | Result |
|---|---|
| `4242 4242 4242 4242` | Always succeeds |
| `4000 0025 0000 3155` | Requires 3D Secure |
| `4100 0000 0000 0019` | Always fails (insufficient funds) |
| `4000 0000 0000 9995` | Always fails (lost card) |

Any future expiry date and any 3-digit CVV will work in test mode.

### 2.6 Going Live

When ready for production:

1. Click **"Start making live payments"** in Stripe dashboard
2. Verify your business details
3. Replace test keys with live keys (`sk_live_...`, `pk_live_...`, `whsec_...`)
4. Update Web.config
5. Remove test cards from your Stripe dashboard

---

## 3. VNPay Setup (Skeleton)

VNPay integration is not yet implemented. To add it:

### 3.1 Create VNPay Merchant Account

1. Register at [https://sandbox.vnpayment.vn](https://sandbox.vnpayment.vn) (test)
2. Get your merchant credentials: Merchant ID, Terminal ID, Secure Hash key

### 3.2 Implement `VNPayPaymentGateway.cs`

Create `GiveAID.Web/Services/Payments/VNPayPaymentGateway.cs`:

```csharp
// Implements IPaymentGateway
// Key VNPay API docs: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/
//
// Key differences from Stripe:
// - VNPay uses a redirect flow (user is redirected to VNPay, then redirected back)
// - Webhook is optional — return URL provides payment result
// - HMAC-SHA256 signature verification for both request and response
//
// Steps:
// 1. CreatePaymentIntent → generate VNPay payment URL (redirect URL)
//    Frontend navigates to this URL for the user to complete payment
// 2. VerifyWebhook → parse the return URL params on the return URL endpoint
// 3. Refund → call VNPay refund API
//
// VNPay return URL (where user comes back after payment):
// GET /api/donations/vnpay-return?...
// Handle in a new endpoint: DonationsController.VNPayReturn()
```

### 3.3 Wire it up in `PaymentGatewayFactory.cs`

```csharp
case "vnpay":
    return new VNPayPaymentGateway();
```

### 3.4 Frontend Changes

In `DonatePage.js`, handle the redirect flow:

```javascript
// If gateway === 'vnpay' and clientSecret is a redirect URL:
// window.location.href = response.data.redirectUrl;
```

---

## 4. MoMo Setup (Skeleton)

MoMo integration is not yet implemented. To add it:

### 4.1 Create MoMo Merchant Account

1. Register at [https://business.momo.vn](https://business.momo.vn) (test)
2. Get your credentials: Partner Code, Access Key, Secret Key

### 4.2 Implement `MoMoPaymentGateway.cs`

Create `GiveAID.Web/Services/Payments/MoMoPaymentGateway.cs`:

```csharp
// Implements IPaymentGateway
// Key MoMo API docs: https://developers.momo.vn/
//
// MoMo payment flow:
// 1. CreatePaymentIntent → call MoMo payment API, get pay URL (QR code or deep link)
// 2. Frontend shows QR code or opens MoMo app
// 3. Webhook or callback URL confirms payment
//
// MoMo signature: HMAC-SHA256
// Supports both QR code and app-to-app payment
//
// Additional steps:
// - Create /api/donations/momo-callback endpoint
// - Create MoMoQrCode endpoint if using QR code flow
```

### 4.3 Wire it up in `PaymentGatewayFactory.cs`

```csharp
case "momo":
    return new MoMoPaymentGateway();
```

---

## 5. Web.config Keys

Add all required and optional keys to your `Web.config` `<appSettings>` section:

```xml
<appSettings>
  <!-- ─── Payment Gateway Configuration ────────────────────────────── -->

  <!-- Active gateway: "stripe" | "vnpay" | "momo" | "mock" -->
  <!-- Default: "mock" (no real payments) -->
  <add key="PaymentGateway__Default" value="stripe" />

  <!-- Global enable/disable. Set to "false" to disable all payments -->
  <add key="PaymentGateway__Enabled" value="true" />


  <!-- ─── Stripe (required when PaymentGateway__Default=stripe) ─────── -->

  <!-- Secret key — backend only, NEVER expose to frontend -->
  <add key="Stripe__ApiKey" value="sk_test_..." />

  <!-- Webhook signing secret from Stripe Dashboard -->
  <add key="Stripe__WebhookSecret" value="whsec_..." />

  <!-- Publishable key — returned in API responses for Stripe.js initialization -->
  <add key="Stripe__PublishableKey" value="pk_test_..." />

  <!-- Optional: Stripe API base URL (for proxies/corporate firewalls) -->
  <!-- <add key="Stripe__ApiBase" value="https://api.stripe.com/v1/" /> -->


  <!-- ─── Mock Gateway (only when PaymentGateway__Default=mock) ─────── -->

  <!-- Simulated processing delay in milliseconds (default: 500) -->
  <add key="MockGateway__DelayMs" value="500" />

  <!-- Probability 0-1 that a payment is randomly declined (default: 0) -->
  <!-- Set to 0.1 for 10% failure rate to test error handling -->
  <add key="MockGateway__FailRate" value="0" />


  <!-- ─── VNPay (stub — implement VNPayPaymentGateway first) ─────────── -->
  <!-- <add key="VNPay__MerchantId" value="YOUR_MERCHANT_ID" /> -->
  <!-- <add key="VNPay__TerminalId" value="YOUR_TERMINAL_ID" /> -->
  <!-- <add key="VNPay__SecureHashKey" value="YOUR_HASH_KEY" /> -->
  <!-- <add key="VNPay__ApiUrl" value="https://sandbox.vnpayment.vn/apis/vnpay/query" /> -->


  <!-- ─── MoMo (stub — implement MoMoPaymentGateway first) ──────────── -->
  <!-- <add key="MoMo__PartnerCode" value="YOUR_PARTNER_CODE" /> -->
  <!-- <add key="MoMo__AccessKey" value="YOUR_ACCESS_KEY" /> -->
  <!-- <add key="MoMo__SecretKey" value="YOUR_SECRET_KEY" /> -->
  <!-- <add key="MoMo__ApiUrl" value="https://test-payment.momo.vn/v2/gateway/api/create" /> -->

</appSettings>
```

---

## 6. Database Migration

Run the payment gateway migration SQL after the main schema:

```sql
-- In SQL Server Management Studio or via sqlcmd:
:r C:\path\to\NGO_Database_PaymentGateway_Migration.sql
```

What it does:
1. Adds `PaymentGateway` (NVARCHAR 20) and `ClientSecret` (NVARCHAR 500) columns to `Donations`
2. Creates `WebhookLogs` table for webhook event audit/debugging

**Columns added to `Donations`:**
- `PaymentGateway` — gateway used: "stripe", "vnpay", "momo", "mock"
- `ClientSecret` — gateway's client secret (Stripe pi_xxx_secret_xxx) for frontend confirmation

**New table `WebhookLogs`:**
- `WebhookLogId` (BIGINT IDENTITY) — primary key
- `Gateway` — "stripe", "vnpay", "momo", "mock"
- `EventType` — e.g. "payment_intent.succeeded"
- `EventId` — unique event ID from gateway (for deduplication)
- `RawPayload` — truncated to 4000 chars
- `SignatureValid` — did signature verification pass?
- `ProcessingStatus` — Processed | Failed | Ignored | Duplicate
- `DonationId` — linked donation if matched
- `ReceivedAt` / `ProcessedAt` — timestamps

---

## 7. Local Testing with Stripe CLI

Stripe CLI forwards webhook events to your local server — no Ngrok needed.

### 7.1 Install Stripe CLI

```bash
# Windows (PowerShell)
curl -sL https://stripe.me/stripe-cli -o stripe.exe
.\stripe.exe --version
```

Or download from: [https://stripe.com/docs/stripe-cli#install](https://stripe.com/docs/stripe-cli#install)

### 7.2 Login to Stripe CLI

```bash
stripe login
```

### 7.3 Start Webhook Forwarding

```bash
stripe listen --forward-to localhost:44300/api/donations/webhook
```

The CLI will print a **webhook signing secret** (starts with `whsec_...`). Add it to Web.config:

```xml
<add key="Stripe__WebhookSecret" value="whsec_..." />
```

### 7.4 Trigger Test Events

In a new terminal:

```bash
# Trigger a successful payment
stripe trigger payment_intent.succeeded

# Trigger a failed payment
stripe trigger payment_intent.payment_failed

# Trigger a refund
stripe trigger charge.refunded
```

### 7.5 Verify Webhook Logs

```bash
stripe logs tail
```

Or via the admin API:

```
GET /api/admin/payments/webhook-logs?gateway=stripe
```

### 7.6 Admin Test Webhook (Mock Gateway Only)

```bash
# First create a pending donation via the API, then:
curl -X POST http://localhost:44300/api/admin/payments/test-webhook \
  -H "Authorization: Bearer YOUR_ADMIN_JWT" \
  -H "Content-Type: application/json" \
  -d '{"donationId": 1, "eventType": "mock.payment.succeeded"}'
```

---

## 8. Webhook Security

### 8.1 Stripe Webhook Verification

Stripe webhooks are verified using HMAC-SHA256 signatures:
- Header: `Stripe-Signature`
- Secret: `Stripe__WebhookSecret` from Web.config
- Payload: raw request body (NOT parsed JSON — use the raw bytes)
- Tolerance: events older than 5 minutes are rejected

This is implemented in `StripePaymentGateway.VerifyWebhook()`.

**Important**: The webhook endpoint reads the raw request body. Ensure no middleware or filter parses the body before the endpoint — it would invalidate the HMAC signature.

### 8.2 Idempotency

Every gateway event has a unique `EventId`. The `WebhookLogs` table stores seen `EventId`s. If the same event is delivered twice (gateway retry), the second delivery is logged as "Duplicate" and acknowledged with HTTP 200 without re-processing.

### 8.3 Never Return Error Status Codes to Gateway

The webhook endpoint **always** returns HTTP 200 to acknowledge receipt. Even if processing fails:
1. Log the failure in `WebhookLogs` with `ProcessingStatus = "Failed"`
2. Return 200 immediately
3. Investigate and replay via `POST /api/admin/payments/{id}/retry-webhook` if needed

Returning 4xx/5xx to the gateway will cause it to retry indefinitely, flooding your logs.

---

## 9. Switching Between Gateways

Change `PaymentGateway__Default` in Web.config:

| Value | Gateway | Production Ready |
|---|---|---|
| `stripe` | Stripe (cards, VND) | ✅ Yes |
| `vnpay` | VNPay (bank transfer, cards, QR) | ❌ Not implemented |
| `momo` | MoMo (QR, app) | ❌ Not implemented |
| `mock` | Mock (fake payments) | ✅ Yes (dev/demo only) |

No code changes required. The `PaymentGatewayFactory` resolves the correct implementation at runtime.

---

## 10. Troubleshooting

### "Payment gateway error: No payment gateway is enabled"
**Cause**: `PaymentGateway__Enabled` is not set to `"true"` in Web.config.
**Fix**: Add `<add key="PaymentGateway__Enabled" value="true" />` to appSettings.

### "Payment gateway error: Stripe API key is not configured"
**Cause**: `Stripe__ApiKey` is missing or empty.
**Fix**: Add your `sk_test_...` key to Web.config.

### Webhook not firing / donation stuck in "Pending"
**Steps**:
1. Check `WebhookLogs` table: `SELECT * FROM WebhookLogs ORDER BY ReceivedAt DESC`
2. If empty: gateway is not sending events — verify webhook URL in gateway dashboard
3. If `SignatureValid = 0`: `Stripe__WebhookSecret` is wrong
4. If `ProcessingStatus = Failed`: check `ErrorMessage` for the exception
5. If `ProcessingStatus = Ignored`: transaction ID wasn't matched — check `DonationTransactionId`

### "Card was declined" in test mode
**Cause**: Using a failing test card (e.g. `4100 0000 0000 0019`).
**Fix**: Use `4242 4242 4242 4242` for always-succeed test payments.

### Stripe.js not loading
**Cause**: `Stripe__PublishableKey` not returned from backend.
**Fix**: Ensure `Stripe__PublishableKey` is set in Web.config and `PaymentGateway__Default=stripe`.

### 3D Secure / redirect not completing
**Cause**: `automatic_payment_methods[enabled]` is set but redirect-based methods are used.
**Fix**: For 3D Secure, Stripe handles it automatically. Ensure the webhook endpoint is publicly accessible (not localhost in production).

---

## Architecture Diagram

```
Frontend (DonatePage.js)
  │
  │ POST /api/donations  { amount, causeId, paymentGateway }
  ▼
Backend (DonationsController.Create)
  │
  │ PaymentGatewayFactory.Current.CreatePaymentIntent()
  ▼
┌─────────────────────────┐
│  IPaymentGateway        │
│  ├── StripePaymentGateway  ──► Stripe REST API (PaymentIntents)
│  ├── VNPayPaymentGateway   ──► VNPay API (TODO)
│  ├── MoMoPaymentGateway    ──► MoMo API (TODO)
│  └── MockPaymentGateway    ──► Fake in-process response
└─────────────────────────┘
  │ Returns { clientSecret, transactionId }
  ▼
Donation record created (status=Pending)
  │
  │ { clientSecret, stripePublishableKey }  ──► Frontend
  ▼
Frontend (Stripe.js)
  │
  │ stripe.confirmCardPayment(clientSecret, cardElement)
  ▼
Stripe.js iframe (PCI-DSS compliant card input)
  │
  │ Stripe validates and charges card
  ▼
Gateway Webhook ──► POST /api/donations/webhook
  │
  │ PaymentGatewayFactory.Current.VerifyWebhook()
  │ PaymentGatewayFactory.Current.Refund()
  ▼
WebhookLogs table (audit)
  │
  │ Apply status change → Donation status updated
  ▼
Done (Completed / Failed / Refunded)
```
