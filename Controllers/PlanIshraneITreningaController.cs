using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using OptiShape.Data;
using OptiShape.Models;

namespace OptiShape.Controllers
{


    [Authorize(Roles = "Administrator, Korisnik, Trener")]
    public class PlanIshraneITreningaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlanIshraneITreningaController(ApplicationDbContext context)
        {
            _context = context;
        }



        // GET: PlanIshraneITreninga
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrator"))
            {
                var sviPlanovi = await _context.PlanIshraneTreninga
                    .Include(p => p.Korisnik)
                    .ToListAsync();

                return View("IndexAdmin", sviPlanovi);
            }
            else if (User.IsInRole("Trener"))
            {
                var email = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (trener != null)
                {
                    var planovi = await _context.PlanIshraneTreninga
                        .Include(p => p.Korisnik)
                        .Where(p => p.Korisnik != null && p.Korisnik.IdTrenera == trener.IdKorisnika)
                        .ToListAsync();

                    return View("IndexAdmin", planovi);
                }

                return Unauthorized();
            }
            else if (User.IsInRole("Korisnik"))
            {
                var email = User.Identity?.Name;
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (korisnik != null)
                {
                    var njegoviPlanovi = await _context.PlanIshraneTreninga
                        .Include(p => p.Korisnik)
                        .Where(p => p.IdKorisnika == korisnik.IdKorisnika)
                        .ToListAsync();

                    return View("IndexKorisnik", njegoviPlanovi);
                }
            }

