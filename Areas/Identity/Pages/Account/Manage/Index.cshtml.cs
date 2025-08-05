// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OptiShape.Data;
using OptiShape.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace OptiShape.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Phone]
            [Display(Name = "Broj telefona")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Polje 'Ime korisnika' je obavezno.")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora sadržavati između 2 i 50 karaktera.")]
            [RegularExpression("[a-zA-Z]+", ErrorMessage = "Ime može sadržavati samo slova.")]
            [Display(Name = "Ime korisnika")]
            public string Ime { get; set; }

            [Required(ErrorMessage = "Polje 'Prezime korisnika' je obavezno.")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora sadržavati između 2 i 50 karaktera.")]
            [RegularExpression("[a-zA-Z]+", ErrorMessage = "Prezime može sadržavati samo slova.")]
            [Display(Name = "Prezime korisnika")]
            public string Prezime { get; set; }

            [Required(ErrorMessage = "Polje 'Datum rođenja' je obavezno.")]
            [DataType(DataType.Date)]
            [Display(Name = "Datum rođenja")]
            public DateTime DatumRodjenja { get; set; }

            [Required(ErrorMessage = "Polje 'Visina' je obavezno.")]
            [Range(100, 250, ErrorMessage = "Visina mora biti između 100 i 250 cm.")]
            [Display(Name = "Visina (cm)")]
            public double Visina { get; set; }

            [Required(ErrorMessage = "Polje 'Težina' je obavezno.")]
            [Range(30, 200, ErrorMessage = "Težina mora biti između 30 i 200 kg.")]
            [Display(Name = "Težina (kg)")]
            public double Tezina { get; set; }

            [Required(ErrorMessage = "Polje 'Spol' je obavezno.")]
            [EnumDataType(typeof(Spol), ErrorMessage = "Odaberite spol.")]
            [Display(Name = "Spol")]
            public Spol Spol { get; set; }

            [Required(ErrorMessage = "Polje 'Cilj' je obavezno.")]
            [EnumDataType(typeof(Cilj), ErrorMessage = "Odaberite cilj.")]
            [Display(Name = "Cilj")]
            public Cilj Cilj { get; set; }

            [Display(Name = "Da li korisnik ima studentski status?")]
            public bool StudentskiStatus { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            // Dohvati korisnika iz Korisnik tabele na osnovu email adrese
            var dbContext = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var korisnik = await dbContext.Korisnik.FirstOrDefaultAsync(k => k.Email == user.Email);

            Input = new InputModel
            {
                PhoneNumber = korisnik.BrojTelefona,
                // Podaci iz ApplicationUser modela
                Ime = user.Ime,
                Prezime = user.Prezime,
                DatumRodjenja = user.DatumRodjenja,
                Visina = user.Visina,
                Tezina = user.Tezina,
                Spol = user.Spol,
                Cilj = user.Cilj,
                StudentskiStatus = user.StudentskiStatus
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nije moguće učitati korisnika s ID-om '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nije moguće učitati korisnika s ID-om '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Neočekivana greška pri postavljanju broja telefona.";
                    return RedirectToPage();
                }
            }

            // Ažuriranje ostalih korisničkih podataka u ApplicationUser
            user.Ime = Input.Ime;
            user.Prezime = Input.Prezime;
            user.DatumRodjenja = Input.DatumRodjenja;
            user.Visina = Input.Visina;
            user.Tezina = Input.Tezina;
            user.Spol = Input.Spol;
            user.Cilj = Input.Cilj;
            user.StudentskiStatus = Input.StudentskiStatus;

            await _userManager.UpdateAsync(user);

            // Ažuriranje podataka i u Korisnik tabeli
            var dbContext = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var korisnik = await dbContext.Korisnik.FirstOrDefaultAsync(k => k.Email == user.Email);

            if (korisnik != null)
            {
                korisnik.Ime = Input.Ime;
                korisnik.Prezime = Input.Prezime;
                korisnik.DatumRodjenja = Input.DatumRodjenja;
                korisnik.Visina = Input.Visina;
                korisnik.Tezina = Input.Tezina;
                korisnik.Spol = Input.Spol;
                korisnik.Cilj = Input.Cilj;
                korisnik.StudentskiStatus = Input.StudentskiStatus;
                korisnik.BrojTelefona = Input.PhoneNumber;

                await dbContext.SaveChangesAsync();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Vaš profil je uspješno ažuriran";
            return RedirectToPage();
        }
    }
}
