using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class dataseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpireTime", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("1410bcb8-deec-475f-8db4-31c538f9c3a4"), 0, "a50f0218-4e18-4530-9f24-654ba7d91403", "ayse.kaya@example.com", true, "Ayşe Kaya", false, null, "AYSE.KAYA@EXAMPLE.COM", "AYSE.KAYA", "AQAAAAIAAYagAAAAEJXgW14WJfVKzsIscwcAITIJzW8bCXSmoKIylrL2lqt2ft6Q8XBir1vnA36XhXNOFQ==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "508bb090-546f-43af-87ad-05c24c6701b0", false, "ayse.kaya" },
                    { new Guid("c62ec918-bda2-49ee-a9c3-98bd2a6fa34b"), 0, "4590ae97-78e4-4808-aece-70580353ffbd", "ahmet.yilmaz@example.com", true, "Ahmet Yılmaz", false, null, "AHMET.YILMAZ@EXAMPLE.COM", "AHMET.YILMAZ", "AQAAAAIAAYagAAAAEMpn8uoCWp/bMSRdzDdF+reUrLnUiRkXnZ8Fq3OVcSnk+0lKn8E3ZsBI6Y6l1jxJNg==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "749cc26c-4c57-4ec3-8641-4721626a5bb8", false, "ahmet.yilmaz" }
                });

            migrationBuilder.InsertData(
                table: "Routes",
                columns: new[] { "Id", "IsActive", "RouteName" },
                values: new object[,]
                {
                    { 1L, true, "Kuzey Hattı" },
                    { 2L, true, "Güney Hattı" },
                    { 3L, false, "Doğu-Batı Ring" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "AppUserId", "DriverName", "IsActive", "LicensePlate", "PhoneNumber" },
                values: new object[,]
                {
                    { 1L, new Guid("c62ec918-bda2-49ee-a9c3-98bd2a6fa34b"), "Ahmet Yılmaz", true, "34 ABC 123", "5551112233" },
                    { 2L, new Guid("1410bcb8-deec-475f-8db4-31c538f9c3a4"), "Mehmet Öztürk", true, "35 DEF 456", "5554445566" }
                });

            migrationBuilder.InsertData(
                table: "RouteVehicleQueues",
                columns: new[] { "Id", "QueueTimestamp", "RouteId", "VehicleId" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 9, 21, 6, 0, 13, 715, DateTimeKind.Utc).AddTicks(1847), 1L, 1L },
                    { 2L, new DateTime(2025, 9, 21, 7, 0, 13, 715, DateTimeKind.Utc).AddTicks(1855), 1L, 2L },
                    { 3L, new DateTime(2025, 9, 21, 8, 0, 13, 715, DateTimeKind.Utc).AddTicks(1857), 2L, 1L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Routes",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Routes",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Routes",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("1410bcb8-deec-475f-8db4-31c538f9c3a4"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c62ec918-bda2-49ee-a9c3-98bd2a6fa34b"));
        }
    }
}
