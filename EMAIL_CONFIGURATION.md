# Email Configuration Guide

## Problem Solved

The API now handles missing email configuration gracefully:
- ✅ User registration proceeds even if emails can't be sent
- ✅ Clear logging shows which environment variables are missing
- ✅ No crashes or exceptions when SMTP is unavailable

## Railway Environment Variables

To enable email functionality, configure these variables in Railway:

### Required Variables

```bash
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SENDER_EMAIL=your-email@gmail.com
SENDER_NAME=ManageMyMoney
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
SMTP_ENABLE_SSL=true
```

### Gmail Configuration (Recommended)

1. **Enable 2-Step Verification**
   - Go to https://myaccount.google.com/security
   - Enable "2-Step Verification"

2. **Generate App Password**
   - Go to https://myaccount.google.com/apppasswords
   - Select "Mail" and "Other (Custom name)"
   - Enter "ManageMyMoney API"
   - Copy the 16-character password
   - Use this in `EMAIL_PASSWORD`

3. **Set Railway Variables**
   ```
   SMTP_SERVER=smtp.gmail.com
   SMTP_PORT=587
   SENDER_EMAIL=your-email@gmail.com
   EMAIL_USERNAME=your-email@gmail.com
   EMAIL_PASSWORD=xxxx xxxx xxxx xxxx (16-character app password)
   SMTP_ENABLE_SSL=true
   ```

### Alternative SMTP Providers

#### SendGrid
```bash
SMTP_SERVER=smtp.sendgrid.net
SMTP_PORT=587
SENDER_EMAIL=noreply@yourdomain.com
EMAIL_USERNAME=apikey
EMAIL_PASSWORD=your-sendgrid-api-key
SMTP_ENABLE_SSL=true
```

#### Mailgun
```bash
SMTP_SERVER=smtp.mailgun.org
SMTP_PORT=587
SENDER_EMAIL=noreply@yourdomain.com
EMAIL_USERNAME=your-mailgun-username
EMAIL_PASSWORD=your-mailgun-password
SMTP_ENABLE_SSL=true
```

#### Amazon SES
```bash
SMTP_SERVER=email-smtp.us-east-1.amazonaws.com
SMTP_PORT=587
SENDER_EMAIL=noreply@yourdomain.com
EMAIL_USERNAME=your-smtp-username
EMAIL_PASSWORD=your-smtp-password
SMTP_ENABLE_SSL=true
```

## Verification

After configuring, check your Railway logs for:

✅ **Success:**
```
Email service initialized successfully. SMTP: smtp.gmail.com:587, From: your-email@gmail.com
```

⚠️ **Not Configured:**
```
Email service is NOT configured. Emails will not be sent.
Missing variables: SENDER_EMAIL, EMAIL_USERNAME, EMAIL_PASSWORD
```

## Testing

1. Register a new user via the API
2. Check logs for email status:
   - If configured: "📧 Email sent successfully"
   - If not configured: "📧 Email not sent - SMTP not configured"

## Troubleshooting

### "Network is unreachable" Error
- ✅ **Fixed:** The app won't crash, but emails won't be sent
- **Solution:** Configure the SMTP environment variables

### Gmail "Less secure app" Error
- **Issue:** Gmail blocks basic authentication
- **Solution:** Use App Password instead of account password

### SMTP Timeout
- Check SMTP_PORT (should be 587 for TLS)
- Verify SMTP_ENABLE_SSL=true
- Ensure Railway can reach external SMTP servers

### Emails Not Received
- Check spam/junk folder
- Verify SENDER_EMAIL is correct
- Check Railway logs for "Email sent successfully"

## Current Behavior

### Without Email Configuration
- ✅ Users can register
- ✅ Users receive API response
- ⚠️ Verification emails are logged but not sent
- ℹ️ Manual email verification may be needed

### With Email Configuration
- ✅ Users can register
- ✅ Verification emails are sent
- ✅ All email templates work (welcome, password reset, etc.)

## Email Templates Available

The app supports these email types:
- Email verification
- Password reset
- Welcome emails (3 styles: Casual, Organized, Power User)
- Budget alerts
- Goal achievements
- Financial reports
- System notifications

All templates are located in: `/app/Email/Templates/`
