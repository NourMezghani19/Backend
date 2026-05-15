using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalleId",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Salles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Capacite = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Disponible = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SalleId",
                table: "Sessions",
                column: "SalleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Salles_SalleId",
                table: "Sessions",
                column: "SalleId",
                principalTable: "Salles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Salles_SalleId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Salles");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SalleId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SalleId",
                table: "Sessions");
        }
    }
}
