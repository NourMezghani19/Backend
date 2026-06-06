using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AjoutHistoriqueEtObjectifPoids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateNaissance",
                table: "Utilisateurs",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ObjectifPoids",
                table: "Utilisateurs",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Historiques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MembreId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TypeEvenement = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AncienPoids = table.Column<float>(type: "float", nullable: true),
                    NouveauPoids = table.Column<float>(type: "float", nullable: true),
                    AncienneTaille = table.Column<float>(type: "float", nullable: true),
                    NouvelleTaille = table.Column<float>(type: "float", nullable: true),
                    AncienObjectifPoids = table.Column<float>(type: "float", nullable: true),
                    NouvelObjectifPoids = table.Column<float>(type: "float", nullable: true),
                    ImcSnapshot = table.Column<float>(type: "float", nullable: true),
                    Note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historiques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Historiques_Utilisateurs_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Historiques_MembreId",
                table: "Historiques",
                column: "MembreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Historiques");

            migrationBuilder.DropColumn(
                name: "DateNaissance",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "ObjectifPoids",
                table: "Utilisateurs");
        }
    }
}
