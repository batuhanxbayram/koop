using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koop.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAccountingDeleteBehaviors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_CreatedByUserId",
                table: "AccountingMonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_UserId",
                table: "AccountingMonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingMonthlySummaries_Vehicles_VehicleId",
                table: "AccountingMonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_CreatedByUserId",
                table: "AccountingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_UserId",
                table: "AccountingTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_CreatedByUserId",
                table: "AccountingMonthlySummaries",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_UserId",
                table: "AccountingMonthlySummaries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingMonthlySummaries_Vehicles_VehicleId",
                table: "AccountingMonthlySummaries",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_CreatedByUserId",
                table: "AccountingTransactions",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_UserId",
                table: "AccountingTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_CreatedByUserId",
                table: "AccountingMonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_UserId",
                table: "AccountingMonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingMonthlySummaries_Vehicles_VehicleId",
                table: "AccountingMonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_CreatedByUserId",
                table: "AccountingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_UserId",
                table: "AccountingTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_CreatedByUserId",
                table: "AccountingMonthlySummaries",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingMonthlySummaries_AspNetUsers_UserId",
                table: "AccountingMonthlySummaries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingMonthlySummaries_Vehicles_VehicleId",
                table: "AccountingMonthlySummaries",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_CreatedByUserId",
                table: "AccountingTransactions",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingTransactions_AspNetUsers_UserId",
                table: "AccountingTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
