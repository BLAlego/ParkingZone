using Microsoft.EntityFrameworkCore;
using ParkingZone.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ParkingZone.Data
{
    public class ParkingZoneContext : DbContext
    {
        public ParkingZoneContext(DbContextOptions<ParkingZoneContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Space> Spaces => Set<Space>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Parking> Parking => Set<Parking>();
        public DbSet<PlateScan> PlateScans => Set<PlateScan>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // USERS
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(10);
                e.HasCheckConstraint("CK_users_role",
                    "[role] IN ('client','worker','admin')");
            });

            // SPACES
            modelBuilder.Entity<Space>(e =>
            {
                e.ToTable("spaces");
                e.HasIndex(s => s.Code).IsUnique();
                e.Property(s => s.Type)
                    .HasConversion<string>()
                    .HasMaxLength(12);
                e.HasCheckConstraint("CK_spaces_type",
                    "[type] IN ('car','motorcycle','pickup')");
                e.Property(s => s.HourlyRate).HasColumnType("decimal(10,2)");
            });

            // VEHICLES
            modelBuilder.Entity<Vehicle>(e =>
            {
                e.ToTable("vehicles");
                e.HasIndex(v => v.Plate).IsUnique();
                e.Property(v => v.Type)
                    .HasConversion<string>()
                    .HasMaxLength(12);
                e.HasCheckConstraint("CK_vehicles_type",
                    "[type] IN ('car','motorcycle','pickup')");
                e.HasOne(v => v.User)
                    .WithMany(u => u.Vehicles)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RESERVATIONS
            modelBuilder.Entity<Reservation>(e =>
            {
                e.ToTable("reservations");
                e.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(10);
                e.HasCheckConstraint("CK_reservations_status",
                    "[status] IN ('active','finished','cancelled')");
                e.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(r => r.Space)
                    .WithMany(s => s.Reservations)
                    .HasForeignKey(r => r.SpaceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PAYMENTS
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("payments");
                e.Property(p => p.TotalAmount).HasColumnType("decimal(10,2)");
                e.Property(p => p.Method)
                    .HasConversion<string>()
                    .HasMaxLength(10);
                e.HasCheckConstraint("CK_payments_method",
                    "[method] IN ('cash','card','transfer')");
                e.HasOne(p => p.Reservation)
                    .WithMany(r => r.Payments)
                    .HasForeignKey(p => p.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PARKING (Composite PK)
            modelBuilder.Entity<Parking>(e =>
            {
                e.ToTable("Parking"); // tal cual tu nombre de tabla
                e.HasKey(p => new { p.SpaceId, p.VehicleId, p.EntryTime });
                e.Property(p => p.Profit).HasColumnType("decimal(10,2)");
                e.HasOne(p => p.Space)
                    .WithMany(s => s.Parkings)
                    .HasForeignKey(p => p.SpaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Vehicle)
                    .WithMany(v => v.Parkings)
                    .HasForeignKey(p => p.VehicleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
