using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaginationSortIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuggestionComments_SuggestionId",
                table: "SuggestionComments");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AssignedOrganizationDepartmentId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_UserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_ReportComments_ReportId",
                table: "ReportComments");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPostComments_CommunityPostId",
                table: "CommunityPostComments");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_VoteScore_CreatedAt",
                table: "Suggestions",
                columns: new[] { "VoteScore", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionComments_SuggestionId_CreatedAt",
                table: "SuggestionComments",
                columns: new[] { "SuggestionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AssignedOrganizationDepartmentId_CreatedAt",
                table: "Reports",
                columns: new[] { "AssignedOrganizationDepartmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_NeighborhoodId_CreatedAt",
                table: "Reports",
                columns: new[] { "NeighborhoodId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId_CreatedAt",
                table: "Reports",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_ReportId_CreatedAt",
                table: "ReportComments",
                columns: new[] { "ReportId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPostComments_CommunityPostId_CreatedAt",
                table: "CommunityPostComments",
                columns: new[] { "CommunityPostId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suggestions_VoteScore_CreatedAt",
                table: "Suggestions");

            migrationBuilder.DropIndex(
                name: "IX_SuggestionComments_SuggestionId_CreatedAt",
                table: "SuggestionComments");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AssignedOrganizationDepartmentId_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_NeighborhoodId_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_UserId_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_ReportComments_ReportId_CreatedAt",
                table: "ReportComments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPostComments_CommunityPostId_CreatedAt",
                table: "CommunityPostComments");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionComments_SuggestionId",
                table: "SuggestionComments",
                column: "SuggestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AssignedOrganizationDepartmentId",
                table: "Reports",
                column: "AssignedOrganizationDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_ReportId",
                table: "ReportComments",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPostComments_CommunityPostId",
                table: "CommunityPostComments",
                column: "CommunityPostId");
        }
    }
}
