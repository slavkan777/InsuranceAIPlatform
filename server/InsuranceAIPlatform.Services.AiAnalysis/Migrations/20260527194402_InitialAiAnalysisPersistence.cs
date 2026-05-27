using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.AiAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class InitialAiAnalysisPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ai_analysis");

            migrationBuilder.CreateTable(
                name: "AiAnalysisRuns",
                schema: "ai_analysis",
                columns: table => new
                {
                    RunId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ProviderMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelConfidence = table.Column<int>(type: "int", nullable: false),
                    Tokens = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalysisRuns", x => x.RunId);
                });

            migrationBuilder.CreateTable(
                name: "AiEvidenceReferences",
                schema: "ai_analysis",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RunId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiEvidenceReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiEvidenceReferences_AiAnalysisRuns_RunId",
                        column: x => x.RunId,
                        principalSchema: "ai_analysis",
                        principalTable: "AiAnalysisRuns",
                        principalColumn: "RunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiFindings",
                schema: "ai_analysis",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RunId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiFindings_AiAnalysisRuns_RunId",
                        column: x => x.RunId,
                        principalSchema: "ai_analysis",
                        principalTable: "AiAnalysisRuns",
                        principalColumn: "RunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiRiskSignals",
                schema: "ai_analysis",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RunId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiRiskSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiRiskSignals_AiAnalysisRuns_RunId",
                        column: x => x.RunId,
                        principalSchema: "ai_analysis",
                        principalTable: "AiAnalysisRuns",
                        principalColumn: "RunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiEvidenceReferences_RunId",
                schema: "ai_analysis",
                table: "AiEvidenceReferences",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_AiFindings_RunId",
                schema: "ai_analysis",
                table: "AiFindings",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_AiRiskSignals_RunId",
                schema: "ai_analysis",
                table: "AiRiskSignals",
                column: "RunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiEvidenceReferences",
                schema: "ai_analysis");

            migrationBuilder.DropTable(
                name: "AiFindings",
                schema: "ai_analysis");

            migrationBuilder.DropTable(
                name: "AiRiskSignals",
                schema: "ai_analysis");

            migrationBuilder.DropTable(
                name: "AiAnalysisRuns",
                schema: "ai_analysis");
        }
    }
}
