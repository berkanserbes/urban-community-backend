using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportSimilarityMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SimilarityHash",
                table: "Reports",
                newName: "ContentHash");

            migrationBuilder.CreateTable(
                name: "ReportSimilarityMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimilarReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimilarityScore = table.Column<double>(type: "double precision", nullable: false),
                    Reasoning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSimilarityMatches", x => x.Id);
                    table.CheckConstraint("CK_ReportSimilarityMatches_DifferentReports", "\"ReportId\" <> \"SimilarReportId\"");
                    table.CheckConstraint("CK_ReportSimilarityMatches_Score_Range", "\"SimilarityScore\" >= 0 AND \"SimilarityScore\" <= 1");
                    table.ForeignKey(
                        name: "FK_ReportSimilarityMatches_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportSimilarityMatches_Reports_SimilarReportId",
                        column: x => x.SimilarReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ContentHash",
                table: "Reports",
                column: "ContentHash");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSimilarityMatches_ReportId_SimilarReportId",
                table: "ReportSimilarityMatches",
                columns: new[] { "ReportId", "SimilarReportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportSimilarityMatches_SimilarReportId",
                table: "ReportSimilarityMatches",
                column: "SimilarReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportSimilarityMatches");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ContentHash",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "ContentHash",
                table: "Reports",
                newName: "SimilarityHash");
        }
    }
}
