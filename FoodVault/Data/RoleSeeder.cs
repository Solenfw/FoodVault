using FoodVault.Constants;
using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FoodVault.Data
{
    /// <summary>
    /// Class để khởi tạo roles và admin user mặc định
    /// </summary>
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RoleSeeder> _logger;

        /// <summary>
        /// Khởi tạo instance mới của RoleSeeder
        /// </summary>
        /// <param name="roleManager">RoleManager để quản lý roles</param>
        /// <param name="userManager">UserManager để quản lý users</param>
        /// <param name="logger">Logger để ghi log</param>
        public RoleSeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Thực hiện seeding roles và admin user
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu seeding roles và admin user...");

                // Tạo roles
                await CreateRoleIfNotExists(AppRoles.Admin);
                await CreateRoleIfNotExists(AppRoles.Moderator);
                await CreateRoleIfNotExists(AppRoles.User);

                // Tạo admin user mặc định
                await CreateDefaultAdminUser();

                _logger.LogInformation("Hoàn thành seeding roles và admin user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi seeding roles và admin user");
                throw;
            }
        }

        /// <summary>
        /// Tạo role nếu chưa tồn tại
        /// </summary>
        /// <param name="roleName">Tên role cần tạo</param>
        private async Task CreateRoleIfNotExists(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    _logger.LogWarning("Tên role không hợp lệ, bỏ qua...");
                    return;
                }

                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName)
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant()
                    };

                    var result = await _roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Đã tạo role thành công: {RoleName}", roleName);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError("Lỗi khi tạo role {RoleName}: {Errors}", roleName, errors);
                    }
                }
                else
                {
                    _logger.LogDebug("Role {RoleName} đã tồn tại, bỏ qua...", roleName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tạo role {RoleName}", roleName);
                throw;
            }
        }

        /// <summary>
        /// Tạo admin user mặc định nếu chưa tồn tại
        /// </summary>
        private async Task CreateDefaultAdminUser()
        {
            const string adminEmail = "admin@foodvault.com";
            const string adminPassword = "Admin@123456";
            const string adminUserName = "admin";

            try
            {
                // Kiểm tra xem admin user đã tồn tại chưa
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    _logger.LogInformation("Tạo admin user mới: {Email}", adminEmail);

                    adminUser = new User
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        Name = "System Administrator",
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(adminUser, adminPassword);

                    if (createResult.Succeeded)
                    {
                        _logger.LogInformation("Đã tạo admin user thành công: {Email}", adminEmail);

                        // Thêm role Admin cho user
                        var roleResult = await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);

                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Đã thêm role Admin cho user: {Email}", adminEmail);
                        }
                        else
                        {
                            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            _logger.LogError("Lỗi khi thêm role Admin cho user {Email}: {Errors}", adminEmail, roleErrors);
                        }

                        // Cảnh báo về mật khẩu mặc định
                        _logger.LogWarning("⚠️  QUAN TRỌNG: Mật khẩu mặc định cho admin: {Password} - HÃY ĐỔI MẬT KHẨU NÀY NGAY!", adminPassword);
                        _logger.LogWarning("⚠️  Email đăng nhập: {Email}", adminEmail);
                    }
                    else
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        _logger.LogError("Lỗi khi tạo admin user {Email}: {Errors}", adminEmail, errors);

                        foreach (var error in createResult.Errors)
                        {
                            _logger.LogError("Chi tiết lỗi: {Code} - {Description}", error.Code, error.Description);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Admin user đã tồn tại: {Email}", adminEmail);

                    // Kiểm tra xem user đã có role Admin chưa
                    var isInRole = await _userManager.IsInRoleAsync(adminUser, AppRoles.Admin);

                    if (!isInRole)
                    {
                        _logger.LogInformation("Thêm role Admin cho user hiện có: {Email}", adminEmail);

                        var roleResult = await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);

                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Đã thêm role Admin thành công cho user: {Email}", adminEmail);
                        }
                        else
                        {
                            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            _logger.LogError("Lỗi khi thêm role Admin cho user {Email}: {Errors}", adminEmail, roleErrors);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("User {Email} đã có role Admin", adminEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tạo admin user");
                throw;
            }
        }
    }
}

