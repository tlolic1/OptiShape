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
    [Authorize(Roles = "Administrator")]
    public class KorisnikController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KorisnikController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Korisnik
        public async Task<IActionResult> Index()
        {
            return View(await _context.Korisnik.ToListAsync());
        }

        // GET: Korisnik/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnik
                .Include(k => k.Trener)  // Make sure we include the Trainer relationship
                .FirstOrDefaultAsync(m => m.IdKorisnika == id);

            if (korisnik == null)
            {
                return NotFound();
            }

            return View(korisnik);
        }

        // GET: Korisnik/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Korisnik/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("IdKorisnika,Ime,Prezime,Email,Sifra,DatumRodjenja,Visina,Tezina,Spol,Cilj,StudentskiStatus,BrojTelefona")] Korisnik korisnik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(korisnik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(korisnik);
        }

        // GET: Korisnik/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnik.FindAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }
            return View(korisnik);
        }

        // POST: Korisnik/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("IdKorisnika,Ime,Prezime,Email,Sifra,DatumRodjenja,Visina,Tezina,Spol,Cilj,StudentskiStatus,BrojTelefona")] Korisnik korisnik)
        {
            if (id != korisnik.IdKorisnika)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(korisnik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KorisnikExists(korisnik.IdKorisnika))
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
            return View(korisnik);
        }

        // GET: Korisnik/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnik
                .FirstOrDefaultAsync(m => m.IdKorisnika == id);
            if (korisnik == null)
            {
                return NotFound();
            }

            return View(korisnik);
        }

        // POST: Korisnik/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var korisnik = await _context.Korisnik.FindAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }

            // Find the corresponding Identity user
            var user = await _userManager.FindByEmailAsync(korisnik.Email);

            // Start a transaction to ensure consistency
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove from Korisnik table
                _context.Korisnik.Remove(korisnik);
                await _context.SaveChangesAsync();

                // If the corresponding Identity user exists, delete it too
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        // If Identity deletion fails, roll back and handle the error
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Greška prilikom brisanja korisničkog računa.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Commit the transaction if everything succeeded
                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "Korisnik je uspješno izbrisan.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Roll back on any exception
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Došlo je do greške: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool KorisnikExists(int id)
        {
            return _context.Korisnik.Any(e => e.IdKorisnika == id);
        }
    }
}