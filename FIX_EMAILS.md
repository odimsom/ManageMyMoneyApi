# 🔧 Solución: Emails No Se Envían en Railway

## 📋 Problema Identificado

Los emails no se estaban enviando porque:
1. ❌ Las **plantillas de email no se copiaban** al contenedor Docker
2. ❌ La ruta de las plantillas no se resolvía correctamente en producción
3. ⚠️ Faltaban **variables de entorno** de SMTP en Railway

## ✅ Cambios Realizados

### 1. Configuración del Proyecto (`.csproj`)
**Archivo**: `ManageMyMoney.Infrastructure.Shared.csproj`

Agregado para copiar plantillas al output:
```xml
<ItemGroup>
  <None Update="Email\Templates\**\*.html">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### 2. Servicio de Email
**Archivo**: `ManageMyMoney.Infrastructure.Shared/Services/Email/EmailService.cs`

- ✅ Resuelve la ruta de plantillas usando `AppContext.BaseDirectory`
- ✅ Agrega logging para debug: muestra la ruta completa de plantillas
- ✅ Mejor manejo de errores cuando no se encuentra una plantilla

### 3. Configuración de Email con Variables de Entorno
**Archivo**: `ManageMyMoney.Infrastructure.Shared/DependencyInjection.cs`

Ahora lee variables de entorno:
- `SMTP_SERVER`
- `SMTP_PORT`
- `SENDER_EMAIL`
- `SENDER_NAME`
- `EMAIL_USERNAME`
- `EMAIL_PASSWORD`
- `SMTP_ENABLE_SSL`

### 4. Peso Dominicano Agregado 🇩🇴
**Archivo**: `ManageMyMoney.Infrastructure.Persistence/Seeds/CurrencySeed.cs`

- ✅ DOP - Dominican Peso (RD$)

### 5. Documentación Creada
- ✅ `RAILWAY_SETUP.md` - Guía completa de deploy
- ✅ `.env.example` - Variables de entorno requeridas
- ✅ `verify-templates.ps1` - Script para verificar plantillas

---

## 🚀 Pasos Siguientes (Railway)

### 1. Hacer Commit y Push de los Cambios

```bash
git add .
git commit -m "Fix: Email templates deployment & add environment variables support"
git push origin main
```

Railway automáticamente detectará los cambios y hará redeploy.

### 2. Configurar Variables de Entorno en Railway

Ve a tu proyecto en Railway → Variables → Agrega:

```bash
# JWT (ya debería estar)
JWT_SECRET_KEY=TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!

# Email - GMAIL (RECOMENDADO)
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SENDER_EMAIL=franciscodanielcastroborrome1@gmail.com
SENDER_NAME=ManageMyMoney
EMAIL_USERNAME=franciscodanielcastroborrome1@gmail.com
EMAIL_PASSWORD=lhkaqsvkrahekpdj
SMTP_ENABLE_SSL=true
```

> ⚠️ **IMPORTANTE**: Para Gmail en producción, necesitas usar una **Contraseña de Aplicación**:
> 1. Ve a https://myaccount.google.com/security
> 2. Activa "Verificación en 2 pasos"
> 3. Busca "Contraseñas de aplicaciones"
> 4. Genera una nueva para "Correo"
> 5. Reemplaza el valor de `EMAIL_PASSWORD` con esa contraseña

### 3. Verificar el Deploy

Después del redeploy, revisa los logs de Railway. Deberías ver:

```
✅ Email templates base path: /app/Email/Templates
✅ User registered successfully: usuario@ejemplo.com
✅ Email sent successfully to usuario@ejemplo.com
```

Si ves:
```
❌ Email template not found: System/EmailVerification at /app/Email/Templates/System/EmailVerification.html
```

Significa que las plantillas no se copiaron. Verifica el build.

### 4. Probar el Envío de Emails

1. Registra un nuevo usuario en: https://managemymoneyapi-production.up.railway.app/
2. Revisa los logs de Railway
3. Verifica el inbox del email registrado

---

## 🧪 Verificación Local (Opcional)

Antes de hacer push, puedes verificar localmente:

```powershell
# Verificar que las plantillas se copian
.\verify-templates.ps1

# Build y run local
dotnet run --project ManageMyMoney.Presentation.Api
```

---

## 📧 Proveedores SMTP Alternativos

Si tienes problemas con Gmail, puedes usar:

### SendGrid (Recomendado para producción)
```bash
SMTP_SERVER=smtp.sendgrid.net
SMTP_PORT=587
EMAIL_USERNAME=apikey
EMAIL_PASSWORD=tu-api-key-de-sendgrid
SMTP_ENABLE_SSL=true
```

### Mailgun
```bash
SMTP_SERVER=smtp.mailgun.org
SMTP_PORT=587
EMAIL_USERNAME=tu-usuario@mailgun
EMAIL_PASSWORD=tu-password-mailgun
SMTP_ENABLE_SSL=true
```

### Mailtrap (Solo para testing)
```bash
SMTP_SERVER=smtp.mailtrap.io
SMTP_PORT=2525
EMAIL_USERNAME=tu-usuario-mailtrap
EMAIL_PASSWORD=tu-password-mailtrap
SMTP_ENABLE_SSL=false
```

---

## 🐛 Troubleshooting

### Problema: "Email template not found"
**Solución**: 
- Verifica que hiciste commit del cambio en `.csproj`
- Verifica los logs: debería mostrar la ruta de templates
- Railway debe hacer rebuild completo

### Problema: "SMTP error 535: Authentication failed"
**Solución**:
- Para Gmail: Usa contraseña de aplicación (no tu contraseña normal)
- Verifica que las credenciales sean correctas
- Verifica que `SMTP_ENABLE_SSL=true`

### Problema: Los emails se envían pero no llegan
**Solución**:
- Revisa la carpeta de SPAM
- Verifica que `SENDER_EMAIL` sea válido
- Si usas Gmail, verifica que esté permitido el acceso SMTP

---

## ✨ Resultado Final

Después de estos cambios:
- ✅ Los emails de verificación se enviarán automáticamente
- ✅ Las plantillas estarán disponibles en el contenedor
- ✅ El peso dominicano (DOP) estará en las monedas
- ✅ Configuración flexible con variables de entorno
- ✅ Mejor logging para debugging

---

**Autor**: GitHub Copilot
**Fecha**: 2026-02-04
