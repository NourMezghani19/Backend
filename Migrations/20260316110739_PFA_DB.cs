using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class PFA_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    genre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    IdSalleSport = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Taille = table.Column<float>(type: "real", nullable: true),
                    Poids = table.Column<float>(type: "real", nullable: true),
                    PhotoProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInscription = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Telephone",
                table: "Utilisateurs",
                column: "Telephone",
                unique: true,
                filter: "[Telephone] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
