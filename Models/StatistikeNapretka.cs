using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OptiShape.Models
{
    public class StatistikeNapretka
    {
        [Key]
        public int IdZapisa { get; set; }

        [Required(ErrorMessage = "Datum je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum unosa")]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Težina je obavezna.")]
        [Range(30, 200, ErrorMessage = "Težina mora biti između 30 i 200 kg.")]
        [Display(Name = "Težina (kg)")]
        public double Tezina { get; set; }

        [Required(ErrorMessage = "BMI je obavezan.")]
        [Range(0, 100, ErrorMessage = "BMI mora biti između 10 i 60.")]
        [Display(Name = "BMI")]
        public double Bmi { get; set; }

        [Required(ErrorMessage = "Kalorijski unos je obavezan.")]
        [Display(Name = "Kalorijski unos")]
        public int KalorijskiUnos { get; set; }

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Odaberite korisnika.")]
        [Display(Name = "Korisnik")]
        public int IdKorisnika { get; set; }

        [ValidateNever]
        public Korisnik? Korisnik { get; set; }
    }
}