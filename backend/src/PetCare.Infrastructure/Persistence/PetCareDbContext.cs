using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Pets;
using PetCare.Domain.MedicalRecords;
using PetCare.Domain.Appointments; // Sprint Addition: Appointment entities
using PetCare.Domain.Notifications; // Sprint Addition: Notification entities
using PetCare.Infrastructure.Auth;
using PetCare.Domain.Inventory;

namespace PetCare.Infrastructure.Persistence;

public class PetCareDbContext : IdentityDbContext<ApplicationUser>
{
    public PetCareDbContext(DbContextOptions<PetCareDbContext> options) : base(options) {}

    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();
    public DbSet<Treatment> Treatments => Set<Treatment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    
    // Sprint Addition: Appointment and Notification DbSets
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<InventoryItem> InventoryItems { get; set; }

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

        // MedicalRecord config
        builder.Entity<MedicalRecord>(b =>
        {
            b.Property(m => m.Title).HasMaxLength(200).IsRequired();
            b.Property(m => m.Description).HasMaxLength(2000);
            b.Property(m => m.Notes).HasMaxLength(1000);
            b.Property(m => m.RecordType).HasConversion<int>().IsRequired();
            b.Property(m => m.CreatedAt).IsRequired();

            // Pet FK
            b.HasIndex(m => m.PetId);
            b.HasOne(m => m.Pet)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vet FK (string)
            b.HasIndex(m => m.VetUserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.VetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Vaccination config
        builder.Entity<Vaccination>(b =>
        {
            b.Property(v => v.VaccineName).HasMaxLength(100).IsRequired();
            b.Property(v => v.BatchNumber).HasMaxLength(50);
            b.Property(v => v.Manufacturer).HasMaxLength(100);
            b.Property(v => v.Notes).HasMaxLength(1000);
            b.Property(v => v.Status).HasConversion<int>().IsRequired();
            b.Property(v => v.CreatedAt).IsRequired();

            // Pet FK
            b.HasIndex(v => v.PetId);
            b.HasOne(v => v.Pet)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(v => v.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vet FK (string)
            b.HasIndex(v => v.VetUserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(v => v.VetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Treatment config
        builder.Entity<Treatment>(b =>
        {
            b.Property(t => t.TreatmentType).HasMaxLength(100).IsRequired();
            b.Property(t => t.Diagnosis).HasMaxLength(500).IsRequired();
            b.Property(t => t.Treatment_Description).HasMaxLength(2000);
            b.Property(t => t.Medications).HasMaxLength(1000);
            b.Property(t => t.Instructions).HasMaxLength(2000);
            b.Property(t => t.Notes).HasMaxLength(1000);
            b.Property(t => t.Status).HasConversion<int>().IsRequired();
            b.Property(t => t.CreatedAt).IsRequired();

            // Pet FK
            b.HasIndex(t => t.PetId);
            b.HasOne(t => t.Pet)
                .WithMany(p => p.Treatments)
                .HasForeignKey(t => t.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vet FK (string)
            b.HasIndex(t => t.VetUserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(t => t.VetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Prescription config
        builder.Entity<Prescription>(b =>
        {
            b.Property(p => p.MedicationName).HasMaxLength(100).IsRequired();
            b.Property(p => p.Dosage).HasMaxLength(100).IsRequired();
            b.Property(p => p.Frequency).HasMaxLength(100).IsRequired();
            b.Property(p => p.Instructions).HasMaxLength(2000);
            b.Property(p => p.Notes).HasMaxLength(1000);
            b.Property(p => p.Status).HasConversion<int>().IsRequired();
            b.Property(p => p.CreatedAt).IsRequired();

            // Pet FK
            b.HasIndex(p => p.PetId);
            b.HasOne(p => p.Pet)
                .WithMany(pet => pet.Prescriptions)
                .HasForeignKey(p => p.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vet FK (string)
            b.HasIndex(p => p.VetUserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.VetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Sprint Addition: Appointment entity configuration
        builder.Entity<Appointment>(b =>
        {
            b.Property(a => a.ReasonForVisit).HasMaxLength(200).IsRequired();
            b.Property(a => a.Notes).HasMaxLength(1000);
            b.Property(a => a.AdminNotes).HasMaxLength(1000);
            b.Property(a => a.Status).HasConversion<int>().IsRequired();
            b.Property(a => a.CreatedAt).IsRequired();

            // Pet FK (Guid) - following existing pattern
            b.HasIndex(a => a.PetId);
            b.HasOne(a => a.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Owner FK (string) - following existing pattern
            b.HasIndex(a => a.OwnerUserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vet FK (string, optional) - following existing pattern
            b.HasIndex(a => a.VetUserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.VetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for common queries
            b.HasIndex(a => a.Status);
            b.HasIndex(a => a.RequestedDateTime);
            b.HasIndex(a => new { a.OwnerUserId, a.Status }); // Owner's appointments by status
        });

        // Sprint Addition: Notification entity configuration
        builder.Entity<Notification>(b =>
        {
            b.Property(n => n.Title).HasMaxLength(200).IsRequired();
            b.Property(n => n.Message).HasMaxLength(1000).IsRequired();
            b.Property(n => n.Data).HasMaxLength(2000); // JSON data
            b.Property(n => n.Type).HasConversion<int>().IsRequired();
            b.Property(n => n.CreatedAt).IsRequired();

            // User FK (string) - following existing pattern
            b.HasIndex(n => n.UserId);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for common queries
            b.HasIndex(n => new { n.UserId, n.IsRead }); // User's unread notifications
            b.HasIndex(n => n.CreatedAt); // Recent notifications
        });

        // InventoryItem configuration
        builder.Entity<InventoryItem>(b =>
        {
            b.Property(i => i.Name).HasMaxLength(200).IsRequired();
            b.Property(i => i.Category).HasMaxLength(100).IsRequired();
            b.Property(i => i.Supplier).HasMaxLength(200).IsRequired();
            b.Property(i => i.Quantity).IsRequired();
            b.Property(i => i.ExpiryDate); // Already nullable, no .IsRequired()
            b.Property(i => i.Description).HasMaxLength(1000);

            // Indexes for common queries
            b.HasIndex(i => i.Name);
            b.HasIndex(i => i.Category);
            b.HasIndex(i => i.Supplier);
            b.HasIndex(i => i.Quantity); // For low stock queries
        });
    }
}
