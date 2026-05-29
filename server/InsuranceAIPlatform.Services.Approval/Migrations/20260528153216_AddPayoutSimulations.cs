using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.Approval.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutSimulations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayoutSimulations",
                schema: "approval",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Deductible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPayoutAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DecisionSource = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DecisionActor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceAiRunId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SimulationOnly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutSimulations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutSimulations_ClaimId",
                schema: "approval",
                table: "PayoutSimulations",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutSimulations_Status",
                schema: "approval",
                table: "PayoutSimulations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutSimulations",
                schema: "approval");
        }
    }
}
