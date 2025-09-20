using Koop.Entity.Entities;
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
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            builder.Entity<RouteVehicleQueue>()
                .HasIndex(q => new { q.RouteId, q.VehicleId })
                .IsUnique();

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }

            


    }
}
