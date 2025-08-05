using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptiShape.Models
{
    public class PlanIshraneTreninga
    {
        [Key]
        public int IdPlana { get; set; }

        [Required(ErrorMessage = "Datum kreiranja je obavezan.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DatumKreiranja { get; set; }

        [Required(ErrorMessage = "Unesite broj kalorija.")]
        public int Kalorije { get; set; }

        [Required(ErrorMessage = "Unesite količinu proteina.")]
        public int Protein { get; set; }

        [Required(ErrorMessage = "Unesite količinu ugljikohidrata.")]
        public int Ugljikohidrati { get; set; }

        [Required(ErrorMessage = "Unesite količinu masti.")]
        public int Masti { get; set; }

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Odaberite korisnika.")]
        public int IdKorisnika { get; set; }

        [ValidateNever]
        public Korisnik? Korisnik { get; set; }


    }
}
