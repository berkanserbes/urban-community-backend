using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOrganizationScopeUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name_JurisdictionLevel_ProvinceId_DistrictId",
                table: "Organizations");

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_DistrictScope",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId", "DistrictId" },
                unique: true,
                filter: "\"DistrictId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_ProvinceScope",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId" },
                unique: true,
                filter: "\"DistrictId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Organizations_DistrictScope",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "UX_Organizations_ProvinceScope",
                table: "Organizations");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name_JurisdictionLevel_ProvinceId_DistrictId",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId", "DistrictId" },
                unique: true);
        }
    }
}
