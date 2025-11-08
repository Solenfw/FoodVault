using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodVault.Models.Data;
using FoodVault.Services;
using FoodVault.Services.Interfaces;
using FoodVault.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using FoodVault.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


// adding Db context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FoodVault.Models.Data.FoodVaultDbContext>(options =>
    options.UseSqlServer(connectionString));
// add Identity services with roles support
builder.Services.AddDefaultIdentity<FoodVault.Models.Entities.User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FoodVault.Models.Data.FoodVaultDbContext>();

// Add Google external authentication
builder.Services.AddAuthentication()
.AddGoogle(options =>
{
    IConfigurationSection googleAuthSection = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuthSection["ClientId"];
    options.ClientSecret = googleAuthSection["ClientSecret"];
});

// Configure application cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Error/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// add services to the container
builder.Services.AddRazorPages(); 

// register application services
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IFridgeService, FridgeService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<AzureBlobService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// memory cache + home service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// register RoleSeeder
builder.Services.AddScoped<RoleSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<FoodVaultDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Custom error handling
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Configure access denied path for cookie authentication
var cookieAuthOptions = app.Services.GetRequiredService<IOptions<CookieAuthenticationOptions>>();
cookieAuthOptions.Value.AccessDeniedPath = "/Error/AccessDenied";

app.UseAuthorization();

// Seed roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        //var seeder = new RoleSeeder(roleManager, userManager, logger);
        // Trong Program.cs, dòng 98-103, sửa thành:
        var seeder = services.GetRequiredService<RoleSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles and admin user");
    }
}

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "profile",
    pattern: "profile/{action=Index}/{id?}",
    defaults: new { controller = "Profile" }
);

app.MapControllerRoute(
    name: "search",
    pattern: "search/{action=Index}/{query?}",
    defaults: new { controller = "Search" }
);

// Admin area routing
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
