using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.AiAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysisRunStructuredFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardrailFlagsJson",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyExplanationText",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedActionJson",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryText",
                schema: "ai_analysis",
                table: "AiAnalysisRuns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "GuardrailFlagsJson",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "ModelName",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "PolicyExplanationText",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "RecommendedActionJson",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");

            migrationBuilder.DropColumn(
                name: "SummaryText",
                schema: "ai_analysis",
                table: "AiAnalysisRuns");
        }
    }
}
