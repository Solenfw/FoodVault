using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FoodVault.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;
        private readonly FoodVaultDbContext _dbContext;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger, FoodVaultDbContext dbContext)
        {
            _searchService = searchService;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SearchViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Results(string query)
        {
            try
            {
                var results = await _searchService.SearchRecipesAsync(query);
                var vm = new SearchResultsViewModel
                {
                    Query = query,
                    Results = results.Select(r => new RecipeListItemViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        UpdatedAt = r.UpdatedAt
                    }).ToList()
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search failed for {Query}", query);
                TempData["Error"] = "Search failed.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ByTag(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Tag không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var recipes = await _searchService.SearchRecipesAsync(string.Empty, new[] { id });
                var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

                var vm = new SearchResultsViewModel
                {
                    Query = tag != null ? $"#${tag.Name}" : "#tag",
                    Results = recipes.Select(r => new RecipeListItemViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        UpdatedAt = r.UpdatedAt
                    }).ToList()
                };

                return View("Results", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search by tag failed for {TagId}", id);
                TempData["Error"] = "Không thể tải công thức theo tag.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}















