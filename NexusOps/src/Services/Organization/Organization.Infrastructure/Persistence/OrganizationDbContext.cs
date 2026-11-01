using Microsoft.EntityFrameworkCore;
using Organization.Domain.Entities;

namespace Organization.Infrastructure.Persistence;

public class OrganizationDbContext : DbContext
{
    public OrganizationDbContext(
        DbContextOptions<OrganizationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureDepartment(modelBuilder);
        ConfigureTeam(modelBuilder);
        ConfigureEmployee(modelBuilder);
    }

    private static void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(d => d.Description)
                .HasMaxLength(500);

            entity.HasIndex(d => d.Name)
                .IsUnique();

            entity.HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(d => d.Teams)
                .WithOne(t => t.Department)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(t => t.Description)
                .HasMaxLength(500);

            entity.HasIndex(t => new { t.DepartmentId, t.Name })
                .IsUnique();

            entity.HasOne(t => t.TeamLead)
                .WithMany()
                .HasForeignKey(t => t.TeamLeadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(t => t.Employees)
                .WithOne(e => e.Team)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.IdentityUserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.EmployeeNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.JobTitle)
                .HasMaxLength(150);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(e => e.WorkEmail)
                .HasMaxLength(256);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasIndex(e => e.IdentityUserId)
                .IsUnique();

            entity.HasIndex(e => e.EmployeeNumber)
                .IsUnique();

            entity.HasIndex(e => e.WorkEmail)
                .IsUnique()
                .HasFilter("\"WorkEmail\" IS NOT NULL");

            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}