            return Unauthorized();
        }





        // GET: PlanIshraneITreninga/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planIshraneTreninga = await _context.PlanIshraneTreninga
                .Include(p => p.Korisnik)
                .FirstOrDefaultAsync(m => m.IdPlana == id);
            if (planIshraneTreninga == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Trener"))
            {
                var trenerEmail = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == trenerEmail);

                if (trener == null || planIshraneTreninga.Korisnik?.IdTrenera != trener.IdKorisnika)
                {
                    return Forbid(); // zabrani pristup ako korisnik nije njegov
                }
            }


            return View(planIshraneTreninga);
        }

        // GET: PlanIshraneITreninga/Create
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            // Provjera uloge i filtriranje korisnika u skladu s tim
            if (User.IsInRole("Trener"))
            {
                // Dohvati email trenutnog trenera
                var trenerEmail = User.Identity.Name;

                // Pronađi trenera u tablici Korisnik
                var trener = await _context.Korisnik
                    .FirstOrDefaultAsync(k => k.Email == trenerEmail);

                if (trener != null)
                {
                    // Dohvati samo korisnike ovog trenera
                    var korisniciTrenera = await _context.Korisnik
                        .Where(k => k.IdTrenera == trener.IdKorisnika)
                        .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                        .ToListAsync();

                    // Kreiraj SelectList s pripremljenim punim imenom
                    ViewData["IdKorisnika"] = new SelectList(korisniciTrenera, "IdKorisnika", "PunoIme");
                }
                else
                {
                    // Fallback ako nije pronađen trener
                    ViewData["IdKorisnika"] = new SelectList(new List<object>(), "IdKorisnika", "PunoIme");
                }
            }
            else // Administrator
            {
                // Administratori vide sve korisnike s punim imenom
                var korisnici = await _context.Korisnik
                    .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                    .ToListAsync();
                ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika", "PunoIme");
            }

            return View();
        }

        // POST: PlanIshraneITreninga/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("IdPlana,DatumKreiranja,Kalorije,Protein,Ugljikohidrati,Masti,IdKorisnika")] PlanIshraneTreninga planIshraneTreninga)
        {
            if (ModelState.IsValid)
            {
                _context.Add(planIshraneTreninga);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Plan je uspješno dodat.";
                return RedirectToAction(nameof(Index));
            }

            



            // DEBUG: Ispis svih grešaka iz ModelState ako forma nije validna
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"Greška za {entry.Key}: {error.ErrorMessage}");
                }
            }



            // Ponovo postavi SelectList s pravilnim formatiranjem ako validacija ne prođe
            if (User.IsInRole("Trener"))
            {
                var trenerEmail = User.Identity.Name;
                var trener = await _context.Korisnik
                    .FirstOrDefaultAsync(k => k.Email == trenerEmail);

                if (trener != null)
                {
                    var korisniciTrenera = await _context.Korisnik
                        .Where(k => k.IdTrenera == trener.IdKorisnika)
                        .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                        .ToListAsync();

                    ViewData["IdKorisnika"] = new SelectList(korisniciTrenera, "IdKorisnika",
                        "PunoIme", planIshraneTreninga.IdKorisnika);
                }
                else
                {
                    ViewData["IdKorisnika"] = new SelectList(new List<object>(), "IdKorisnika", "PunoIme", planIshraneTreninga.IdKorisnika);
                }
            }
            else
            {
                var korisnici = await _context.Korisnik
                    .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                    .ToListAsync();
                ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika",
                    "PunoIme", planIshraneTreninga.IdKorisnika);
            }

            return View(planIshraneTreninga);
        }

        // GET: PlanIshraneITreninga/Edit/5
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planIshraneTreninga = await _context.PlanIshraneTreninga
    .Include(p => p.Korisnik)
    .FirstOrDefaultAsync(p => p.IdPlana == id);

            if (planIshraneTreninga == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Trener"))
            {
                var trenerEmail = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == trenerEmail);

                if (trener == null || planIshraneTreninga.Korisnik?.IdTrenera != trener.IdKorisnika)
                {
                    return Forbid(); // zabrani uređivanje ako nije njegov korisnik
                }
            }


            // Postavi SelectList na isti način kao i u Create akciji
            if (User.IsInRole("Trener"))
            {
                var trenerEmail = User.Identity.Name;
                var trener = await _context.Korisnik
                    .FirstOrDefaultAsync(k => k.Email == trenerEmail);

                if (trener != null)
                {
                    var korisniciTrenera = await _context.Korisnik
                        .Where(k => k.IdTrenera == trener.IdKorisnika)
                        .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                        .ToListAsync();

                    ViewData["IdKorisnika"] = new SelectList(korisniciTrenera, "IdKorisnika",
                        "PunoIme", planIshraneTreninga.IdKorisnika);
                }
                else
                {
                    ViewData["IdKorisnika"] = new SelectList(new List<object>(), "IdKorisnika", "PunoIme", planIshraneTreninga.IdKorisnika);
                }
            }
            else
            {
                var korisnici = await _context.Korisnik
                    .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                    .ToListAsync();
                ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika",
                    "PunoIme", planIshraneTreninga.IdKorisnika);
            }

            return View(planIshraneTreninga);
        }

        // POST: PlanIshraneITreninga/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Edit(int id, [Bind("IdPlana,DatumKreiranja,Kalorije,Protein,Ugljikohidrati,Masti,IdKorisnika")] PlanIshraneTreninga planIshraneTreninga)
        {
            if (id != planIshraneTreninga.IdPlana)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(planIshraneTreninga);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlanIshraneTreningaExists(planIshraneTreninga.IdPlana))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Trener"))
            {
                var trenerEmail = User.Identity?.Name;
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == trenerEmail);

                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.IdKorisnika == planIshraneTreninga.IdKorisnika);

                if (trener == null || korisnik?.IdTrenera != trener.IdKorisnika)
                {
                    return Forbid();
                }
            }


            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"❌ Greška u polju '{entry.Key}': {error.ErrorMessage}");
                    }
                }
            }

           
                 // dodana ova 2

            // Ponovno postavi SelectList ako validacija ne uspije
            if (User.IsInRole("Trener"))
            {
                var trenerEmail = User.Identity.Name;
                var trener = await _context.Korisnik
                    .FirstOrDefaultAsync(k => k.Email == trenerEmail);

                if (trener != null)
                {
                    var korisniciTrenera = await _context.Korisnik
                        .Where(k => k.IdTrenera == trener.IdKorisnika)
                        .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                        .ToListAsync();

                    ViewData["IdKorisnika"] = new SelectList(korisniciTrenera, "IdKorisnika",
                        "PunoIme", planIshraneTreninga.IdKorisnika);
                }
                else
                {
                    ViewData["IdKorisnika"] = new SelectList(new List<object>(), "IdKorisnika", "PunoIme", planIshraneTreninga.IdKorisnika);
                }
            }
            else
            {
                var korisnici = await _context.Korisnik
                    .Select(k => new { k.IdKorisnika, PunoIme = k.Ime + " " + k.Prezime })
                    .ToListAsync();
                ViewData["IdKorisnika"] = new SelectList(korisnici, "IdKorisnika",
                    "PunoIme", planIshraneTreninga.IdKorisnika);
            }

            return View(planIshraneTreninga);
        }

        // GET: PlanIshraneITreninga/Delete/5
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.IsInRole("Trener"))
            {
                return Forbid(); // trenerima zabranjeno
            }

            if (id == null)
            {
                return NotFound();
            }

            var planIshraneTreninga = await _context.PlanIshraneTreninga
                .Include(p => p.Korisnik)
                .FirstOrDefaultAsync(m => m.IdPlana == id);

            if (planIshraneTreninga == null)
            {
                return NotFound();
            }

            return View(planIshraneTreninga);
        }


        // POST: PlanIshraneITreninga/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.IsInRole("Trener"))
            {
                return Forbid(); // trenerima zabranjeno
            }

            var plan = await _context.PlanIshraneTreninga.FindAsync(id);

            if (plan != null)
            {
                _context.PlanIshraneTreninga.Remove(plan);
                TempData["DeleteMessage"] = "Plan je uspješno obrisan.";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        // za piechart
        [Authorize(Roles = "Administrator, Korisnik, Trener")]
        public async Task<IActionResult> PieChart(int id)
        {
            var plan = await _context.PlanIshraneTreninga
                .Include(p => p.Korisnik)
                .FirstOrDefaultAsync(p => p.IdPlana == id);

            if (plan == null)
                return NotFound();

            // Autorizacija: korisnik vidi samo svoj plan, trener samo svojih korisnika
            var email = User.Identity?.Name;
            var trenutniKorisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

            if (User.IsInRole("Korisnik") && plan.IdKorisnika != trenutniKorisnik?.IdKorisnika)
                return Forbid();

            if (User.IsInRole("Trener") && plan.Korisnik?.IdTrenera != trenutniKorisnik?.IdKorisnika)
                return Forbid();

            return View(plan);
        }



        private bool PlanIshraneTreningaExists(int id)
        {
            return _context.PlanIshraneTreninga.Any(e => e.IdPlana == id);
        }
    }
}