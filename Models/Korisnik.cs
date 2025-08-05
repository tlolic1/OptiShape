using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptiShape.Models
{
    public class Korisnik
    {
        [Key]
        public int IdKorisnika { get; set; }

        [Required(ErrorMessage = "Polje 'Ime korisnika' je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora sadržavati između 2 i 50 karaktera.")]
        [RegularExpression("[a-zA-Z]+", ErrorMessage = "Ime može sadržavati samo slova.")]
        [DisplayName("Ime korisnika")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Polje 'Prezime korisnika' je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora sadržavati između 2 i 50 karaktera.")]
        [RegularExpression("[a-zA-Z]+", ErrorMessage = "Prezime može sadržavati samo slova.")]
        [DisplayName("Prezime korisnika")]
        public string Prezime { get; set; }

        [Required(ErrorMessage = "Polje 'Email adresa' je obavezno.")]
        [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
        [DisplayName("Email adresa")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Polje 'Šifra' je obavezno.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Šifra mora imati najmanje 6 karaktera.")]
        [DisplayName("Šifra")]
        public string Sifra { get; set; }

        [Required(ErrorMessage = "Polje 'Datum rođenja' je obavezno.")]
        [DataType(DataType.Date)]
        [DisplayName("Datum rođenja")]
        public DateTime DatumRodjenja { get; set; }

        [Required(ErrorMessage = "Polje 'Visina' je obavezno.")]
        [Range(100, 250, ErrorMessage = "Visina mora biti između 100 i 250 cm.")]
        [DisplayName("Visina (cm)")]
        public double Visina { get; set; }

        [Required(ErrorMessage = "Polje 'Težina' je obavezno.")]
        [Range(30, 200, ErrorMessage = "Težina mora biti između 30 i 200 kg.")]
        [DisplayName("Težina (kg)")]
        public double Tezina { get; set; }

        [Required(ErrorMessage = "Polje 'Spol' je obavezno.")]
        [EnumDataType(typeof(Spol), ErrorMessage = "Odaberite spol.")]
        [DisplayName("Spol")]
        public Spol Spol { get; set; }

        [Required(ErrorMessage = "Polje 'Cilj' je obavezno.")]
        [EnumDataType(typeof(Cilj), ErrorMessage = "Odaberite cilj.")]
        [DisplayName("Cilj")]
        public Cilj Cilj { get; set; }

        [Required(ErrorMessage = "Polje 'Studentski status' je obavezno.")]
        [DisplayName("Da li korisnik ima studentski status?")]
        public bool StudentskiStatus { get; set; }

        [Required(ErrorMessage = "Polje 'Broj telefona' je obavezno.")]
        [Phone(ErrorMessage = "Unesite ispravan broj telefona.")]
        [DisplayName("Broj telefona")]
        public string BrojTelefona { get; set; }

        public int? IdTrenera { get; set; }

        [ForeignKey("IdTrenera")]
        [ValidateNever]
        public Korisnik? Trener { get; set; }

        


    }
}