using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeOrganizationDepartmentModelAndAddNeighborhoodOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Departments_AssignedDepartmentId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AssignedDepartmentId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "UX_Organizations_DistrictScope",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "UX_Organizations_ProvinceScope",
                table: "Organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organizations_Jurisdiction",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AssignedDepartmentId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "NeighborhoodId",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_NeighborhoodId",
                table: "Organizations",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_DistrictScope",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId", "DistrictId" },
                unique: true,
                filter: "\"DistrictId\" IS NOT NULL AND \"NeighborhoodId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_NeighborhoodScope",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId", "DistrictId", "NeighborhoodId" },
                unique: true,
                filter: "\"NeighborhoodId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Organizations_ProvinceScope",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId" },
                unique: true,
                filter: "\"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organizations_Jurisdiction",
                table: "Organizations",
                sql: "(\"JurisdictionLevel\" = 1 AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR (\"JurisdictionLevel\" = 2 AND \"DistrictId\" IS NOT NULL AND \"NeighborhoodId\" IS NULL) OR (\"JurisdictionLevel\" = 3 AND \"DistrictId\" IS NOT NULL AND \"NeighborhoodId\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers",
                sql: "(\"Role\" = 2 AND \"OrganizationDepartmentId\" IS NOT NULL AND ((\"OfficialJurisdictionLevel\" = 1 AND \"OfficialProvinceId\" IS NOT NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 2 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NOT NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 3 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NOT NULL))) OR (\"Role\" <> 2 AND \"OrganizationDepartmentId\" IS NULL AND \"OfficialJurisdictionLevel\" IS NULL AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Neighborhoods_NeighborhoodId",
                table: "Organizations",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Neighborhoods_NeighborhoodId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_NeighborhoodId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "UX_Organizations_DistrictScope",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "UX_Organizations_NeighborhoodScope",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "UX_Organizations_ProvinceScope",
                table: "Organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organizations_Jurisdiction",
                table: "Organizations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "Organizations");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedDepartmentId",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Departments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Departments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AssignedDepartmentId",
                table: "Reports",
                column: "AssignedDepartmentId");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organizations_Jurisdiction",
                table: "Organizations",
                sql: "(\"JurisdictionLevel\" = 1 AND \"DistrictId\" IS NULL) OR (\"JurisdictionLevel\" = 2 AND \"DistrictId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers",
                sql: "(\"Role\" = 2 AND (\"OrganizationDepartmentId\" IS NOT NULL OR \"DepartmentId\" IS NOT NULL) AND ((\"OfficialJurisdictionLevel\" = 1 AND \"OfficialProvinceId\" IS NOT NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 2 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NOT NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 3 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NOT NULL))) OR (\"Role\" <> 2 AND \"OrganizationDepartmentId\" IS NULL AND \"DepartmentId\" IS NULL AND \"OfficialJurisdictionLevel\" IS NULL AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Departments_AssignedDepartmentId",
                table: "Reports",
                column: "AssignedDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
