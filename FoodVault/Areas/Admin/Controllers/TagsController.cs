using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class TagsController : Controller
    {
        private readonly FoodVaultDbContext _dbContext;
        private readonly ILogger<TagsController> _logger;

        public TagsController(FoodVaultDbContext dbContext, ILogger<TagsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _dbContext.Tags
                    .Include(t => t.RecipeTags)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(t => t.Name.ToLower().Contains(searchLower));
                }

                var totalTags = await query.CountAsync();
                var tags = await query
                    .OrderBy(t => t.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = tags.Select(t => new TagListViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    RecipeCount = t.RecipeTags?.Count ?? 0
                }).ToList();

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalTags = totalTags;
                ViewBag.TotalPages = (int)Math.Ceiling(totalTags / (double)pageSize);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tags");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách tags.";
                return View(new List<TagListViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTagViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTagViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                // Check if tag name already exists
                var exists = await _dbContext.Tags.AnyAsync(t => t.Name.ToLower() == vm.Name.ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(vm.Name), "Tag này đã tồn tại.");
                    return View(vm);
                }

                var tag = new Tag
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = vm.Name
                };

                await _dbContext.Tags.AddAsync(tag);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Tạo tag thành công.";
                return RedirectToAction(nameof(Details), new { id = tag.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                TempData["Error"] = "Có lỗi xảy ra khi tạo tag.";
                return View(vm);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID tag không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var tag = await _dbContext.Tags
                    .Include(t => t.RecipeTags)
                    .ThenInclude(rt => rt.Recipe)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    TempData["Error"] = "Không tìm thấy tag.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = new TagDetailsViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    RecipeCount = tag.RecipeTags?.Count ?? 0
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag details {TagId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin tag.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID tag không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var tag = await _dbContext.Tags.FindAsync(id);
                if (tag == null)
                {
                    TempData["Error"] = "Không tìm thấy tag.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = new EditTagViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag for edit {TagId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin tag.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTagViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var tag = await _dbContext.Tags.FindAsync(vm.Id);
                if (tag == null)
                {
                    TempData["Error"] = "Không tìm thấy tag.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if name changed and conflicts with existing
                if (tag.Name.ToLower() != vm.Name.ToLower())
                {
                    var exists = await _dbContext.Tags.AnyAsync(t => t.Name.ToLower() == vm.Name.ToLower() && t.Id != vm.Id);
                    if (exists)
                    {
                        ModelState.AddModelError(nameof(vm.Name), "Tag này đã tồn tại.");
                        return View(vm);
                    }
                }

                tag.Name = vm.Name;

                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Cập nhật tag thành công.";
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag {TagId}", vm.Id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật tag.";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID tag không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var tag = await _dbContext.Tags
                    .Include(t => t.RecipeTags)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    TempData["Error"] = "Không tìm thấy tag cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if tag is used in recipes
                if (tag.RecipeTags?.Any() ?? false)
                {
                    TempData["Error"] = "Không thể xóa tag này vì nó đang được sử dụng trong công thức.";
                    return RedirectToAction(nameof(Index));
                }

                _dbContext.Tags.Remove(tag);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa tag thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag {TagId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa tag: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

