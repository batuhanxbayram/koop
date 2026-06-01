using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    WaybillNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QuantityKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceEffect = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingRecords_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountingRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRecords_CreatedByUserId",
                table: "AccountingRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRecords_VehicleId_Date",
                table: "AccountingRecords",
                columns: new[] { "VehicleId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingRecords");
        }
    }
}
