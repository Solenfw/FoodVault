using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodVault.Models;
namespace FoodVault.Controllers
{
    public class RecipesController : Controller
    {
        // Everyone can see this
        public IActionResult Index()
        {
            // ... code to list recipes
            return View();
        }

        // Everyone can see this
        public IActionResult Details(int id)
        {
            // ... code to show one recipe
            return View();
        }

        // ONLY logged-in users can access this
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ONLY logged-in users can submit this form
        [Authorize]
        [HttpPost]
        public IActionResult Create(Recipe recipe)
        {
            // ... code to save the new recipe
            return RedirectToAction("Index");
        }
    }
}
