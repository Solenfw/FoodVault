using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
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
    }
}











