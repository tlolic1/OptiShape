// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OptiShape.Models;
using OptiShape.Data;


namespace OptiShape.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
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

            [Required(ErrorMessage = "Polje 'Studentski status' je obavezno.")]
            [Display(Name = "Da li korisnik ima studentski status?")]
            public bool StudentskiStatus { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            // Modificirajte validaciju lozinke da bude samo ograničenje dužine
            [Required(ErrorMessage = "Polje 'Šifra' je obavezno.")]
            [StringLength(100, ErrorMessage = "Šifra mora imati između {2} i {1} karaktera.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Šifra")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potvrdite šifru")]
            [Compare("Password", ErrorMessage = "Šifra i potvrda šifre se ne podudaraju.")]
            public string ConfirmPassword { get; set; }

            // Dodajte ovo polje u InputModel klasu
            [Required(ErrorMessage = "Polje 'Broj telefona' je obavezno.")]
            [Phone(ErrorMessage = "Unesite ispravan broj telefona.")]
            [Display(Name = "Broj telefona")]
            public string BrojTelefona { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Set custom ApplicationUser properties
                user.Ime = Input.Ime;
                user.Prezime = Input.Prezime;
                user.DatumRodjenja = Input.DatumRodjenja;
                user.Visina = Input.Visina;
                user.Tezina = Input.Tezina;
                user.Spol = Input.Spol;
                user.Cilj = Input.Cilj;
                user.StudentskiStatus = Input.StudentskiStatus;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add user to "Korisnik" role
                    await _userManager.AddToRoleAsync(user, "Korisnik");

                    // Kreiranje zapisa u Korisnik tabeli sa podacima iz ApplicationUser
                    var korisnik = new Korisnik
                    {
                        Ime = user.Ime,
                        Prezime = user.Prezime,
                        Email = user.Email,
                        Sifra = Input.Password,
                        DatumRodjenja = user.DatumRodjenja,
                        Visina = user.Visina,
                        Tezina = user.Tezina,
                        Spol = user.Spol,
                        Cilj = user.Cilj,
                        StudentskiStatus = user.StudentskiStatus,
                        BrojTelefona = Input.BrojTelefona
                    };

                    _context.Korisnik.Add(korisnik);
                    await _context.SaveChangesAsync();

                    // Generate automatic personalized nutrition plan
                    await CreateInitialPlan(user, korisnik);

                    // Ostatak originalnog koda
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Potvrdite svoju email adresu",
                        $"Molimo potvrdite vaš račun klikom <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>ovdje</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = "/Home/Dashboard" });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect("/Home/Dashboard");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        /// <summary>
        /// Creates an initial personalized nutrition and training plan for a new user
        /// </summary>
        private async Task CreateInitialPlan(ApplicationUser user, Korisnik korisnik)
        {
            // Calculate age based on date of birth
            int age = DateTime.Now.Year - user.DatumRodjenja.Year;
            if (DateTime.Now.DayOfYear < user.DatumRodjenja.DayOfYear)
                age--;

            // Calculate BMR (Basal Metabolic Rate) using Mifflin-St Jeor Equation
            double bmr;
            if (user.Spol == Spol.MUSKO)
            {
                bmr = 10 * user.Tezina + 6.25 * user.Visina - 5 * age + 5;
            }
            else
            {
                bmr = 10 * user.Tezina + 6.25 * user.Visina - 5 * age - 161;
            }

            // Adjust calories based on goal
            int totalCalories;
            switch (user.Cilj)
            {
                case Cilj.MRSANJE:
                    totalCalories = (int)(bmr * 0.85); // 15% calorie deficit for weight loss
                    break;
                case Cilj.NABIJANJEMISICNEMASE:
                    totalCalories = (int)(bmr * 1.15); // 15% calorie surplus for muscle gain
                    break;
                default:
                    totalCalories = (int)bmr; // Maintenance by default
                    break;
            }

            // Calculate macronutrients based on goal
            int protein, carbs, fats;

            if (user.Cilj == Cilj.MRSANJE)
            {
                // Higher protein, moderate fat, lower carbs for fat loss
                protein = (int)(user.Tezina * 2.2); // 2.2g per kg bodyweight
                fats = (int)((totalCalories * 0.25) / 9); // 25% of calories from fat (9 cal/g)
                carbs = (int)((totalCalories - (protein * 4) - (fats * 9)) / 4); // Remaining calories from carbs (4 cal/g)
            }
            else // NABIJANJEMISICNEMASE
            {
                // High protein, higher carbs, moderate fat for muscle gain
                protein = (int)(user.Tezina * 2.0); // 2.0g per kg bodyweight
                fats = (int)((totalCalories * 0.20) / 9); // 20% of calories from fat
                carbs = (int)((totalCalories - (protein * 4) - (fats * 9)) / 4); // Remaining calories from carbs
            }

            // Ensure we don't get negative values due to calculation rounding
            carbs = Math.Max(carbs, 30);
            protein = Math.Max(protein, 50);
            fats = Math.Max(fats, 20);

            // Create the plan
            var plan = new PlanIshraneTreninga
            {
                IdKorisnika = korisnik.IdKorisnika,
                DatumKreiranja = DateTime.Now,
                Kalorije = totalCalories,
                Protein = protein,
                Ugljikohidrati = carbs,
                Masti = fats
            };

            _context.PlanIshraneTreninga.Add(plan);
            await _context.SaveChangesAsync();
        }
    }
}