using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.AiAnalysis.Migrations
{
    /// <inheritdoc />
    public partial class AddRagFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidenceChunks",
                schema: "ai_analysis",
                columns: table => new
                {
                    ChunkId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DocumentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    ChunkHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    SourceVersion = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    EmbeddingModel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EmbeddingDim = table.Column<int>(type: "int", nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceChunks", x => x.ChunkId);
                });

            migrationBuilder.CreateTable(
                name: "PolicyClauses",
                schema: "ai_analysis",
                columns: table => new
                {
                    ClauseId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PolicyId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClauseType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyClauses", x => x.ClauseId);
                });

            migrationBuilder.CreateTable(
                name: "RagAuditTraces",
                schema: "ai_analysis",
                columns: table => new
                {
                    TraceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UseCase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    QueryText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetrievedChunkIdsCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CitationsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    ProviderMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PromptTokens = table.Column<int>(type: "int", nullable: false),
                    CompletionTokens = table.Column<int>(type: "int", nullable: false),
                    CostMicros = table.Column<long>(type: "bigint", nullable: false),
                    RetrievalMs = table.Column<long>(type: "bigint", nullable: false),
                    AdvisoryOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagAuditTraces", x => x.TraceId);
                });

            migrationBuilder.CreateTable(
                name: "RagEvaluationQuestions",
                schema: "ai_analysis",
                columns: table => new
                {
                    QuestionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClaimId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UseCase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ExpectedSourceChunkIdsCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MustNotCiteChunkIdsCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedAnswerKeywordsCsv = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagEvaluationQuestions", x => x.QuestionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceChunks_ClaimId",
                schema: "ai_analysis",
                table: "EvidenceChunks",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyClauses_ProductCode",
                schema: "ai_analysis",
                table: "PolicyClauses",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_RagAuditTraces_ClaimId",
                schema: "ai_analysis",
                table: "RagAuditTraces",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_RagEvaluationQuestions_ClaimId",
                schema: "ai_analysis",
                table: "RagEvaluationQuestions",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceChunks",
                schema: "ai_analysis");

            migrationBuilder.DropTable(
                name: "PolicyClauses",
                schema: "ai_analysis");

            migrationBuilder.DropTable(
                name: "RagAuditTraces",
                schema: "ai_analysis");

            migrationBuilder.DropTable(
                name: "RagEvaluationQuestions",
                schema: "ai_analysis");
        }
    }
}
