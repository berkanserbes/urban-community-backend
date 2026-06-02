using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityPostScopeLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "NeighborhoodId",
                table: "CommunityPosts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "CommunityPosts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500);

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "CommunityPosts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                table: "CommunityPosts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "CommunityPosts",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_DistrictId",
                table: "CommunityPosts",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_ProvinceId",
                table: "CommunityPosts",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_Scope_DistrictId_Status",
                table: "CommunityPosts",
                columns: new[] { "Scope", "DistrictId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_Scope_NeighborhoodId_Status",
                table: "CommunityPosts",
                columns: new[] { "Scope", "NeighborhoodId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_Scope_ProvinceId_Status",
                table: "CommunityPosts",
                columns: new[] { "Scope", "ProvinceId", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommunityPosts_Scope_Location",
                table: "CommunityPosts",
                sql: "(\"Scope\" = 1 AND \"ProvinceId\" IS NOT NULL AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR (\"Scope\" = 2 AND \"DistrictId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR (\"Scope\" = 3 AND \"NeighborhoodId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"DistrictId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityPosts_Districts_DistrictId",
                table: "CommunityPosts",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityPosts_Provinces_ProvinceId",
                table: "CommunityPosts",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityPosts_Districts_DistrictId",
                table: "CommunityPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityPosts_Provinces_ProvinceId",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_DistrictId",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_ProvinceId",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_Scope_DistrictId_Status",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_Scope_NeighborhoodId_Status",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_Scope_ProvinceId_Status",
                table: "CommunityPosts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CommunityPosts_Scope_Location",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "CommunityPosts");

            migrationBuilder.AlterColumn<Guid>(
                name: "NeighborhoodId",
                table: "CommunityPosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "CommunityPosts",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }
    }
}
