using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OptiShape.Models
{
    public class Placanje
    {
        [Key]
        public int IdPlacanja { get; set; }

        [Required(ErrorMessage = "Datum plaćanja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum plaćanja")]
        public DateTime DatumPlacanja { get; set; }

        [Required(ErrorMessage = "Iznos je obavezan.")]
        [Range(0.01, 10000, ErrorMessage = "Iznos mora biti veći od 0.")]
        [Display(Name = "Iznos")]
        public double Iznos { get; set; }

        [Display(Name = "Popust primijenjen")]
        public bool PopustPrimijenjen { get; set; }

        [Required(ErrorMessage = "Način plaćanja je obavezan.")]
        [Display(Name = "Način plaćanja")]
        public NacinPlacanja NacinPlacanja { get; set; }

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Odaberite korisnika.")]
        [Display(Name = "Korisnik")]
        public int IdKorisnika { get; set; }

        [ValidateNever]
        public Korisnik? Korisnik { get; set; }
    }
}