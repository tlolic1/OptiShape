using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiShape.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptiShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Korisnik",
                columns: table => new
                {
                    IdKorisnika = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sifra = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumRodjenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Visina = table.Column<double>(type: "float", nullable: false),
                    Tezina = table.Column<double>(type: "float", nullable: false),
                    Spol = table.Column<int>(type: "int", nullable: false),
                    Cilj = table.Column<int>(type: "int", nullable: false),
                    StudentskiStatus = table.Column<bool>(type: "bit", nullable: false),
                    BrojTelefona = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnik", x => x.IdKorisnika);
                });

            migrationBuilder.CreateTable(
                name: "Placanje",
                columns: table => new
                {
                    IdPlacanja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumPlacanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Iznos = table.Column<double>(type: "float", nullable: false),
                    PopustPrimijenjen = table.Column<bool>(type: "bit", nullable: false),
                    NacinPlacanja = table.Column<int>(type: "int", nullable: false),
                    IdKorisnika = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placanje", x => x.IdPlacanja);
                    table.ForeignKey(
                        name: "FK_Placanje_Korisnik_IdKorisnika",
                        column: x => x.IdKorisnika,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanIshraneTreninga",
                columns: table => new
                {
                    IdPlana = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kalorije = table.Column<int>(type: "int", nullable: false),
                    Protein = table.Column<int>(type: "int", nullable: false),
                    Ugljikohidrati = table.Column<int>(type: "int", nullable: false),
                    Masti = table.Column<int>(type: "int", nullable: false),
                    IdKorisnika = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanIshraneTreninga", x => x.IdPlana);
                    table.ForeignKey(
                        name: "FK_PlanIshraneTreninga_Korisnik_IdKorisnika",
                        column: x => x.IdKorisnika,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatistikeNapretka",
                columns: table => new
                {
                    IdZapisa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tezina = table.Column<double>(type: "float", nullable: false),
                    Bmi = table.Column<double>(type: "float", nullable: false),
                    KalorijskiUnos = table.Column<int>(type: "int", nullable: false),
                    IdKorisnika = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatistikeNapretka", x => x.IdZapisa);
                    table.ForeignKey(
                        name: "FK_StatistikeNapretka_Korisnik_IdKorisnika",
                        column: x => x.IdKorisnika,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Termin",
                columns: table => new
                {
                    IdTermina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdKorisnika = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Termin", x => x.IdTermina);
                    table.ForeignKey(
                        name: "FK_Termin_Korisnik_IdKorisnika",
                        column: x => x.IdKorisnika,
                        principalTable: "Korisnik",
                        principalColumn: "IdKorisnika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Placanje_IdKorisnika",
                table: "Placanje",
                column: "IdKorisnika");

            migrationBuilder.CreateIndex(
                name: "IX_PlanIshraneTreninga_IdKorisnika",
                table: "PlanIshraneTreninga",
                column: "IdKorisnika");

            migrationBuilder.CreateIndex(
                name: "IX_StatistikeNapretka_IdKorisnika",
                table: "StatistikeNapretka",
                column: "IdKorisnika");

            migrationBuilder.CreateIndex(
                name: "IX_Termin_IdKorisnika",
                table: "Termin",
                column: "IdKorisnika");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Placanje");

            migrationBuilder.DropTable(
                name: "PlanIshraneTreninga");

            migrationBuilder.DropTable(
                name: "StatistikeNapretka");

            migrationBuilder.DropTable(
                name: "Termin");

            migrationBuilder.DropTable(
                name: "Korisnik");
        }
    }
}
