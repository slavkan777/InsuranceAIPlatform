using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.Approval.Migrations
{
    /// <inheritdoc />
    public partial class InitialApprovalPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "approval");

            migrationBuilder.CreateTable(
                name: "ApprovalDrafts",
                schema: "approval",
                columns: table => new
                {
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CurrentDecision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Submitted = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SavedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AiRecommendation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RecommendedPayout = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDrafts", x => x.ClaimId);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalDecisionOptions",
                schema: "approval",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Recommended = table.Column<bool>(type: "bit", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDecisionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalDecisionOptions_ApprovalDrafts_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "approval",
                        principalTable: "ApprovalDrafts",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDecisionOptions_ClaimId",
                schema: "approval",
                table: "ApprovalDecisionOptions",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalDecisionOptions",
                schema: "approval");

            migrationBuilder.DropTable(
                name: "ApprovalDrafts",
                schema: "approval");
        }
    }
}
