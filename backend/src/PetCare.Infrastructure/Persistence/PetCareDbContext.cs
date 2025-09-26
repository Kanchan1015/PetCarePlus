using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Pets;
using PetCare.Infrastructure.Auth;

namespace PetCare.Infrastructure.Persistence;

public class PetCareDbContext : IdentityDbContext<ApplicationUser>
{
    public PetCareDbContext(DbContextOptions<PetCareDbContext> options) : base(options) {}

    public DbSet<Pet> Pets => Set<Pet>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser config
        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FullName)
                .HasMaxLength(128)
                .IsRequired();

            // Unique Email (normalized email is already indexed by Identity;
            // we also enforce a unique Email for our API semantics)
            b.HasIndex(u => u.Email).IsUnique();
        });

        // Pet config
        builder.Entity<Pet>(b =>
        {
            b.Property(p => p.Name).HasMaxLength(60).IsRequired();
            b.Property(p => p.Species).HasConversion<int>().IsRequired();
            b.Property(p => p.Breed).HasMaxLength(60);
            b.Property(p => p.Color).HasMaxLength(30);
            b.Property(p => p.Weight).HasColumnType("decimal(5,2)");
            b.Property(p => p.MedicalNotes).HasMaxLength(1000);
            b.Property(p => p.IsActive).HasDefaultValue(true);
            b.Property(p => p.CreatedAt).IsRequired();

            // Owner FK (string)
            b.HasIndex(p => p.OwnerUserId);
            b.HasOne<ApplicationUser>()
                .WithMany() // No navigation property to avoid circular dependencies
                .HasForeignKey(p => p.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict); // keep pets if user is disabled
        });
    }
}
