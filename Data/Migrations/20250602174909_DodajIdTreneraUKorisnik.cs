using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiShape.Data.Migrations
{
    /// <inheritdoc />
    public partial class DodajIdTreneraUKorisnik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTrenera",
                table: "Korisnik",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnik_IdTrenera",
                table: "Korisnik",
                column: "IdTrenera");

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnik_Korisnik_IdTrenera",
                table: "Korisnik",
                column: "IdTrenera",
                principalTable: "Korisnik",
                principalColumn: "IdKorisnika");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Korisnik_Korisnik_IdTrenera",
                table: "Korisnik");

            migrationBuilder.DropIndex(
                name: "IX_Korisnik_IdTrenera",
                table: "Korisnik");

            migrationBuilder.DropColumn(
                name: "IdTrenera",
                table: "Korisnik");
        }
    }
}
