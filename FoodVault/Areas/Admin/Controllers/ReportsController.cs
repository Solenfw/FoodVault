using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ReportsController : Controller
    {
        private readonly FoodVaultDbContext _dbContext;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            FoodVaultDbContext dbContext,
            IRecipeService recipeService,
            ILogger<ReportsController> logger)
        {
            _dbContext = dbContext;
            _recipeService = recipeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? status, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _dbContext.Reports
                    .Include(r => r.Reporter)
                    .Include(r => r.Resolver)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status) && status != "All")
                {
                    query = query.Where(r => r.Status == status);
                }

                var totalReports = await query.CountAsync();
                var reports = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = new List<ReportListViewModel>();

                foreach (var report in reports)
                {
                    string targetName = "N/A";
                    try
                    {
                        if (report.TargetType == "Recipe")
                        {
                            var recipe = await _recipeService.GetRecipeByIdAsync(report.TargetId);
                            targetName = recipe?.Title ?? "Recipe đã bị xóa";
                        }
                        else if (report.TargetType == "User")
                        {
                            var user = await _dbContext.Users.FindAsync(report.TargetId);
                            targetName = user?.UserName ?? "User đã bị xóa";
                        }
                        else if (report.TargetType == "Comment" || report.TargetType == "Rating")
                        {
                            var rating = await _dbContext.Ratings
                                .Include(r => r.Recipe)
                                .FirstOrDefaultAsync(r => r.Id == report.TargetId);
                            targetName = rating != null ? $"Comment trên recipe: {rating.Recipe?.Title ?? "N/A"}" : "Comment đã bị xóa";
                        }
                    }
                    catch
                    {
                        targetName = "Không tìm thấy";
                    }

                    viewModels.Add(new ReportListViewModel
                    {
                        Id = report.Id,
                        ReporterName = report.Reporter?.UserName ?? report.ReporterId,
                        ReporterId = report.ReporterId,
                        TargetType = report.TargetType,
                        TargetId = report.TargetId,
                        TargetName = targetName,
                        Reason = report.Reason,
                        Status = report.Status,
                        CreatedAt = report.CreatedAt,
                        ResolvedAt = report.ResolvedAt,
                        ResolvedBy = report.ResolvedBy
                    });
                }

                ViewBag.Status = status ?? "All";
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalReports = totalReports;
                ViewBag.TotalPages = (int)Math.Ceiling(totalReports / (double)pageSize);
                ViewBag.PendingCount = await _dbContext.Reports.CountAsync(r => r.Status == "Pending");

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách báo cáo.";
                return View(new List<ReportListViewModel>());
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID báo cáo không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var report = await _dbContext.Reports
                    .Include(r => r.Reporter)
                    .Include(r => r.Resolver)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (report == null)
                {
                    TempData["Error"] = "Không tìm thấy báo cáo.";
                    return RedirectToAction(nameof(Index));
                }

                string targetName = "N/A";
                string targetLink = "#";
                try
                {
                    if (report.TargetType == "Recipe")
                    {
                        var recipe = await _recipeService.GetRecipeByIdAsync(report.TargetId);
                        targetName = recipe?.Title ?? "Recipe đã bị xóa";
                        targetLink = recipe != null ? $"/recipe/details/{report.TargetId}" : "#";
                    }
                    else if (report.TargetType == "User")
                    {
                        var user = await _dbContext.Users.FindAsync(report.TargetId);
                        targetName = user?.UserName ?? "User đã bị xóa";
                        targetLink = user != null ? $"/admin/users/details/{report.TargetId}" : "#";
                    }
                    else if (report.TargetType == "Comment" || report.TargetType == "Rating")
                    {
                        var rating = await _dbContext.Ratings
                            .Include(r => r.Recipe)
                            .FirstOrDefaultAsync(r => r.Id == report.TargetId);
                        targetName = rating != null ? $"Comment trên recipe: {rating.Recipe?.Title ?? "N/A"}" : "Comment đã bị xóa";
                        targetLink = rating?.RecipeId != null ? $"/recipe/details/{rating.RecipeId}" : "#";
                    }
                }
                catch
                {
                    targetName = "Không tìm thấy";
                }

                var vm = new ReportDetailsViewModel
                {
                    Id = report.Id,
                    ReporterName = report.Reporter?.UserName ?? report.ReporterId,
                    ReporterId = report.ReporterId,
                    TargetType = report.TargetType,
                    TargetId = report.TargetId,
                    TargetName = targetName,
                    Reason = report.Reason,
                    Status = report.Status,
                    CreatedAt = report.CreatedAt,
                    ResolvedAt = report.ResolvedAt,
                    ResolvedBy = report.ResolvedBy,
                    ResolvedByName = report.Resolver?.UserName
                };

                ViewBag.TargetLink = targetLink;
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report details {ReportId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin báo cáo.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(string id, string action, string? notes)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID báo cáo không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var report = await _dbContext.Reports.FindAsync(id);
                if (report == null)
                {
                    TempData["Error"] = "Không tìm thấy báo cáo.";
                    return RedirectToAction(nameof(Index));
                }

                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                if (action == "Resolve")
                {
                    report.Status = "Resolved";
                    report.ResolvedAt = DateTime.UtcNow;
                    report.ResolvedBy = adminId;

                    // Optionally take action on the reported item
                    if (report.TargetType == "Recipe")
                    {
                        // Could delete or hide the recipe here
                    }
                    else if (report.TargetType == "Comment" || report.TargetType == "Rating")
                    {
                        var rating = await _dbContext.Ratings.FindAsync(report.TargetId);
                        if (rating != null)
                        {
                            // Could delete the rating/comment here
                            // _dbContext.Ratings.Remove(rating);
                        }
                    }

                    TempData["Success"] = "Đã giải quyết báo cáo.";
                }
                else if (action == "Dismiss")
                {
                    report.Status = "Dismissed";
                    report.ResolvedAt = DateTime.UtcNow;
                    report.ResolvedBy = adminId;
                    TempData["Success"] = "Đã bỏ qua báo cáo.";
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving report {ReportId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xử lý báo cáo.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID báo cáo không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var report = await _dbContext.Reports.FindAsync(id);
                if (report == null)
                {
                    TempData["Error"] = "Không tìm thấy báo cáo.";
                    return RedirectToAction(nameof(Index));
                }

                _dbContext.Reports.Remove(report);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa báo cáo thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa báo cáo.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

