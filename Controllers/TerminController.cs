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
    public class TerminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TerminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<Termin> terminiQuery;

            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                if (trener == null)
                    return Forbid();

                terminiQuery = _context.Termin
                    .Include(t => t.Korisnik)
                    .Where(t => t.Korisnik != null && t.Korisnik.IdTrenera == trener.IdKorisnika);

            }
            else if (User.IsInRole("Korisnik"))
            {
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                if (korisnik == null)
                    return Forbid();

                terminiQuery = _context.Termin
                    .Include(t => t.Korisnik)
                    .Where(t => t.IdKorisnika == korisnik.IdKorisnika);
            }
            else
            {
                // Administrator vidi sve termine
                terminiQuery = _context.Termin.Include(t => t.Korisnik);
            }

            return View(await terminiQuery.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var termin = await _context.Termin.Include(t => t.Korisnik).FirstOrDefaultAsync(m => m.IdTermina == id);

            if (termin == null)
                return NotFound();

            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                if (trener == null || termin.Korisnik?.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }

            return View(termin);
        }

        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Create()
        {

            await PostaviKorisnikeDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Create([Bind("IdTermina,Datum,IdKorisnika,VrijemeOd")] Termin termin)
        {
            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.IdKorisnika == termin.IdKorisnika);

                if (trener == null || korisnik == null || korisnik.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }

            // Kombinuj datum i vrijeme
            termin.Datum = termin.Datum.Date + termin.VrijemeOd;

            if (ModelState.IsValid)
            {
                _context.Add(termin);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Termin je uspješno dodat.";
                return RedirectToAction(nameof(Index));
            }

            if (termin.VrijemeOd == default)
            {
                ModelState.AddModelError("VrijemeOd", "Unesite vrijeme početka.");
            }

            await PostaviKorisnikeDropdown(termin.IdKorisnika);
            return View(termin);
        }


        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var termin = await _context.Termin.Include(t => t.Korisnik).FirstOrDefaultAsync(t => t.IdTermina == id);
            if (termin == null)
                return NotFound();

            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                if (trener == null || termin.Korisnik?.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }

            await PostaviKorisnikeDropdown(termin.IdKorisnika);
            return View(termin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Edit(int id, [Bind("IdTermina,Datum,IdKorisnika")] Termin termin)
        {
            if (id != termin.IdTermina)
                return NotFound();

            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                var korisnik = await _context.Korisnik.FirstOrDefaultAsync(k => k.IdKorisnika == termin.IdKorisnika);

                if (trener == null || korisnik == null || korisnik.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }

            termin.Datum = termin.Datum.Date + termin.VrijemeOd;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(termin);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Termin je uspješno ažuriran.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TerminExists(termin.IdTermina))
                        return NotFound();
                    else throw;
                }
            }

            await PostaviKorisnikeDropdown(termin.IdKorisnika);
            return View(termin);
        }

        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var termin = await _context.Termin.Include(t => t.Korisnik).FirstOrDefaultAsync(m => m.IdTermina == id);
            if (termin == null)
                return NotFound();

            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                if (trener == null || termin.Korisnik?.IdTrenera != trener.IdKorisnika)
                    return Forbid();
            }

            return View(termin);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Trener")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var termin = await _context.Termin.Include(t => t.Korisnik).FirstOrDefaultAsync(t => t.IdTermina == id);

            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (termin != null)
            {
                if (User.IsInRole("Trener"))
                {
                    var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);
                    if (trener == null || termin.Korisnik?.IdTrenera != trener.IdKorisnika)
                        return Forbid();
                }

                _context.Termin.Remove(termin);
                TempData["SuccessMessage"] = "Termin je uspješno obrisan.";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PostaviKorisnikeDropdown(int? selectedId = null)
        {
            List<SelectListItem> korisnici;
            var appUser = await _userManager.GetUserAsync(User);
            var email = appUser?.Email;

            if (User.IsInRole("Trener"))
            {
                var trener = await _context.Korisnik.FirstOrDefaultAsync(k => k.Email == email);

                if (trener == null)
                {
                    korisnici = new List<SelectListItem>(); // ili baci grešku ako želiš
                }
                else
                {
                    korisnici = await _context.Korisnik
                        .Where(k => k.IdTrenera == trener.IdKorisnika)
                        .Select(k => new SelectListItem
                        {
                            Value = k.IdKorisnika.ToString(),
                            Text = k.Ime + " " + k.Prezime,
                            Selected = k.IdKorisnika == selectedId
                        })
                        .ToListAsync();
                }
            }
            else
            {
                korisnici = await _context.Korisnik
                    .Select(k => new SelectListItem
                    {
                        Value = k.IdKorisnika.ToString(),
                        Text = k.Ime + " " + k.Prezime,
                        Selected = k.IdKorisnika == selectedId
                    })
                    .ToListAsync();
            }

            ViewBag.Korisnici = korisnici;
        }


        private bool TerminExists(int id)
        {
            return _context.Termin.Any(e => e.IdTermina == id);
        }
    }
}
