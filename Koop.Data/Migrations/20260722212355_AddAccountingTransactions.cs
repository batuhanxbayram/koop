using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingTransactions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountingTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingTransactions_CreatedByUserId",
                table: "AccountingTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingTransactions_UserId_TransactionDate_Id",
                table: "AccountingTransactions",
                columns: new[] { "UserId", "TransactionDate", "Id" });

            migrationBuilder.Sql("""
                ;WITH FirstMonthlySummary AS (
                    SELECT
                        s.*,
                        ROW_NUMBER() OVER (
                            PARTITION BY s.UserId, s.VehicleId
                            ORDER BY s.PeriodYear, s.PeriodMonth, s.Id
                        ) AS RowNumber
                    FROM AccountingMonthlySummaries s
                    WHERE s.UserId IS NOT NULL
                )
                INSERT INTO AccountingTransactions
                    (UserId, TransactionDate, Description, TransactionType, Amount, CreatedByUserId, CreatedAt, UpdatedAt)
                SELECT
                    UserId,
                    DATEFROMPARTS(PeriodYear, PeriodMonth, 1),
                    CONCAT(N'Acilis bakiyesi', CASE WHEN PlateNumber IS NULL OR PlateNumber = N'' THEN N'' ELSE CONCAT(N' - ', PlateNumber) END),
                    CASE WHEN PreviousBalance >= 0 THEN 1 ELSE 2 END,
                    ABS(PreviousBalance),
                    CreatedByUserId,
                    CreatedAt,
                    UpdatedAt
                FROM FirstMonthlySummary
                WHERE RowNumber = 1 AND PreviousBalance <> 0;

                INSERT INTO AccountingTransactions
                    (UserId, TransactionDate, Description, TransactionType, Amount, CreatedByUserId, CreatedAt, UpdatedAt)
                SELECT
                    UserId,
                    EOMONTH(DATEFROMPARTS(PeriodYear, PeriodMonth, 1)),
                    CONCAT(COALESCE(NULLIF(Description, N''), N'Aylik cari ozet'), CASE WHEN PlateNumber IS NULL OR PlateNumber = N'' THEN N'' ELSE CONCAT(N' - ', PlateNumber) END, N' - Alacak'),
                    1,
                    IncomeAmount,
                    CreatedByUserId,
                    CreatedAt,
                    UpdatedAt
                FROM AccountingMonthlySummaries
                WHERE UserId IS NOT NULL AND IncomeAmount > 0;

                INSERT INTO AccountingTransactions
                    (UserId, TransactionDate, Description, TransactionType, Amount, CreatedByUserId, CreatedAt, UpdatedAt)
                SELECT
                    UserId,
                    EOMONTH(DATEFROMPARTS(PeriodYear, PeriodMonth, 1)),
                    CONCAT(COALESCE(NULLIF(Description, N''), N'Aylik cari ozet'), CASE WHEN PlateNumber IS NULL OR PlateNumber = N'' THEN N'' ELSE CONCAT(N' - ', PlateNumber) END, N' - Borc'),
                    2,
                    ExpenseAmount,
                    CreatedByUserId,
                    CreatedAt,
                    UpdatedAt
                FROM AccountingMonthlySummaries
                WHERE UserId IS NOT NULL AND ExpenseAmount > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingTransactions");
        }
    }
}
