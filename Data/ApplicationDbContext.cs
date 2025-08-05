using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OptiShape.Models;

namespace OptiShape.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Korisnik> Korisnik { get; set; }
        public DbSet<StatistikeNapretka> StatistikeNapretka { get; set; }
        public DbSet<PlanIshraneTreninga> PlanIshraneTreninga { get; set; }
        public DbSet<Placanje> Placanje { get; set; }
        public DbSet<Termin> Termin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<StatistikeNapretka>().ToTable("StatistikeNapretka");
            modelBuilder.Entity<PlanIshraneTreninga>().ToTable("PlanIshraneTreninga");
            modelBuilder.Entity<Placanje>().ToTable("Placanje");
            modelBuilder.Entity<Termin>().ToTable("Termin");

            base.OnModelCreating(modelBuilder);
        }
    }
}
