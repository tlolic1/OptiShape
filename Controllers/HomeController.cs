using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OptiShape.Data;
using OptiShape.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace OptiShape.Controllers
{
    public class TrenerViewModel
    {
        public Korisnik Trener { get; set; }
        public int BrojKorisnika { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public IActionResult StudentApplication()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendStudentRequest(string emailTo)
        {
            try
            {
                var userName = User.Identity?.Name;
                _logger.LogInformation($"Sending student application email for user: {userName}");

                // Get the user's email from the Korisnik table
                var korisnik = await _db.Korisnik.FirstOrDefaultAsync(k => k.Email == userName);

                if (korisnik == null)
                {
                    TempData["Error"] = "Korisnik nije pronađen u bazi podataka.";
                    return RedirectToAction("StudentApplication");
                }

                var fromAddress = new MailAddress("ooooaadd1@gmail.com", "OptiShape App");
                // Use the user's email from the database instead of the entered one
                var toAddress = new MailAddress(korisnik.Email, korisnik.Ime + " " + korisnik.Prezime);

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("ooooaadd1@gmail.com", "xtkc ssht bihg mxzz")
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Zahtjev za studentski status",
                    Body = $"<h3>Zahtjev za studentski status</h3>" +
                           $"<p><strong>Korisnik:</strong> {korisnik.Ime} {korisnik.Prezime}</p>" +
                           $"<p>Korisnik je podnio zahtjev za studentski status putem OptiShape aplikacije.</p>" +
                           $"<p>Datum i vrijeme zahtjeva: {DateTime.Now}</p>",
                    IsBodyHtml = true
                })
                {
                    await Task.Run(() => smtp.Send(message));
                    _logger.LogInformation("Email successfully sent to {0}", toAddress.Address);
                    TempData["Success"] = "Zahtjev je uspješno poslan. Očekujte odgovor na vaš email u narednih nekoliko dana.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                TempData["Error"] = $"Greška prilikom slanja zahtjeva: {ex.Message}";
            }

            return RedirectToAction("StudentApplication");
        }


        [Authorize]
        public async Task<IActionResult> IzborTrenera()
        {
            var userEmail = User.Identity?.Name;
            var korisnik = await _db.Korisnik.FirstOrDefaultAsync(k => k.Email == userEmail);

            if (korisnik == null || korisnik.IdTrenera != null || User.IsInRole("Administrator") || User.IsInRole("Trener"))
            {
                return RedirectToAction("Dashboard");
            }

            var korisnikovCilj = korisnik.Cilj;

            // Dohvati sve Identity korisnike s rolom "Trener"
            var sviIdentityKorisnici = _userManager.Users.ToList();
            var treneriEmails = new List<string>();

            foreach (var identityUser in sviIdentityKorisnici)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                if (roles.Contains("Trener"))
                {
                    treneriEmails.Add(identityUser.Email);
                }
            }

            // Pronađi trenere iz baze s istim ciljem
            var treneri = await _db.Korisnik
                .Where(k => treneriEmails.Contains(k.Email) && k.Cilj == korisnikovCilj)
                .ToListAsync();

            // Pripremi ViewModel s brojem korisnika po treneru
            var trenerVMs = treneri.Select(trener => new TrenerViewModel
            {
                Trener = trener,
                BrojKorisnika = _db.Korisnik.Count(k => k.IdTrenera == trener.IdKorisnika)
            }).ToList();

            return View(trenerVMs);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> OdaberiTrenera(int trenerId)
        {
            var userEmail = User.Identity?.Name;
            var korisnik = await _db.Korisnik.FirstOrDefaultAsync(k => k.Email == userEmail);

            if (korisnik == null || korisnik.IdTrenera != null)
            {
                return RedirectToAction("Dashboard");
            }

            var trener = await _db.Korisnik.FirstOrDefaultAsync(k => k.IdKorisnika == trenerId);
            if (trener == null)
            {
                TempData["Error"] = "Odabrani trener ne postoji.";
                return RedirectToAction("IzborTrenera");
            }

            korisnik.IdTrenera = trenerId;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Uspješno ste odabrali trenera!";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> GuestDashboard()
        {
            try
            {
                // Get trainers directly with a single efficient query
                var trenerVMs = await GetTrainersOptimized();

                // Set TempData message to inform the user they're in guest mode
                TempData["GuestMode"] = "Pregledavate aplikaciju u gost načinu rada. Neke funkcionalnosti nisu dostupne za goste.";

                return View(trenerVMs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading guest dashboard: {ex.Message}");

                // Return the view with an empty list if there's an error
                TempData["Error"] = "Došlo je do greške prilikom učitavanja podataka o trenerima.";
                return View(new List<TrenerViewModel>());
            }
        }

        private async Task<List<TrenerViewModel>> GetTrainersOptimized()
        {
            // Get trainers and their client counts in a single efficient query
            var trenerData = await (
                from k in _db.Korisnik
                join u in _userManager.Users on k.Email equals u.Email
                join ur in _db.UserRoles on u.Id equals ur.UserId
                join r in _db.Roles on ur.RoleId equals r.Id
                where r.Name == "Trener"
                select new
                {
                    Trener = k,
                    // Use left join logic to count clients
                    ClientCount = _db.Korisnik.Count(client => client.IdTrenera == k.IdKorisnika)
                }
            ).ToListAsync();

            // Convert to view model
            return trenerData.Select(td => new TrenerViewModel
            {
                Trener = td.Trener,
                BrojKorisnika = td.ClientCount
            }).ToList();
        }
    }

}