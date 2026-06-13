using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementScopeLocationConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Districts_DistrictId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Neighborhoods_NeighborhoodId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Provinces_ProvinceId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_NeighborhoodId_IsActive",
                table: "Announcements");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_NeighborhoodId",
                table: "Announcements",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Scope_DistrictId_IsActive",
                table: "Announcements",
                columns: new[] { "Scope", "DistrictId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Scope_NeighborhoodId_IsActive",
                table: "Announcements",
                columns: new[] { "Scope", "NeighborhoodId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Scope_ProvinceId_IsActive",
                table: "Announcements",
                columns: new[] { "Scope", "ProvinceId", "IsActive" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Announcements_Scope_Location",
                table: "Announcements",
                sql: "(\"Scope\" = 0 AND \"ProvinceId\" IS NULL AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR (\"Scope\" = 1 AND \"ProvinceId\" IS NOT NULL AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR (\"Scope\" = 2 AND \"DistrictId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR (\"Scope\" = 3 AND \"NeighborhoodId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"DistrictId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Districts_DistrictId",
                table: "Announcements",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Neighborhoods_NeighborhoodId",
                table: "Announcements",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Provinces_ProvinceId",
                table: "Announcements",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Districts_DistrictId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Neighborhoods_NeighborhoodId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Provinces_ProvinceId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_NeighborhoodId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Scope_DistrictId_IsActive",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Scope_NeighborhoodId_IsActive",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Scope_ProvinceId_IsActive",
                table: "Announcements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Announcements_Scope_Location",
                table: "Announcements");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_NeighborhoodId_IsActive",
                table: "Announcements",
                columns: new[] { "NeighborhoodId", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Districts_DistrictId",
                table: "Announcements",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Neighborhoods_NeighborhoodId",
                table: "Announcements",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Provinces_ProvinceId",
                table: "Announcements",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id");
        }
    }
}
