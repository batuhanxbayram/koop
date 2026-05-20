using Koop.Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace Koop.Data.Context
{
    public class AppDbContext: IdentityDbContext<AppUser, AppRole, Guid>
    {
        public DbSet<Route> Routes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<RouteVehicleQueue> RouteVehicleQueues { get; set; }
        public DbSet<AccountingRecord> AccountingRecords { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Route>()
            .HasIndex(r => r.RouteName)
            .IsUnique();

            builder.Entity<Vehicle>()
                .HasOne(v => v.AppUser)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.AppUserId)
                .IsRequired(false);

            builder.Entity<RouteVehicleQueue>()
                .HasIndex(q => new { q.RouteId, q.VehicleId })
                .IsUnique();

            builder.Entity<AccountingRecord>()
                .HasOne(a => a.Vehicle)
                .WithMany(v => v.AccountingRecords)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccountingRecord>()
                .HasOne(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AccountingRecord>()
                .HasIndex(a => new { a.VehicleId, a.Date });

            
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


            
        

    }

            


    }
}
