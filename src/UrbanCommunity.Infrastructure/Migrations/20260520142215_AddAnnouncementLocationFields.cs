using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "Announcements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                table: "Announcements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "Announcements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_DistrictId",
                table: "Announcements",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_ProvinceId",
                table: "Announcements",
                column: "ProvinceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Districts_DistrictId",
                table: "Announcements",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Provinces_ProvinceId",
                table: "Announcements",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Districts_DistrictId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Provinces_ProvinceId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_DistrictId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_ProvinceId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Announcements");
        }
    }
}
