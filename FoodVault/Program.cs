<<<<<<< HEAD
using FoodVault.Data;
using FoodVault.Services;
=======

using FoodVault.Models.Data;
using FoodVault.ViewComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
>>>>>>> origin/develop
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// adding Db context
<<<<<<< HEAD
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FoodVaultDbContext>(options =>
    options.UseSqlServer(connectionString));

=======
builder.Services.AddDbContext<FoodVaultDbContext>(options=>     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'FoodVaultContext' not found.")));
>>>>>>> origin/develop

// add Identity services
builder.Services.AddDefaultIdentity<FoodVault.Models.User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<FoodVaultDbContext>();
<<<<<<< HEAD

// add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<FileUploadService>();
=======
builder.Services.AddScoped<SearchBarViewComponent>();
// add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
>>>>>>> origin/develop

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
