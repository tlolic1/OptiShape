using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OptiShape.Data;
using OptiShape.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Konfiguracija validacije lozinke
    options.Password.RequireDigit = false;            // Ne zahtijeva brojeve
    options.Password.RequireLowercase = false;        // Ne zahtijeva mala slova
    options.Password.RequireUppercase = false;        // Ne zahtijeva velika slova
    options.Password.RequireNonAlphanumeric = false;  // Ne zahtijeva specijalne znakove
    options.Password.RequiredLength = 6;              // Minimalna dužina 6 znakova
})
     .AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure cookie authentication to respect "Remember Me" checkbox
builder.Services.ConfigureApplicationCookie(options =>
{
    // When Remember Me is checked, cookie will persist for 14 days
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // This makes the auth cookie expire with browser session when Remember Me is not checked
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews()


    .AddMvcOptions(options =>
    {
        var provider = options.ModelBindingMessageProvider;

        provider.SetValueMustNotBeNullAccessor(field => $"Polje '{field}' je obavezno.");
        provider.SetValueMustBeANumberAccessor(field => $"Polje '{field}' mora biti broj.");
        provider.SetMissingBindRequiredValueAccessor(field => $"Vrijednost za '{field}' nije dostavljena.");
        provider.SetAttemptedValueIsInvalidAccessor((value, field) => $"Vrijednost '{value}' nije ispravna za '{field}'.");
        provider.SetMissingKeyOrValueAccessor(() => "Nedostaje vrijednost.");
        provider.SetUnknownValueIsInvalidAccessor(field => $"Vrijednost za '{field}' nije ispravna.");
        provider.SetValueIsInvalidAccessor(field => $"Vrijednost za '{field}' nije validna.");
        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Vrijednost '{value}' nije validna.");
        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Vrijednost nije validna.");
        provider.SetNonPropertyValueMustBeANumberAccessor(() => "Vrijednost mora biti broj.");
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Added this line to ensure authentication is configured
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();