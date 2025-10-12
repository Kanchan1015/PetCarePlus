# Multi-Environment Configuration Guide

This document explains how the PetCarePlus application is configured to run in multiple environments with both frontend and backend in a single container.

## Environment Configuration

### 1. Development Environment
- **Frontend Configuration**: `.env` with `VITE_API_BASE_URL=http://localhost:5002/api`
- **Backend Configuration**: `appsettings.Development.json`
- **Database**: Local MySQL server
- **CORS**: Specific allowed origins (e.g., http://localhost:5173)
- **Logging**: Information level
- **Swagger**: Enabled
- **Deployment**: Separate frontend (Vite dev server) and backend (dotnet run)

### 2. Production Environment (Azure App Service)
- **Frontend Configuration**: `.env.production` with `VITE_API_BASE_URL=/api`
- **Backend Configuration**: `appsettings.Production.json` + Environment Variables
- **Database**: Azure Database for MySQL (connection string from Azure)
- **CORS**: Allow any origin (*)
- **Logging**: Warning level only
- **Swagger**: Disabled
- **Deployment**: Single container with frontend served from ASP.NET Core wwwroot

## Frontend API Configuration

### Key Changes Made:
1. **Development**: `VITE_API_BASE_URL=http://localhost:5002/api`
2. **Production**: `VITE_API_BASE_URL=/api` (relative URL for same-container communication)
3. **API Calls**: All API endpoints now exclude `/api` prefix since it's in the base URL
4. **Build Time**: Production environment variables are set during Docker build

### Example API Usage:
```typescript
const BASE = import.meta.env.VITE_API_BASE_URL; // "/api" in production
const response = await fetch(`${BASE}/auth/login`, { /* ... */ });
// Results in: "/api/auth/login" in production
```

## Configuration Priority

The application follows this configuration priority (highest to lowest):
1. Environment Variables
2. appsettings.{Environment}.json  
3. appsettings.json

## Required Environment Variables for Production

When deploying to Azure App Service, set these environment variables:

### Required Variables:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
MySql={Your Azure MySQL Connection String}
JWT_ISSUER=ProductionIssuer
JWT_AUDIENCE=ProductionAudience
JWT_SECRET={Your Production JWT Secret - minimum 32 characters}
```

### Example Azure MySQL Connection String:
```
Server={server_name}.mysql.database.azure.com;Database={database_name};Uid={username};Pwd={password};SslMode=Required;
```

## Docker Multi-Stage Build Process

### Stage 1: Backend Build
- Builds and publishes .NET application
- Optimized layered approach for faster rebuilds

### Stage 2: Frontend Build  
- Installs Node.js dependencies
- Creates production environment file during build:
  ```dockerfile
  RUN echo "VITE_API_BASE_URL=/api" > .env.production
  RUN echo "VITE_FEATURE_AUTH_ROUTING=true" >> .env.production
  ```
- Builds React application with production settings

### Stage 3: Final Runtime Image
- Combines backend and frontend in single ASP.NET Core application
- Frontend served from `wwwroot` directory
- Single port (80) for both API and static files

## Key Features of the Updated Program.cs

### 1. Environment-Specific Database Configuration
```csharp
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? Environment.GetEnvironmentVariable("MySql")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:MySql");
```

### 2. Static File Serving for Frontend
```csharp
app.UseStaticFiles(); // Serves files from wwwroot
app.MapFallbackToFile("index.html"); // SPA fallback routing
```

### 3. Environment-Specific CORS Configuration
```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddCors(o =>
    {
        o.AddPolicy("ui", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });
}
```

### 4. JWT Configuration with Environment Variable Support
```csharp
var issuer = jwt["Issuer"] 
    ?? Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new InvalidOperationException("Jwt:Issuer is missing or empty");
```

## Container Deployment

### Build and Deploy Commands:
```bash
# Build the container (includes both frontend and backend)
docker build -t petcareplus .

# Run locally for testing
docker run -p 8080:80 petcareplus

# Deploy to Azure Container Registry
az acr build --registry {your-registry} --image petcareplus:latest .
```

### Container Architecture:
- **Port 80**: Single entry point
- **API Routes**: `/api/*` → ASP.NET Core controllers  
- **Static Files**: `/*` → Frontend files from wwwroot
- **SPA Routing**: Unmapped routes → `index.html` for client-side routing

## Development vs Production Differences

| Aspect | Development | Production |
|--------|-------------|------------|
| Frontend Server | Vite dev server (port 5173) | ASP.NET Core wwwroot |
| Backend Server | dotnet run (port 5002) | ASP.NET Core (port 80) |
| API Base URL | `http://localhost:5002/api` | `/api` |
| Database | Local MySQL | Azure Database for MySQL |
| CORS | Specific origins | Allow any origin |
| Logging | Information | Warning only |
| Swagger | Enabled | Disabled |
| Configuration | appsettings files | Environment variables |

## Testing the Configuration

### Local Development Testing:
```bash
# Terminal 1: Start backend
cd backend/src/PetCare.Api
dotnet run --environment Development

# Terminal 2: Start frontend  
cd frontend
npm run dev
```

### Production Container Testing:
```bash
# Build and run container
docker build -t petcareplus .
docker run -p 8080:80 -e "MySql=your-connection-string" -e "JWT_SECRET=your-secret" petcareplus

# Access application at http://localhost:8080
```

## Troubleshooting

### Common Issues:
1. **API 404 Errors**: Check that API calls don't include duplicate `/api` paths
2. **CORS Issues**: Ensure frontend and backend are properly configured for environment
3. **Static Files Not Loading**: Verify `UseStaticFiles()` is configured in Program.cs
4. **SPA Routing Issues**: Ensure `MapFallbackToFile("index.html")` is configured
5. **Environment Variables**: Check Azure App Service configuration for all required variables

### Health Check:
- **API Health**: `GET /health` → `{"status":"ok"}`
- **Frontend**: Access root URL to verify SPA loads
- **Combined**: All frontend routes should work with client-side routing