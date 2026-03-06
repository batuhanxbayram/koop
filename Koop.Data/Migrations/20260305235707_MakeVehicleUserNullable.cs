using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeVehicleUserNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_AppUserId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_AppUserId",
                table: "Vehicles");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("1410bcb8-deec-475f-8db4-31c538f9c3a4"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c62ec918-bda2-49ee-a9c3-98bd2a6fa34b"));

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId1",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpireTime", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("8887f80e-c437-4e02-9654-3e736ebfd67e"), 0, "d1a90276-ccbe-447c-b341-d0ee8b47d455", "ayse.kaya@example.com", true, "Ayşe Kaya", false, null, "AYSE.KAYA@EXAMPLE.COM", "AYSE.KAYA", "AQAAAAIAAYagAAAAEIHSVx3FNsVhl83k3R1as2wqRjDy01BM4AO2MRwt4U2ooinMcjf1yJXCczlpW8YR9g==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "4f6f0d56-67ff-463d-adb1-d64ea1ec713c", false, "ayse.kaya" },
                    { new Guid("c7523f28-cc68-4aa7-9356-1fa9fed71cff"), 0, "1acb2615-3017-4e89-97e1-da061bf92249", "ahmet.yilmaz@example.com", true, "Ahmet Yılmaz", false, null, "AHMET.YILMAZ@EXAMPLE.COM", "AHMET.YILMAZ", "AQAAAAIAAYagAAAAED2POlR88vUsjqaRcX3dUKO89gab4cPhBxAZbtq4/zEsNbVkUHcQZJmxdYLc7zwThg==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1d605d60-3f00-49d3-a627-d926ad61bfed", false, "ahmet.yilmaz" }
                });

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 1L,
                column: "QueueTimestamp",
                value: new DateTime(2026, 3, 5, 21, 57, 7, 330, DateTimeKind.Utc).AddTicks(3174));

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 2L,
                column: "QueueTimestamp",
                value: new DateTime(2026, 3, 5, 22, 57, 7, 330, DateTimeKind.Utc).AddTicks(3181));

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 3L,
                column: "QueueTimestamp",
                value: new DateTime(2026, 3, 5, 23, 57, 7, 330, DateTimeKind.Utc).AddTicks(3182));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "AppUserId", "AppUserId1" },
                values: new object[] { new Guid("c7523f28-cc68-4aa7-9356-1fa9fed71cff"), null });

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "AppUserId", "AppUserId1" },
                values: new object[] { new Guid("8887f80e-c437-4e02-9654-3e736ebfd67e"), null });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_AppUserId",
                table: "Vehicles",
                column: "AppUserId",
                unique: true,
                filter: "[AppUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_AppUserId1",
                table: "Vehicles",
                column: "AppUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_AppUserId",
                table: "Vehicles",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_AppUserId1",
                table: "Vehicles",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_AppUserId",
                table: "Vehicles");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_AppUserId1",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_AppUserId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_AppUserId1",
                table: "Vehicles");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("8887f80e-c437-4e02-9654-3e736ebfd67e"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c7523f28-cc68-4aa7-9356-1fa9fed71cff"));

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "Vehicles");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "Vehicles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpireTime", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("1410bcb8-deec-475f-8db4-31c538f9c3a4"), 0, "a50f0218-4e18-4530-9f24-654ba7d91403", "ayse.kaya@example.com", true, "Ayşe Kaya", false, null, "AYSE.KAYA@EXAMPLE.COM", "AYSE.KAYA", "AQAAAAIAAYagAAAAEJXgW14WJfVKzsIscwcAITIJzW8bCXSmoKIylrL2lqt2ft6Q8XBir1vnA36XhXNOFQ==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "508bb090-546f-43af-87ad-05c24c6701b0", false, "ayse.kaya" },
                    { new Guid("c62ec918-bda2-49ee-a9c3-98bd2a6fa34b"), 0, "4590ae97-78e4-4808-aece-70580353ffbd", "ahmet.yilmaz@example.com", true, "Ahmet Yılmaz", false, null, "AHMET.YILMAZ@EXAMPLE.COM", "AHMET.YILMAZ", "AQAAAAIAAYagAAAAEMpn8uoCWp/bMSRdzDdF+reUrLnUiRkXnZ8Fq3OVcSnk+0lKn8E3ZsBI6Y6l1jxJNg==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "749cc26c-4c57-4ec3-8641-4721626a5bb8", false, "ahmet.yilmaz" }
                });

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 1L,
                column: "QueueTimestamp",
                value: new DateTime(2025, 9, 21, 6, 0, 13, 715, DateTimeKind.Utc).AddTicks(1847));

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 2L,
                column: "QueueTimestamp",
                value: new DateTime(2025, 9, 21, 7, 0, 13, 715, DateTimeKind.Utc).AddTicks(1855));

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 3L,
                column: "QueueTimestamp",
                value: new DateTime(2025, 9, 21, 8, 0, 13, 715, DateTimeKind.Utc).AddTicks(1857));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "AppUserId",
                value: new Guid("c62ec918-bda2-49ee-a9c3-98bd2a6fa34b"));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "AppUserId",
                value: new Guid("1410bcb8-deec-475f-8db4-31c538f9c3a4"));

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_AppUserId",
                table: "Vehicles",
                column: "AppUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_AppUserId",
                table: "Vehicles",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
