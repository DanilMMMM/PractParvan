using Microsoft.EntityFrameworkCore;
using VetClinic.Models;

namespace VetClinic.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PetType> PetTypes => Set<PetType>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Visit> Visits => Set<Visit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique index for Login and Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // Visit relations
        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Client)
            .WithMany(u => u.ClientVisits)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Veterinarian)
            .WithMany(u => u.VetVisits)
            .HasForeignKey(v => v.VeterinarianId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Pet)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Service)
            .WithMany(s => s.Visits)
            .HasForeignKey(v => v.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pet relations
        modelBuilder.Entity<Pet>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Pet>()
            .HasOne(p => p.PetType)
            .WithMany(pt => pt.Pets)
            .HasForeignKey(p => p.PetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
