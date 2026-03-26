using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class EmploiDeTemps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmploisDuTemps_Sessions_SessionId",
                table: "EmploisDuTemps");

            migrationBuilder.DropIndex(
                name: "IX_EmploisDuTemps_SessionId",
                table: "EmploisDuTemps");

            migrationBuilder.DropColumn(
                name: "Jour",
                table: "EmploisDuTemps");

            migrationBuilder.DropColumn(
                name: "Recurrent",
                table: "EmploisDuTemps");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "EmploisDuTemps");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreeLe",
                table: "EmploisDuTemps",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "EmploisDuTemps",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "EmploisDuTemps",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreeLe",
                table: "EmploisDuTemps");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "EmploisDuTemps");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "EmploisDuTemps");

            migrationBuilder.AddColumn<int>(
                name: "Jour",
                table: "EmploisDuTemps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Recurrent",
                table: "EmploisDuTemps",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "EmploisDuTemps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmploisDuTemps_SessionId",
                table: "EmploisDuTemps",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmploisDuTemps_Sessions_SessionId",
                table: "EmploisDuTemps",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
