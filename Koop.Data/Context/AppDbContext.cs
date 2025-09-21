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

                builder.Entity<AppUser>()
                    .HasOne(u => u.Vehicle)          // Bir AppUser'ın bir aracı vardır
                    .WithOne(v => v.AppUser)         // Bu araç da bir AppUser'a aittir
                    .HasForeignKey<Vehicle>(v => v.AppUserId);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            // IdentityUser için parola hash'leme
            var passwordHasher = new PasswordHasher<AppUser>();

            var user1 = new AppUser
            {
                Id = user1Id,
                FullName = "Ahmet Yılmaz",
                UserName = "ahmet.yilmaz",
                NormalizedUserName = "AHMET.YILMAZ",
                Email = "ahmet.yilmaz@example.com",
                NormalizedEmail = "AHMET.YILMAZ@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "Sifre123!"), // Lütfen güçlü bir şifre kullanın
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var user2 = new AppUser
            {
                Id = user2Id,
                FullName = "Ayşe Kaya",
                UserName = "ayse.kaya",
                NormalizedUserName = "AYSE.KAYA",
                Email = "ayse.kaya@example.com",
                NormalizedEmail = "AYSE.KAYA@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null, "Sifre456!"), // Lütfen güçlü bir şifre kullanın
                SecurityStamp = Guid.NewGuid().ToString()
            };

            builder.Entity<AppUser>().HasData(user1, user2);

            // --- ROTA (ROUTE) SEED ---
            builder.Entity<Route>().HasData(
                new Route { Id = 1, RouteName = "Kuzey Hattı", IsActive = true },
                new Route { Id = 2, RouteName = "Güney Hattı", IsActive = true },
                new Route { Id = 3, RouteName = "Doğu-Batı Ring", IsActive = false }
            );

            // --- ARAÇ (VEHICLE) SEED ---
            builder.Entity<Vehicle>().HasData(
                new Vehicle
                {
                    Id = 1,
                    AppUserId = user1Id, // İlk kullanıcıya bağlandı
                    LicensePlate = "34 ABC 123",
                    DriverName = "Ahmet Yılmaz",
                    PhoneNumber = "5551112233",
                    IsActive = true
                },
                new Vehicle
                {
                    Id = 2,
                    AppUserId = user2Id, // İkinci kullanıcıya bağlandı
                    LicensePlate = "35 DEF 456",
                    DriverName = "Mehmet Öztürk",
                    PhoneNumber = "5554445566",
                    IsActive = true
                }
            );

            // --- ROTA-ARAÇ SIRASI (ROUTEVEHICLEQUEUE) SEED ---
            builder.Entity<RouteVehicleQueue>().HasData(
                new RouteVehicleQueue
                {
                    Id = 1,
                    RouteId = 1, // Kuzey Hattı
                    VehicleId = 1, // 34 ABC 123
                    QueueTimestamp = DateTime.UtcNow.AddHours(-2) // 2 saat önce sıraya girmiş
                },
                new RouteVehicleQueue
                {
                    Id = 2,
                    RouteId = 1, // Kuzey Hattı
                    VehicleId = 2, // 35 DEF 456
                    QueueTimestamp = DateTime.UtcNow.AddHours(-1) // 1 saat önce sıraya girmiş
                },
                new RouteVehicleQueue
                {
                    Id = 3,
                    RouteId = 2, // Güney Hattı
                    VehicleId = 1, // 34 ABC 123
                    QueueTimestamp = DateTime.UtcNow // Şimdi sıraya girmiş
                }
            );
        

    }

            


    }
}
