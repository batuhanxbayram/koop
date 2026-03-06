using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class _111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("8887f80e-c437-4e02-9654-3e736ebfd67e"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c7523f28-cc68-4aa7-9356-1fa9fed71cff"));

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpireTime", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("c2ae243e-7dec-4119-8690-68df77a864fc"), 0, "1b051173-d14b-4735-927a-04aa06e65534", "ahmet.yilmaz@example.com", true, "Ahmet Yılmaz", false, null, "AHMET.YILMAZ@EXAMPLE.COM", "AHMET.YILMAZ", "AQAAAAIAAYagAAAAEGfD0TmH8QCCh5tk1E4KFLzncBOptnHTClOWn26COXZXsg/flpQ8/HLROnG572KCiQ==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "e5c6e4e2-453b-4a08-8f53-5996ca9a08af", false, "ahmet.yilmaz" },
                    { new Guid("dd9534a4-0709-416d-9fcf-865a9589bf44"), 0, "281bbca7-4f8e-475c-baa5-c24658157c0e", "ayse.kaya@example.com", true, "Ayşe Kaya", false, null, "AYSE.KAYA@EXAMPLE.COM", "AYSE.KAYA", "AQAAAAIAAYagAAAAENS00t6A8FaogqtB1OYO1jWOn8ZS+rzdcFQ6fFSvWyRqYkluz130Rn1s/00B2f21IQ==", null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "885a9c58-1bb0-4b12-9e14-4f0897e7357d", false, "ayse.kaya" }
                });

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 1L,
                column: "QueueTimestamp",
                value: new DateTime(2026, 3, 5, 22, 8, 29, 814, DateTimeKind.Utc).AddTicks(4812));

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 2L,
                column: "QueueTimestamp",
                value: new DateTime(2026, 3, 5, 23, 8, 29, 814, DateTimeKind.Utc).AddTicks(4819));

            migrationBuilder.UpdateData(
                table: "RouteVehicleQueues",
                keyColumn: "Id",
                keyValue: 3L,
                column: "QueueTimestamp",
                value: new DateTime(2026, 3, 6, 0, 8, 29, 814, DateTimeKind.Utc).AddTicks(4821));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "AppUserId",
                value: new Guid("c2ae243e-7dec-4119-8690-68df77a864fc"));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "AppUserId",
                value: new Guid("dd9534a4-0709-416d-9fcf-865a9589bf44"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c2ae243e-7dec-4119-8690-68df77a864fc"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("dd9534a4-0709-416d-9fcf-865a9589bf44"));

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
                column: "AppUserId",
                value: new Guid("c7523f28-cc68-4aa7-9356-1fa9fed71cff"));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "AppUserId",
                value: new Guid("8887f80e-c437-4e02-9654-3e736ebfd67e"));
        }
    }
}
