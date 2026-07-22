using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingMonthlySummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingMonthlySummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VehicleId = table.Column<long>(type: "bigint", nullable: true),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodMonth = table.Column<int>(type: "int", nullable: false),
                    PeriodYear = table.Column<int>(type: "int", nullable: false),
                    PreviousBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingMonthlySummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingMonthlySummaries_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AccountingMonthlySummaries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AccountingMonthlySummaries_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMonthlySummaries_CreatedByUserId",
                table: "AccountingMonthlySummaries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMonthlySummaries_PeriodYear_PeriodMonth",
                table: "AccountingMonthlySummaries",
                columns: new[] { "PeriodYear", "PeriodMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMonthlySummaries_UserId_VehicleId_PeriodYear_PeriodMonth",
                table: "AccountingMonthlySummaries",
                columns: new[] { "UserId", "VehicleId", "PeriodYear", "PeriodMonth" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [VehicleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingMonthlySummaries_VehicleId",
                table: "AccountingMonthlySummaries",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingMonthlySummaries");
        }
    }
}
