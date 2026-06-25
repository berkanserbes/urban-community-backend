using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationsAndOfficialOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedOrganizationDepartmentId",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AssignmentNeedsReview",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationDepartmentId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JurisdictionLevel = table.Column<int>(type: "integer", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.CheckConstraint("CK_Organizations_Jurisdiction", "(\"JurisdictionLevel\" = 1 AND \"DistrictId\" IS NULL) OR (\"JurisdictionLevel\" = 2 AND \"DistrictId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Organizations_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organizations_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationDepartmentCategories",
                columns: table => new
                {
                    OrganizationDepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportCategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationDepartmentCategories", x => new { x.OrganizationDepartmentId, x.ReportCategoryId });
                    table.ForeignKey(
                        name: "FK_OrganizationDepartmentCategories_OrganizationDepartments_Or~",
                        column: x => x.OrganizationDepartmentId,
                        principalTable: "OrganizationDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationDepartmentCategories_ReportCategories_ReportCat~",
                        column: x => x.ReportCategoryId,
                        principalTable: "ReportCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AssignedOrganizationDepartmentId",
                table: "Reports",
                column: "AssignedOrganizationDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AssignmentNeedsReview",
                table: "Reports",
                column: "AssignmentNeedsReview");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationDepartmentId",
                table: "AspNetUsers",
                column: "OrganizationDepartmentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers",
                sql: "(\"Role\" = 2 AND (\"OrganizationDepartmentId\" IS NOT NULL OR \"DepartmentId\" IS NOT NULL) AND ((\"OfficialJurisdictionLevel\" = 1 AND \"OfficialProvinceId\" IS NOT NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 2 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NOT NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 3 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NOT NULL))) OR (\"Role\" <> 2 AND \"OrganizationDepartmentId\" IS NULL AND \"DepartmentId\" IS NULL AND \"OfficialJurisdictionLevel\" IS NULL AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartmentCategories_ReportCategoryId",
                table: "OrganizationDepartmentCategories",
                column: "ReportCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartments_DepartmentId",
                table: "OrganizationDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationDepartments_OrganizationId_DepartmentId",
                table: "OrganizationDepartments",
                columns: new[] { "OrganizationId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DistrictId",
                table: "Organizations",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name_JurisdictionLevel_ProvinceId_DistrictId",
                table: "Organizations",
                columns: new[] { "Name", "JurisdictionLevel", "ProvinceId", "DistrictId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ProvinceId",
                table: "Organizations",
                column: "ProvinceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_OrganizationDepartments_OrganizationDepartmentId",
                table: "AspNetUsers",
                column: "OrganizationDepartmentId",
                principalTable: "OrganizationDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_OrganizationDepartments_AssignedOrganizationDepartm~",
                table: "Reports",
                column: "AssignedOrganizationDepartmentId",
                principalTable: "OrganizationDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_OrganizationDepartments_OrganizationDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_OrganizationDepartments_AssignedOrganizationDepartm~",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "OrganizationDepartmentCategories");

            migrationBuilder.DropTable(
                name: "OrganizationDepartments");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AssignedOrganizationDepartmentId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AssignmentNeedsReview",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrganizationDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AssignedOrganizationDepartmentId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "AssignmentNeedsReview",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers",
                sql: "(\"Role\" = 2 AND \"DepartmentId\" IS NOT NULL AND ((\"OfficialJurisdictionLevel\" = 1 AND \"OfficialProvinceId\" IS NOT NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 2 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NOT NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 3 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NOT NULL))) OR (\"Role\" <> 2 AND \"DepartmentId\" IS NULL AND \"OfficialJurisdictionLevel\" IS NULL AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL)");
        }
    }
}
