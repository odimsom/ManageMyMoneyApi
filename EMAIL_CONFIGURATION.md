# Email Configuration Guide - SendGrid API

## ✅ Improvements Made

The API now uses **SendGrid API (HTTP)** instead of SMTP, which:
- ✅ Works perfectly with Railway (no port blocking issues)
- ✅ Is faster and more reliable
- ✅ Has better error reporting
- ✅ Requires **only 2 environment variables** (simpler setup)

## Railway Environment Variables (Simplified)

### Required Variables

```bash
SENDGRID_API_KEY=SG.your-api-key-here
SENDER_EMAIL=franciscodanielcastroborrome1@gmail.com
```

### Optional (has defaults)

```bash
SENDER_NAME=ManageMyMoney
```

## Complete Setup Guide

### Step 1: Get Your SendGrid API Key

1. Go to SendGrid: https://app.sendgrid.com
2. Navigate to **Settings** → **API Keys**
3. Click **"Create API Key"**
4. Name: `ManageMyMoney`
5. Permissions: **Full Access** or **Mail Send** (Restricted)
6. Click **"Create & View"**
7. **Copy the API Key** (starts with `SG.` - only shown once!)

### Step 2: Verify Your Sender Email

1. Go to **Settings** → **Sender Authentication**
2. Under **Single Sender Verification**, click **"Get Started"** or **"Verify a Single Sender"**
3. Fill the form:
   - From Name: `ManageMyMoney`
   - From Email: `franciscodanielcastroborrome1@gmail.com` (or your email)
   - Reply To: Same as From Email
   - Address, City, State, etc.: (fill with your info)
4. Click **"Create"**
5. **Check your email inbox** for verification link from SendGrid
6. **Click the verification link**
7. Wait for **"Verified"** ✅ status in SendGrid

### Step 3: Configure Railway

1. Go to your Railway project
2. Click on your service → **"Variables"** tab
3. **Remove old SMTP variables** (if they exist):
   - ❌ `SMTP_SERVER`
   - ❌ `SMTP_PORT`
   - ❌ `EMAIL_USERNAME`
   - ❌ `EMAIL_PASSWORD`
   - ❌ `SMTP_ENABLE_SSL`

4. **Add these new variables:**
   ```
   SENDGRID_API_KEY=SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
   SENDER_EMAIL=franciscodanielcastroborrome1@gmail.com
   ```

5. Railway will auto-deploy

### Step 4: Verify It Works

After deployment, check Railway logs for:

✅ **Success:**
```
=== Email Configuration Status ===
✅ SendGrid API: CONFIGURED - API Key: SG.xxxxxxx..., From: franciscodanielcastroborrome1@gmail.com
==================================
```

Then register a test user and look for:
```
📤 Sending email via SendGrid API to user@example.com
✅ Email sent successfully to user@example.com via SendGrid
```

⚠️ **Not Configured:**
```
⚠️  SendGrid API: NOT CONFIGURED
Missing variables: SENDGRID_API_KEY, SENDER_EMAIL
```

## Testing

1. Register a new user via your API
2. Check Railway logs:
   - ✅ Look for "Email sent successfully via SendGrid"
   - ❌ If you see "SendGrid API error", check the error details
3. Check your email inbox for the verification email
4. Check SendGrid Activity Feed:
   - Go to **Activity** in SendGrid dashboard
   - See real-time email delivery status

## Troubleshooting

### "Email not sent - SendGrid not configured"
- **Issue:** Missing SENDGRID_API_KEY or SENDER_EMAIL
- **Solution:** Add both environment variables in Railway

### "SendGrid API error: 401 Unauthorized"
- **Issue:** Invalid or missing API Key
- **Solution:** 
  1. Generate a new API Key in SendGrid
  2. Make sure it has "Mail Send" permission
  3. Copy the FULL key (starts with `SG.`)
  4. Update `SENDGRID_API_KEY` in Railway

### "SendGrid API error: 403 Forbidden - Sender not verified"
- **Issue:** FROM email not verified in SendGrid
- **Solution:** 
  1. Go to Settings → Sender Authentication
  2. Verify your email address
  3. Check your inbox for verification link

### Emails not received
- Check **spam/junk** folder
- Check **SendGrid Activity Feed** to see delivery status
- Verify sender email matches the one verified in SendGrid

### Railway Variables Not Working
- Make sure variable names are EXACT (case-sensitive)
- No extra spaces in variable values
- API Key must include the `SG.` prefix
- Click "Redeploy" after changing variables

## Why SendGrid API vs SMTP?

| Feature | SMTP (Old) | SendGrid API (New) |
|---------|------------|-------------------|
| **Port Blocking** | ❌ Railway blocks port 587 | ✅ Uses HTTPS (443) |
| **Speed** | Slow | ✅ Fast |
| **Error Details** | Generic errors | ✅ Detailed status codes |
| **Setup Complexity** | 6 variables | ✅ 2 variables |
| **Reliability** | Timeouts common | ✅ Highly reliable |

## SendGrid Free Tier

- **100 emails/day** for free
- Perfect for testing and small apps
- Upgrade later if needed

## Current Behavior

### Without SendGrid Configured
- ✅ Users can register successfully
- ⚠️ Emails are logged but not sent
- ℹ️ No errors or crashes

### With SendGrid Configured
- ✅ Users can register
- ✅ Verification emails are sent instantly
- ✅ All email templates work (welcome, password reset, etc.)

## Email Templates Available

All templates work out of the box once configured:
- ✉️ Email verification
- 🔐 Password reset
- 👋 Welcome emails (Casual, Organized, Power User)
- 📊 Budget alerts
- 🎯 Goal achievements
- 📈 Financial reports
- 🔔 System notifications

Templates location: `/app/Email/Templates/`

---

**Need help?** Check SendGrid documentation: https://docs.sendgrid.com/for-developers/sending-email/api-getting-started
