
using FoodVault.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace FoodVault.ViewComponents
{
    public class SearchBarViewComponent : ViewComponent
    {
        private readonly FoodVaultDbContext _context;

        public SearchBarViewComponent(FoodVaultDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tags = await _context.Tags
                .OrderBy(t => t.Name) // Sắp xếp theo tên
                .Take(5) // Lấy 5 tag đầu
                .Select(t => t.Name)
                .ToListAsync();

            return View("SearchBar",tags);
        }
    }
}