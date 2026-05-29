using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAIPlatform.Services.Documents.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentContentForLocalSandbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                schema: "documents",
                table: "ClaimDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UploadedAtUtc",
                schema: "documents",
                table: "ClaimDocuments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadedByActor",
                schema: "documents",
                table: "ClaimDocuments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                schema: "documents",
                table: "ClaimDocuments");

            migrationBuilder.DropColumn(
                name: "UploadedAtUtc",
                schema: "documents",
                table: "ClaimDocuments");

            migrationBuilder.DropColumn(
                name: "UploadedByActor",
                schema: "documents",
                table: "ClaimDocuments");
        }
    }
}
