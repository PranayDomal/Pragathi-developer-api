using Microsoft.EntityFrameworkCore;
using Pragathi.Admin.Domain.Entities;

namespace Pragathi.Admin.Infrastructure.Data;

public class AdminDb : DbContext
{
    public AdminDb(DbContextOptions<AdminDb> options) : base(options) { }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AdminUser>(e =>
        {
            e.ToTable("AdminUsers");
            e.Property(x => x.LoginId).HasMaxLength(60).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(300).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);   // saved as "Viewer", "Editor", ...
            e.HasIndex(x => x.LoginId).IsUnique();                              // no two users with the same ID
        });
    }
}