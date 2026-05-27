using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.AuditCost.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxAndCommandAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                schema: "audit_cost",
                table: "AuditEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Actor",
                schema: "audit_cost",
                table: "AuditEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                schema: "audit_cost",
                table: "AuditEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                schema: "audit_cost",
                table: "AuditEvents",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OccurredAtUtc",
                schema: "audit_cost",
                table: "AuditEvents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "audit_cost",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_IdempotencyKey",
                schema: "audit_cost",
                table: "OutboxMessages",
                column: "IdempotencyKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "audit_cost");

            migrationBuilder.DropColumn(
                name: "ActionType",
                schema: "audit_cost",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "Actor",
                schema: "audit_cost",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                schema: "audit_cost",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                schema: "audit_cost",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "OccurredAtUtc",
                schema: "audit_cost",
                table: "AuditEvents");
        }
    }
}
