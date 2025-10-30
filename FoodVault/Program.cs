using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodVault.Models.Data;
using FoodVault.Services;
using FoodVault.Services.Interfaces;
using FoodVault.Models.Entities;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// adding Db context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FoodVault.Models.Data.FoodVaultDbContext>(options =>
    options.UseSqlServer(connectionString));
// add Identity services
builder.Services.AddDefaultIdentity<FoodVault.Models.Entities.User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<FoodVault.Models.Data.FoodVaultDbContext>();

// add services to the container
builder.Services.AddRazorPages(); 

// register application services
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IFridgeService, FridgeService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// memory cache + home service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IHomeService, HomeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Apply migrations and seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<FoodVaultDbContext>();
    db.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<User>>();
    var adminEmail = "admin@foodvault.local";
    var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Name = "Admin",
            Role = "admin",
            CreatedAt = DateTime.UtcNow
        };
        var result = userManager.CreateAsync(adminUser, "Admin@12345!").GetAwaiter().GetResult();
        if (!result.Succeeded)
        {
            throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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
    name: "recipe",
    pattern: "recipe/{action=Index}/{id?}",
    defaults: new { controller = "Recipes" }
);

app.MapControllerRoute(
    name: "search",
    pattern: "search/{action=Index}/{query?}",
    defaults: new { controller = "Search" }
);

app.MapRazorPages();
app.Run();
