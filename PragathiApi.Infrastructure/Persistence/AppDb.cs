using Microsoft.EntityFrameworkCore;
using PragathiApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragathiApi.Infrastructure.Persistence
{
    public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
    {
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<RoomConfiguration> Configurations => Set<RoomConfiguration>();
        public DbSet<LeadActivity> Activities => Set<LeadActivity>();
        public DbSet<Visit> Visits => Set<Visit>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Lead>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                e.Property(x => x.Email).HasMaxLength(200);
                e.HasIndex(x => x.Phone).IsUnique();               // one person per phone number
            });

            b.Entity<RoomConfiguration>(e =>
            {
                e.ToTable("Configurations");
                e.Property(x => x.Name).HasMaxLength(60).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
                e.HasData(                                          // the starting list of choices
                    new RoomConfiguration { Id = 1, Name = "2 BHK Apartment", SortOrder = 1 },
                    new RoomConfiguration { Id = 2, Name = "3 BHK Apartment", SortOrder = 2 },
                    new RoomConfiguration { Id = 3, Name = "Sky Villa (Triplex)", SortOrder = 3 },
                    new RoomConfiguration { Id = 4, Name = "Garden Villa (Duplex)", SortOrder = 4 },
                    new RoomConfiguration { Id = 5, Name = "Corner Villa", SortOrder = 5 });
            });

            b.Entity<LeadActivity>(e =>
            {
                e.ToTable("Activities");
                e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);   // stored as readable text
                e.Property(x => x.Message).HasMaxLength(2000);
                e.Property(x => x.Detail).HasMaxLength(200);

                e.HasOne(x => x.Lead)
                    .WithMany(l => l.Activities)
                    .HasForeignKey(x => x.LeadId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Configuration)
                    .WithMany()
                    .HasForeignKey(x => x.ConfigurationId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(x => new { x.LeadId, x.Type, x.CreatedAt });   // fast "did they just do this?" check
            });

            b.Entity<Visit>(e =>
            {
                e.Property(x => x.TimeSlot).HasMaxLength(20).IsRequired();
                e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

                e.HasOne(x => x.Activity)
                    .WithOne(a => a.Visit)
                    .HasForeignKey<Visit>(x => x.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
