using FoodVault.Models;
using FoodVault.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodVault.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly FoodVaultDbContext _context;
        public HomeController(FoodVaultDbContext context)
        {
            
            _context = context;
        }

        public IActionResult Index()
        {
            // Eager-load related data you want to show (Ratings, RecipeTags, etc.)
            var lstrecipes = _context.Recipes
                .Include(r => r.Ratings)            // để hiển thị avg rating nếu cần
                .Include(r => r.RecipeIngredients)  // nếu cần hiển thị ingredient count
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(lstrecipes);
        }

        // optional: Details action
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var recipe = _context.Recipes
                .Include(r => r.Ratings)
                .Include(r => r.RecipeIngredients)
                .FirstOrDefault(r => r.Id == id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
