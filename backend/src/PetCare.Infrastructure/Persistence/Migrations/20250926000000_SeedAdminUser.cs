using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Admin role if it doesn't exist
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                VALUES ('admin-role-guid', 'Admin', 'ADMIN', 'admin-concurrency-stamp');
            ");

            // Create Vet role if it doesn't exist (for completeness)
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                VALUES ('vet-role-guid', 'Vet', 'VET', 'vet-concurrency-stamp');
            ");

            // Create Owner role if it doesn't exist (for completeness)
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                VALUES ('owner-role-guid', 'Owner', 'OWNER', 'owner-concurrency-stamp');
            ");

            // ASP.NET Core Identity hash for password 'Admin@123!'
            // This hash was generated using: new PasswordHasher<ApplicationUser>().HashPassword(user, "Admin@123!")
            var passwordHash = "AQAAAAIAAYagAAAAELvHd5Auc5JKDSUgS4PgNdqphNiZx9zLsXEzHFIyZx4Zz/9uQwY0j8tTr1yJqDnKzQ==";
            
            // Insert the admin user
            migrationBuilder.Sql($@"
                INSERT IGNORE INTO AspNetUsers 
                (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                 PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, 
                 TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, AccountStatus)
                VALUES 
                ('admin-user-guid', 'admin@petcareplus.com', 'ADMIN@PETCAREPLUS.COM', 
                 'admin@petcareplus.com', 'ADMIN@PETCAREPLUS.COM', 1, 
                 '{passwordHash}', 'admin-security-stamp', 'admin-user-concurrency', 0, 0, 1, 0, 
                 'System Administrator', 0);
            ");

            // Assign Admin role to the admin user
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO AspNetUserRoles (UserId, RoleId)
                VALUES ('admin-user-guid', 'admin-role-guid');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove admin user and role assignments
            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserRoles WHERE UserId = 'admin-user-guid';
                DELETE FROM AspNetUsers WHERE Id = 'admin-user-guid';
                DELETE FROM AspNetRoles WHERE Id IN ('admin-role-guid', 'vet-role-guid', 'owner-role-guid');
            ");
        }
    }
}