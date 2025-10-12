# Admin User Seeding Migration

## Overview
This migration creates a hardcoded admin user in your database to allow initial system access since admin accounts cannot be created via the public registration endpoint.

## Migration Details
- **File**: `20250926000000_SeedAdminUser.cs`
- **Location**: `backend/src/PetCare.Infrastructure/Persistence/Migrations/`

## What it creates:

### 1. Roles
- **Admin**: For system administrators
- **Vet**: For veterinarians  
- **Owner**: For pet owners

### 2. Admin User
- **Email**: `admin@petcareplus.com`
- **Username**: `admin@petcareplus.com`
- **Password**: `Admin@123!`
- **Full Name**: `System Administrator`
- **Account Status**: Active (0)
- **Role**: Admin

## How to Apply the Migration

### Option 1: Using dotnet ef (Recommended)
```bash
cd backend/src/PetCare.Infrastructure
dotnet ef database update --context PetCareDbContext --startup-project ../PetCare.Api/PetCare.Api.csproj
```

### Option 2: Automatic on Application Startup
If your application is configured to apply migrations on startup, they will be applied automatically when you deploy.

### Option 3: Manual SQL Execution
If needed, you can run the SQL commands manually in your MySQL database:

```sql
-- Insert roles
INSERT IGNORE INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES 
('admin-role-guid', 'Admin', 'ADMIN', 'admin-concurrency-stamp'),
('vet-role-guid', 'Vet', 'VET', 'vet-concurrency-stamp'),
('owner-role-guid', 'Owner', 'OWNER', 'owner-concurrency-stamp');

-- Insert admin user
INSERT IGNORE INTO AspNetUsers 
(Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
 PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, 
 TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, AccountStatus)
VALUES 
('admin-user-guid', 'admin@petcareplus.com', 'ADMIN@PETCAREPLUS.COM', 
 'admin@petcareplus.com', 'ADMIN@PETCAREPLUS.COM', 1, 
 'AQAAAAIAAYagAAAAELvHd5Auc5JKDSUgS4PgNdqphNiZx9zLsXEzHFIyZx4Zz/9uQwY0j8tTr1yJqDnKzQ==', 
 'admin-security-stamp', 'admin-user-concurrency', 0, 0, 1, 0, 
 'System Administrator', 0);

-- Assign admin role
INSERT IGNORE INTO AspNetUserRoles (UserId, RoleId)
VALUES ('admin-user-guid', 'admin-role-guid');
```

## Security Notes

⚠️ **IMPORTANT**: After applying this migration:

1. **Change the default password immediately** after first login
2. The password `Admin@123!` is only for initial setup
3. Consider implementing password change enforcement on first login
4. The password hash is generated using ASP.NET Core Identity's `PasswordHasher<ApplicationUser>`

## Login Credentials
```
Email: admin@petcareplus.com
Password: Admin@123!
```

## Verification
After applying the migration, you can verify it worked by:
1. Checking the database tables: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`
2. Logging in with the credentials above
3. Accessing admin-only endpoints like `/api/admin/users`

## Azure App Service Configuration

### 🚨 CRITICAL: Required Environment Variables
Your application **WILL FAIL TO START** without these environment variables in Azure App Service:

#### JWT Configuration (Application Settings)
```
JWT_SECRET=your-super-secret-jwt-key-at-least-32-characters-long
JWT_ISSUER=PetCare
JWT_AUDIENCE=PetCare.Client
```

#### Container Configuration
```
WEBSITES_PORT=80
WEBSITES_ENABLE_APP_SERVICE_STORAGE=false
```

#### ASP.NET Core Configuration
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
```

### 🗄️ Connection String (Connection Strings section)
```
Name: MySql
Value: Server=your-mysql-server;Database=your-database;Uid=your-username;Pwd=your-password;
Type: MySQL
```

### ⚠️ Security Notes
- **JWT_SECRET**: Must be at least 32 characters long, use a cryptographically secure random string
- **MySQL Password**: Use a strong password for your database
- **Admin Password**: Change `Admin@123!` immediately after first login

## Azure Deployment
Since your app is deployed to Azure App Service, the migration will be applied automatically when the new container image is deployed (if configured to run migrations on startup) or you can run it manually using Azure CLI:

```bash
# If you have access to the Azure container console
dotnet ef database update --context PetCareDbContext
```

## Quick Setup Checklist for Azure
- [ ] Set `JWT_SECRET` environment variable (32+ character random string)
- [ ] Set `JWT_ISSUER=PetCare` environment variable
- [ ] Set `JWT_AUDIENCE=PetCare.Client` environment variable
- [ ] Set `WEBSITES_PORT=80` environment variable
- [ ] Set `WEBSITES_ENABLE_APP_SERVICE_STORAGE=false` environment variable
- [ ] Configure MySQL connection string in Connection Strings section
- [ ] Deploy application with migration
- [ ] Test admin login: `admin@petcareplus.com` / `Admin@123!`
- [ ] Change admin password immediately