using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialJurisdiction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Neighborhoods_Districts_DistrictId",
                table: "Neighborhoods");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_DepartmentId_Requires_OfficialRole",
                table: "AspNetUsers");

            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS pgcrypto;

                WITH source_provinces AS (
                    SELECT DISTINCT
                        COALESCE(NULLIF(BTRIM("City"), ''), 'Unknown Province') AS "Name"
                    FROM "Neighborhoods"
                )
                INSERT INTO "Provinces" ("Id", "Name", "CreatedAt", "UpdatedAt")
                SELECT gen_random_uuid(), source_provinces."Name", NOW(), NULL
                FROM source_provinces
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "Provinces"
                    WHERE "Provinces"."Name" = source_provinces."Name"
                );

                WITH source_districts AS (
                    SELECT DISTINCT
                        COALESCE(NULLIF(BTRIM("Neighborhoods"."District"), ''), 'Unknown District') AS "DistrictName",
                        "Provinces"."Id" AS "ProvinceId"
                    FROM "Neighborhoods"
                    INNER JOIN "Provinces"
                        ON "Provinces"."Name" = COALESCE(NULLIF(BTRIM("Neighborhoods"."City"), ''), 'Unknown Province')
                )
                INSERT INTO "Districts" ("Id", "Name", "ProvinceId", "CreatedAt", "UpdatedAt")
                SELECT gen_random_uuid(), source_districts."DistrictName", source_districts."ProvinceId", NOW(), NULL
                FROM source_districts
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "Districts"
                    WHERE "Districts"."Name" = source_districts."DistrictName"
                      AND "Districts"."ProvinceId" = source_districts."ProvinceId"
                );

                UPDATE "Neighborhoods"
                SET "DistrictId" = "Districts"."Id"
                FROM "Districts"
                INNER JOIN "Provinces"
                    ON "Provinces"."Id" = "Districts"."ProvinceId"
                WHERE COALESCE(NULLIF(BTRIM("Neighborhoods"."District"), ''), 'Unknown District') = "Districts"."Name"
                  AND COALESCE(NULLIF(BTRIM("Neighborhoods"."City"), ''), 'Unknown Province') = "Provinces"."Name"
                  AND (
                      "Neighborhoods"."DistrictId" IS NULL
                      OR NOT EXISTS (
                          SELECT 1
                          FROM "Districts" existing_districts
                          WHERE existing_districts."Id" = "Neighborhoods"."DistrictId"
                      )
                  );
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "DistrictId",
                table: "Neighborhoods",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficialDistrictId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OfficialJurisdictionLevel",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficialNeighborhoodId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficialProvinceId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OfficialDistrictId",
                table: "AspNetUsers",
                column: "OfficialDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OfficialJurisdictionLevel_OfficialDistrictId",
                table: "AspNetUsers",
                columns: new[] { "OfficialJurisdictionLevel", "OfficialDistrictId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OfficialJurisdictionLevel_OfficialNeighborhoodId",
                table: "AspNetUsers",
                columns: new[] { "OfficialJurisdictionLevel", "OfficialNeighborhoodId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OfficialJurisdictionLevel_OfficialProvinceId",
                table: "AspNetUsers",
                columns: new[] { "OfficialJurisdictionLevel", "OfficialProvinceId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OfficialNeighborhoodId",
                table: "AspNetUsers",
                column: "OfficialNeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OfficialProvinceId",
                table: "AspNetUsers",
                column: "OfficialProvinceId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers",
                sql: "(\"Role\" = 2 AND \"DepartmentId\" IS NOT NULL AND ((\"OfficialJurisdictionLevel\" = 1 AND \"OfficialProvinceId\" IS NOT NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 2 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NOT NULL AND \"OfficialNeighborhoodId\" IS NULL) OR (\"OfficialJurisdictionLevel\" = 3 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NOT NULL))) OR (\"Role\" <> 2 AND \"DepartmentId\" IS NULL AND \"OfficialJurisdictionLevel\" IS NULL AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Districts_OfficialDistrictId",
                table: "AspNetUsers",
                column: "OfficialDistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Neighborhoods_OfficialNeighborhoodId",
                table: "AspNetUsers",
                column: "OfficialNeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Provinces_OfficialProvinceId",
                table: "AspNetUsers",
                column: "OfficialProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Neighborhoods_Districts_DistrictId",
                table: "Neighborhoods",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Districts_OfficialDistrictId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Neighborhoods_OfficialNeighborhoodId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Provinces_OfficialProvinceId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Neighborhoods_Districts_DistrictId",
                table: "Neighborhoods");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OfficialDistrictId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OfficialJurisdictionLevel_OfficialDistrictId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OfficialJurisdictionLevel_OfficialNeighborhoodId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OfficialJurisdictionLevel_OfficialProvinceId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OfficialNeighborhoodId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OfficialProvinceId",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_OfficialAccess",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OfficialDistrictId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OfficialJurisdictionLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OfficialNeighborhoodId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OfficialProvinceId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "DistrictId",
                table: "Neighborhoods",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_DepartmentId_Requires_OfficialRole",
                table: "AspNetUsers",
                sql: "\"DepartmentId\" IS NULL OR \"Role\" = 2");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Neighborhoods_Districts_DistrictId",
                table: "Neighborhoods",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");
        }
    }
}
