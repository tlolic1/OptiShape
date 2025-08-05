using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OptiShape.Data;
using OptiShape.Models;

namespace OptiShape.Controllers
{
    [Authorize(Roles = "Administrator, Korisnik, Trener")]

    public class StatistikeNapretkaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatistikeNapretkaController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StatistikeNapretka
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrator"))
            {
                var statistike = await _context.StatistikeNapretka
                    .Include(s => s.Korisnik)
                    .ToListAsync();
                return View(statistike);
            }
            else if (User.IsInRole("Trener"))
            {
                var email = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (trener == null)
                    return Unauthorized();

                var statistike = await _context.StatistikeNapretka
                    .Include(s => s.Korisnik)
                    .Where(s => s.Korisnik != null && s.Korisnik.IdTrenera == trener.IdKorisnika)
                    .ToListAsync();

                return View(statistike); // možeš imati poseban view ako želiš
            }
            else // Korisnik
            {
                var email = User.Identity?.Name;
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (korisnik == null)
                    return NotFound("Korisnik nije pronađen.");

                var statistike = await _context.StatistikeNapretka
                    .Include(s => s.Korisnik)
                    .Where(s => s.IdKorisnika == korisnik.IdKorisnika)
                    .ToListAsync();

                return View(statistike);
            }
        }


        // GET: StatistikeNapretka/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var statistika = await _context.StatistikeNapretka
                .Include(s => s.Korisnik)
                .FirstOrDefaultAsync(m => m.IdZapisa == id);

            if (statistika == null)
                return NotFound();

            if (User.IsInRole("Korisnik"))
            {
                var email = User.Identity?.Name;
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (korisnik == null || statistika.IdKorisnika != korisnik.IdKorisnika)
                    return Forbid();
            }
            else if (User.IsInRole("Trener"))
            {
                var email = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (trener == null || statistika.Korisnik?.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }


            return View(statistika);
        }

        // GET: StatistikeNapretka/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            var korisnici = _context.Korisnik
                .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                .ToList();
            ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika", "PunoIme");
            return View();
        }

        // POST: StatistikeNapretka/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("IdZapisa,Datum,Tezina,KalorijskiUnos,IdKorisnika")] StatistikeNapretka statistikeNapretka)
        {
            // Pronađi korisnika i uzmi visinu
            var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.IdKorisnika == statistikeNapretka.IdKorisnika);

            if (korisnik == null)
            {
                ModelState.AddModelError("IdKorisnika", "Korisnik nije pronađen.");
            }
            else
            {
                // BMI = tezina (kg) / (visina (m))^2
                var visinaMetri = korisnik.Visina / 100.0;
                if (visinaMetri > 0)
                {
                    statistikeNapretka.Bmi = Math.Round(statistikeNapretka.Tezina / (visinaMetri * visinaMetri), 2);
                }
                else
                {
                    ModelState.AddModelError("IdKorisnika", "Visina korisnika nije validna.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(statistikeNapretka);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Statistika napretka je uspješno dodana.";
                return RedirectToAction(nameof(Index));
            }

            // Prikaz grešaka u konzoli (debug)
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"Greška za {entry.Key}: {error.ErrorMessage}");
                }
            }

            var korisnici = _context.Korisnik
                .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                .ToList();
            ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika", "PunoIme", statistikeNapretka.IdKorisnika);
            return View(statistikeNapretka);
        }

        // GET: StatistikeNapretka/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var statistika = await _context.StatistikeNapretka
                .Include(s => s.Korisnik)
                .FirstOrDefaultAsync(s => s.IdZapisa == id);

            if (statistika == null)
                return NotFound();

            if (User.IsInRole("Trener"))
            {
                var email = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (trener == null || statistika.Korisnik?.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }

            var korisnici = await _context.Korisnik
                .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                .ToListAsync();

            ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika", "PunoIme", statistika.IdKorisnika);
            return View(statistika);
        }


        // POST: StatistikeNapretka/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("IdZapisa,Datum,Tezina,KalorijskiUnos,IdKorisnika")] StatistikeNapretka statistikeNapretka)
        {
            if (id != statistikeNapretka.IdZapisa)
                return NotFound();

            var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.IdKorisnika == statistikeNapretka.IdKorisnika);

            if (korisnik == null)
            {
                ModelState.AddModelError("IdKorisnika", "Korisnik nije pronađen.");
            }
            else
            {
                // Provjeri da li je trener vlasnik korisnika
                if (User.IsInRole("Trener"))
                {
                    var email = User.Identity?.Name;
                    var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                    if (trener == null || korisnik.IdTrenera != trener.IdKorisnika)
                        return Forbid();
                }

                var visinaMetri = korisnik.Visina / 100.0;
                if (visinaMetri > 0)
                {
                    statistikeNapretka.Bmi = Math.Round(statistikeNapretka.Tezina / (visinaMetri * visinaMetri), 2);
                }
                else
                {
                    ModelState.AddModelError("IdKorisnika", "Visina korisnika nije validna.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(statistikeNapretka);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Statistika napretka je uspješno ažurirana.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatistikeNapretkaExists(statistikeNapretka.IdZapisa))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Debug greške u modelu
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"Greška za {entry.Key}: {error.ErrorMessage}");
                }
            }

            var korisnici = await _context.Korisnik
                .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                .ToListAsync();

            ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika", "PunoIme", statistikeNapretka.IdKorisnika);
            return View(statistikeNapretka);
        }


        // GET: StatistikeNapretka/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var statistika = await _context.StatistikeNapretka
                .Include(s => s.Korisnik)
                .FirstOrDefaultAsync(m => m.IdZapisa == id);

            if (statistika == null)
                return NotFound();

            return View(statistika);
        }

        // POST: StatistikeNapretka/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var statistika = await _context.StatistikeNapretka.FindAsync(id);
            if (statistika != null)
            {
                _context.StatistikeNapretka.Remove(statistika);
                TempData["DeleteMessage"] = "Statistika napretka je uspješno obrisana.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // za korisnika create poseban (GET)
        [Authorize(Roles = "Korisnik")]
        public IActionResult CreateForUser()
        {
            // Initialize the model with default values
            var statistika = new StatistikeNapretka
            {
                Datum = DateTime.Today // Set default date to today
            };

            ViewData["Title"] = "Dodaj statistiku napretka";
            return View("CreateForUser", statistika); // Pass the model to the view
        }


        // ZA KORISNIKA POST CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Korisnik")]
        public async Task<IActionResult> CreateForUser([Bind("Datum,Tezina")] StatistikeNapretka statistikeNapretka)
        {
            var email = User.Identity?.Name;
            var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

            if (korisnik == null)
                return NotFound("Korisnik nije pronađen.");

            var visinaMetri = korisnik.Visina / 100.0;
            if (visinaMetri <= 0)
            {
                ModelState.AddModelError(string.Empty, "Visina korisnika nije validna.");
                return View("CreateForUser", statistikeNapretka);
            }

            statistikeNapretka.IdKorisnika = korisnik.IdKorisnika;
            statistikeNapretka.Bmi = Math.Round(statistikeNapretka.Tezina / (visinaMetri * visinaMetri), 2);
            statistikeNapretka.KalorijskiUnos = (int)Math.Round(24 * statistikeNapretka.Tezina * 1.2, 0);
            // primjer: BMR * aktivnost

            if (ModelState.IsValid)
            {
                _context.Add(statistikeNapretka);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Statistika je uspješno dodana.";
                return RedirectToAction(nameof(Index));
            }

            return View("CreateForUser", statistikeNapretka);
        }





        private bool StatistikeNapretkaExists(int id)
        {
            return _context.StatistikeNapretka.Any(e => e.IdZapisa == id);
        }
        [Authorize(Roles = "Korisnik")]
        public async Task<IActionResult> Graf()
        {
            List<StatistikeNapretka> statistike;

            if (User.IsInRole("Administrator"))
            {
                statistike = await _context.StatistikeNapretka
                    .Include(s => s.Korisnik)
                    .ToListAsync();
            }
            else if (User.IsInRole("Trener"))
            {
                var email = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                statistike = await _context.StatistikeNapretka
                    .Include(s => s.Korisnik)
                    .Where(s => s.Korisnik != null && s.Korisnik.IdTrenera == trener.IdKorisnika)
                    .ToListAsync();
            }
            else // Korisnik
            {
                var email = User.Identity?.Name;
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                statistike = await _context.StatistikeNapretka
                    .Where(s => s.IdKorisnika == korisnik.IdKorisnika)
                    .ToListAsync();
            }

            statistike = statistike.OrderBy(s => s.Datum).ToList();
            return View("Graf", statistike);
        } // za grafikon statistike napretka korisnika




    }


}