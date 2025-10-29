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

        // register DbSets for each model
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This is crucial for Identity to work

            // --- Configure the Recipe-Comment Relationship ---
            // This breaks the cascade delete path from Recipe to Comment.
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Recipe)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RecipeId)
                .OnDelete(DeleteBehavior.Restrict); // Change from Cascade to Restrict

            // --- Proactive Fix for the Favorite Relationship ---
            // You will have the EXACT same problem with Favorites, so we'll fix it now.
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Recipe)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RecipeId)
                .OnDelete(DeleteBehavior.Restrict); // Change from Cascade to Restrict
        }
    }
}
