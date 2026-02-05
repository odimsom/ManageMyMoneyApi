# Railway Deployment Setup

## Variables de Entorno Requeridas

Para que la aplicación funcione correctamente en Railway, debes configurar las siguientes variables de entorno:

### 1. Base de Datos (PostgreSQL)

Railway automáticamente proporciona `DATABASE_URL` cuando agregas un servicio PostgreSQL. Sin embargo, nuestra aplicación usa un formato diferente:

```bash
# Railway proporciona:
DATABASE_URL=postgresql://user:password@host:port/database

# Nuestra aplicación necesita (Railway puede transformarlo automáticamente):
CONNECTIONSTRINGS__MANAGEMYMONEYCONNECTION=Host=host;Port=port;Database=database;Username=user;Password=password
```

**Solución**: Agregar un servicio PostgreSQL en Railway conectará automáticamente la variable `DATABASE_URL`. Si necesitas el formato específico, agrégalo manualmente.

### 2. JWT (Autenticación)

**Requerido**:
```bash
JWT_SECRET_KEY=TuClaveSecretaSuperSeguraDeAlMenos32CaracteresParaProduccion!
```

**Opcional** (tienen valores por defecto):
```bash
JWT_ISSUER=ManageMyMoney
JWT_AUDIENCE=ManageMyMoneyUsers
```

### 3. Email (SMTP)

**Requerido para envío de emails**:

#### Usando Gmail:
```bash
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SENDER_EMAIL=tunombre@gmail.com
SENDER_NAME=ManageMyMoney
EMAIL_USERNAME=tunombre@gmail.com
EMAIL_PASSWORD=tu-contraseña-de-aplicacion-de-gmail
SMTP_ENABLE_SSL=true
```

> **Nota importante sobre Gmail**: Necesitas generar una "Contraseña de aplicación" en tu cuenta de Google:
> 1. Ve a https://myaccount.google.com/security
> 2. Activa la verificación en 2 pasos
> 3. Genera una contraseña de aplicación
> 4. Usa esa contraseña en `EMAIL_PASSWORD`

#### Usando otros proveedores SMTP:
```bash
SMTP_SERVER=smtp.tuproveedor.com
SMTP_PORT=587
SENDER_EMAIL=noreply@tudominio.com
SENDER_NAME=ManageMyMoney
EMAIL_USERNAME=tu-usuario-smtp
EMAIL_PASSWORD=tu-password-smtp
SMTP_ENABLE_SSL=true
```

### 4. Aplicación

**Opcional** (Railway maneja PORT automáticamente):
```bash
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

## Configuración Paso a Paso en Railway

1. **Conecta tu repositorio GitHub** a Railway

2. **Agrega un servicio PostgreSQL**:
   - Click en "New" → "Database" → "PostgreSQL"
   - Railway automáticamente conectará `DATABASE_URL`

3. **Configura las variables de entorno** en tu servicio:
   - Ve a tu servicio → "Variables"
   - Agrega cada variable listada arriba

4. **Variables mínimas requeridas**:
   ```
   JWT_SECRET_KEY=TuClaveSegura32CaracteresOMas!
   SMTP_SERVER=smtp.gmail.com
   SMTP_PORT=587
   SENDER_EMAIL=tuemail@gmail.com
   SENDER_NAME=ManageMyMoney
   EMAIL_USERNAME=tuemail@gmail.com
   EMAIL_PASSWORD=tu-contraseña-de-aplicacion
   SMTP_ENABLE_SSL=true
   ```

5. **Deploy**:
   - Railway automáticamente hará deploy después de configurar las variables
   - Verifica los logs para asegurarte de que todo está funcionando

## Verificar que los Emails Funcionan

1. Revisa los logs de Railway para ver mensajes de error relacionados con SMTP
2. Intenta registrar un usuario nuevo
3. Deberías ver en los logs: `Email sent successfully to {email}`
4. Si ves errores SMTP, verifica:
   - Que `EMAIL_PASSWORD` sea una contraseña de aplicación (no tu contraseña normal de Gmail)
   - Que `SMTP_ENABLE_SSL=true` esté configurado
   - Que las credenciales sean correctas

## Troubleshooting

### "Cannot create a SymmetricSecurityKey, key length is zero"
- **Causa**: `JWT_SECRET_KEY` no está configurado
- **Solución**: Agrega la variable de entorno `JWT_SECRET_KEY` con al menos 32 caracteres

### "SMTP error sending email"
- **Causa**: Credenciales de email incorrectas o configuración SMTP errónea
- **Solución**: 
  - Para Gmail: Usa contraseña de aplicación, no tu contraseña normal
  - Verifica que todas las variables EMAIL_* estén configuradas
  - Revisa los logs para el error específico

### La base de datos no se conecta
- **Causa**: Variable de conexión no configurada correctamente
- **Solución**: Verifica que el servicio PostgreSQL esté vinculado a tu aplicación en Railway

## Monedas Soportadas

La aplicación incluye las siguientes monedas predeterminadas:
- USD - US Dollar ($)
- EUR - Euro (€)
- GBP - British Pound (£)
- JPY - Japanese Yen (¥)
- CAD - Canadian Dollar (CA$)
- AUD - Australian Dollar (A$)
- CHF - Swiss Franc (CHF)
- CNY - Chinese Yuan (¥)
- MXN - Mexican Peso ($)
- BRL - Brazilian Real (R$)
- ARS - Argentine Peso ($)
- COP - Colombian Peso ($)
- CLP - Chilean Peso ($)
- PEN - Peruvian Sol (S/)
- **DOP - Dominican Peso (RD$)** ⬅️ Recién agregado

Las monedas se seedean automáticamente al iniciar la aplicación por primera vez.
