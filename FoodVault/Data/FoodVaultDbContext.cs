using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FoodVault.Models;

namespace FoodVault.Data
{
    public class FoodVaultDbContext : IdentityDbContext<User>
    {
        public FoodVaultDbContext(DbContextOptions<FoodVaultDbContext> options) : base(options)
        {
        }
    }
}
