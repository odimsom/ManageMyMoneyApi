# 💰 ManageMyMoney API

API REST para gestión de finanzas personales construida con **.NET 8** siguiendo **Clean Architecture**.

## 🌐 URL de Producción

```
https://managemymoneyapi-production.up.railway.app
```

## 📖 Documentación Interactiva (Swagger)

```
https://managemymoneyapi-production.up.railway.app/
```

---

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** (Onion Architecture) con separación estricta de capas:

```
┌─────────────────────────────────────────────────────┐
│  Presentation.Api  (Controllers, API configuration) │
├─────────────────────────────────────────────────────┤
│  Infrastructure.Persistence  │  Infrastructure.Shared│
│  (EF Core, Repositories)     │  (External services)  │
├─────────────────────────────────────────────────────┤
│  Core.Application  (Use cases, DTOs, Interfaces)    │
├─────────────────────────────────────────────────────┤
│  Core.Domain  (Entities, Value Objects, Enums)      │
└─────────────────────────────────────────────────────┘
```

### Dependencias entre Capas
- **Domain** → Sin dependencias (C# puro)
- **Application** → Referencia solo a Domain
- **Infrastructure** → Referencia a Application (implementa interfaces)
- **Presentation** → Referencia a Application e Infrastructure (DI)

---

## 🚀 Características

- ✅ Autenticación JWT con refresh tokens
- ✅ Gestión de gastos e ingresos
- ✅ Categorías y subcategorías personalizables
- ✅ Múltiples cuentas financieras
- ✅ Presupuestos con alertas
- ✅ Metas de ahorro con contribuciones
- ✅ Reportes y estadísticas
- ✅ Exportación a Excel/CSV/PDF
- ✅ Notificaciones por email
- ✅ API documentada con Swagger

---

## 🛠️ Tecnologías

| Tecnología | Versión |
|------------|---------|
| .NET | 8.0 |
| Entity Framework Core | 8.0 |
| PostgreSQL | 17 |
| JWT Authentication | - |
| Swagger/OpenAPI | - |
| BCrypt | - |
| ClosedXML (Excel) | - |
| QuestPDF (PDF) | - |

---

## 📦 Estructura del Proyecto

```
ManageMyMoneyApi/
├── ManageMyMoney.Core.Domain/           # Entidades, Value Objects, Enums
├── ManageMyMoney.Core.Application/      # DTOs, Interfaces, Services
├── ManageMyMoney.Infrastructure.Persistence/  # EF Core, Repositories
├── ManageMyMoney.Infrastructure.Shared/       # Email, Export, Security
└── ManageMyMoney.Presentation.Api/      # Controllers, Middleware
```

---

## 🔧 Desarrollo Local

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

### Configuración

1. **Clonar el repositorio:**
```bash
git clone https://github.com/odimsom/ManageMyMoneyApi.git
cd ManageMyMoneyApi
```

2. **Configurar la base de datos** en `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "ManageMyMoneyConnection": "Host=localhost;Port=5432;Database=managemymoney_dev;Username=postgres;Password=tu_password"
  }
}
```

3. **Restaurar paquetes:**
```bash
dotnet restore
```

4. **Aplicar migraciones:**
```bash
dotnet ef database update -p ManageMyMoney.Infrastructure.Persistence -s ManageMyMoney.Presentation.Api
```

5. **Ejecutar la API:**
```bash
dotnet run --project ManageMyMoney.Presentation.Api
```

6. **Acceder a Swagger UI:**
```
http://localhost:5253
```

---

## 📚 API Endpoints

### 🔐 Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar usuario |
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/refresh-token` | Refrescar token |
| POST | `/api/auth/logout` | Cerrar sesión |
| GET | `/api/auth/me` | Obtener usuario actual |
| POST | `/api/auth/forgot-password` | Recuperar contraseña |
| POST | `/api/auth/reset-password` | Restablecer contraseña |
| POST | `/api/auth/change-password` | Cambiar contraseña |

### 💰 Gastos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/expenses` | Listar gastos (paginado) |
| GET | `/api/expenses/{id}` | Obtener gasto por ID |
| POST | `/api/expenses` | Crear gasto |
| POST | `/api/expenses/quick` | Crear gasto rápido |
| PUT | `/api/expenses/{id}` | Actualizar gasto |
| DELETE | `/api/expenses/{id}` | Eliminar gasto |
| GET | `/api/expenses/summary/monthly` | Resumen mensual |
| GET | `/api/expenses/summary/category` | Resumen por categoría |
| GET | `/api/expenses/export/excel` | Exportar a Excel |
| GET | `/api/expenses/export/csv` | Exportar a CSV |

### 📂 Categorías

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/categories` | Listar categorías |
| GET | `/api/categories/expenses` | Categorías de gastos |
| GET | `/api/categories/income` | Categorías de ingresos |
| POST | `/api/categories` | Crear categoría |
| PUT | `/api/categories/{id}` | Actualizar categoría |
| DELETE | `/api/categories/{id}` | Eliminar categoría |

### 🏦 Cuentas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/accounts` | Listar cuentas |
| GET | `/api/accounts/summary` | Resumen de cuentas |
| POST | `/api/accounts` | Crear cuenta |
| PUT | `/api/accounts/{id}` | Actualizar cuenta |
| DELETE | `/api/accounts/{id}` | Desactivar cuenta |
| POST | `/api/accounts/transfer` | Transferir entre cuentas |

### 📊 Presupuestos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/budgets` | Listar presupuestos |
| GET | `/api/budgets/{id}/progress` | Progreso del presupuesto |
| POST | `/api/budgets` | Crear presupuesto |
| PUT | `/api/budgets/{id}` | Actualizar presupuesto |
| DELETE | `/api/budgets/{id}` | Desactivar presupuesto |

### 🎯 Metas de Ahorro

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/budgets/goals` | Listar metas |
| POST | `/api/budgets/goals` | Crear meta |
| POST | `/api/budgets/goals/{id}/contributions` | Agregar contribución |

### 💵 Ingresos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/income` | Listar ingresos |
| POST | `/api/income` | Crear ingreso |
| PUT | `/api/income/{id}` | Actualizar ingreso |
| DELETE | `/api/income/{id}` | Eliminar ingreso |

### 📈 Reportes

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/reports/summary` | Resumen financiero |
| GET | `/api/reports/monthly` | Reporte mensual |
| GET | `/api/reports/yearly` | Reporte anual |
| POST | `/api/reports/comparison` | Comparar períodos |
| GET | `/api/reports/trends/expenses` | Tendencias de gastos |

---

## 🔐 Autenticación

La API usa **JWT (JSON Web Tokens)**. Incluye el token en el header:

```http
Authorization: Bearer <tu_token_jwt>
```

### Ejemplo de Login

```bash
curl -X POST https://managemymoneyapi-production.up.railway.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "usuario@ejemplo.com", "password": "MiPassword123!"}'
```

---

## 📱 Ejemplo de Integración (JavaScript)

```javascript
const API_URL = 'https://managemymoneyapi-production.up.railway.app/api';

// Login
const login = async (email, password) => {
  const response = await fetch(`${API_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  return response.json();
};

// Obtener gastos (con token)
const getExpenses = async (token) => {
  const response = await fetch(`${API_URL}/expenses`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  return response.json();
};

// Crear gasto
const createExpense = async (token, expense) => {
  const response = await fetch(`${API_URL}/expenses`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(expense)
  });
  return response.json();
};
```

---

## � Despliegue en Railway

### Variables de Entorno Requeridas

Para desplegar en producción (Railway), debes configurar las siguientes variables de entorno:

#### 🔑 JWT (Requerido)
```bash
JWT_SECRET_KEY=TuClaveSecretaSuperSeguraDeAlMenos32CaracteresParaProduccion!
```

#### 📧 Email/SMTP (Requerido para notificaciones)
```bash
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SENDER_EMAIL=tuemail@gmail.com
SENDER_NAME=ManageMyMoney
EMAIL_USERNAME=tuemail@gmail.com
EMAIL_PASSWORD=tu-contraseña-de-aplicacion-de-gmail
SMTP_ENABLE_SSL=true
```

> **⚠️ Importante para Gmail**: Debes generar una "Contraseña de aplicación":
> 1. Ve a https://myaccount.google.com/security
> 2. Activa la verificación en 2 pasos
> 3. Genera una contraseña de aplicación para "Correo"
> 4. Usa esa contraseña en `EMAIL_PASSWORD` (NO tu contraseña normal)

#### 🗄️ Base de Datos
Railway proporciona automáticamente `DATABASE_URL` cuando agregas PostgreSQL.

#### 📖 Guía Completa
Ver [RAILWAY_SETUP.md](RAILWAY_SETUP.md) para instrucciones detalladas.

### Monedas Soportadas 🌍

La API incluye estas monedas por defecto:
- 🇺🇸 USD - US Dollar
- 🇪🇺 EUR - Euro  
- 🇬🇧 GBP - British Pound
- 🇯🇵 JPY - Japanese Yen
- 🇨🇦 CAD - Canadian Dollar
- 🇦🇺 AUD - Australian Dollar
- 🇨🇭 CHF - Swiss Franc
- 🇨🇳 CNY - Chinese Yuan
- 🇲🇽 MXN - Mexican Peso
- 🇧🇷 BRL - Brazilian Real
- 🇦🇷 ARS - Argentine Peso
- 🇨🇴 COP - Colombian Peso
- 🇨🇱 CLP - Chilean Peso
- 🇵🇪 PEN - Peruvian Sol
- 🇩🇴 **DOP - Dominican Peso (RD$)**

---

## �🚨 Códigos de Error

| Código | Descripción |
|--------|-------------|
| 200 | OK - Solicitud exitosa |
| 201 | Created - Recurso creado |
| 400 | Bad Request - Error de validación |
| 401 | Unauthorized - No autenticado |
| 403 | Forbidden - Sin permisos |
| 404 | Not Found - Recurso no encontrado |
| 409 | Conflict - Conflicto (ej: email duplicado) |
| 500 | Internal Server Error - Error del servidor |

---

## 🗄️ Base de Datos

### Convenciones (PostgreSQL)

- Tablas: `snake_case` (ej: `expense_tags`, `savings_goals`)
- Columnas: `snake_case` (ej: `created_at`, `user_id`)
- Índices: `ix_{tabla}_{columnas}`

### Tipos de Datos

| Tipo | Formato |
|------|---------|
| Money | `decimal(18,2)` |
| Exchange rates | `decimal(18,6)` |
| Percentages | `decimal(5,2)` |
| Currency codes | `varchar(3)` |
| Timestamps | `timestamp` |

---

## 🧪 Comandos Útiles

```bash
# Build
dotnet build

# Run
dotnet run --project ManageMyMoney.Presentation.Api

# Crear migración
dotnet ef migrations add NombreMigracion -p ManageMyMoney.Infrastructure.Persistence -s ManageMyMoney.Presentation.Api

# Aplicar migraciones
dotnet ef database update -p ManageMyMoney.Infrastructure.Persistence -s ManageMyMoney.Presentation.Api

# Tests (cuando se agreguen)
dotnet test
```

---

## 📄 Licencia

Este proyecto es de uso privado.

---

## 👨‍💻 Autor

**Francisco Daniel Castro**

- GitHub: [@odimsom](https://github.com/odimsom)
