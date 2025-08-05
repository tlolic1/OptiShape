using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OptiShape.Models
{
    public class Termin
    {
        [Key]
        public int IdTermina { get; set; }

        [Required(ErrorMessage = "Datum termina je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum termina")]
        public DateTime Datum { get; set; }

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Odaberite korisnika.")]
        [Display(Name = "Korisnik")]
        public int IdKorisnika { get; set; }

        [ValidateNever]
        public Korisnik? Korisnik { get; set; }

        [NotMapped]
        [Display(Name = "Vrijeme početka")]
        [DataType(DataType.Time)]
        public TimeSpan VrijemeOd { get; set; }


        [NotMapped]
        [Display(Name = "Vrijeme završetka")]
        public TimeSpan VrijemeDo => VrijemeOd.Add(TimeSpan.FromHours(1));

    }
}