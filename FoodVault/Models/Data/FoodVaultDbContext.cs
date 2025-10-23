using System;
using System.Collections.Generic;
using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // THÊM DÒNG NÀY
using Microsoft.AspNetCore.Identity; // THÊM DÒNG NÀY
using Microsoft.EntityFrameworkCore;
using FoodVault.Models.Data.Entities;
namespace FoodVault.Models.Data;

public partial class FoodVaultDbContext : IdentityDbContext<User>
{
    public FoodVaultDbContext()
    {
    }

    public FoodVaultDbContext(DbContextOptions<FoodVaultDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Fridge> Fridges { get; set; }

    public virtual DbSet<FridgeIngredient> FridgeIngredients { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Login> Logins { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<RecipeTag> RecipeTags { get; set; }

    public virtual DbSet<Step> Steps { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-JNPUHSM7\\SQLEXPRESS;Database=FoodVault;User Id=sa;Password=06062005;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__favorite__3213E83F15A400B3");

            entity.ToTable("favorites");

            entity.HasIndex(e => e.RecipeId, "IX_favorites_recipe_id");

            entity.HasIndex(e => e.UserId, "IX_favorites_user_id");

            entity.HasIndex(e => new { e.UserId, e.RecipeId }, "UQ_favorites_user_recipe").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.FavoritedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("favorited_at");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("recipe_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_recipes");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_users");
        });

        modelBuilder.Entity<Fridge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__fridges__3213E83FE08B7873");

            entity.ToTable("fridges");

            entity.HasIndex(e => e.UserId, "IX_fridges_user_id");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Fridges)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fridges_users");
        });

        modelBuilder.Entity<FridgeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__fridge_i__3213E83F1CDBB8C4");

            entity.ToTable("fridge_ingredients");

            entity.HasIndex(e => e.FridgeId, "IX_fridge_ingredients_fridge_id");

            entity.HasIndex(e => e.IngredientId, "IX_fridge_ingredients_ingredient_id");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.FridgeId)
                .HasMaxLength(50)
                .HasColumnName("fridge_id");
            entity.Property(e => e.IngredientId)
                .HasMaxLength(50)
                .HasColumnName("ingredient_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");

            entity.HasOne(d => d.Fridge).WithMany(p => p.FridgeIngredients)
                .HasForeignKey(d => d.FridgeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fridge_ingredients_fridges");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.FridgeIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fridge_ingredients_ingredients");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ingredie__3213E83F39D23340");

            entity.ToTable("ingredients");

            entity.HasIndex(e => e.Name, "UQ__ingredie__72E12F1B4CE953AF").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.DefaultCalories).HasColumnName("default_calories");
            entity.Property(e => e.DefaultCarbs).HasColumnName("default_carbs");
            entity.Property(e => e.DefaultFat).HasColumnName("default_fat");
            entity.Property(e => e.DefaultProtein).HasColumnName("default_protein");
            entity.Property(e => e.DefaultUnit)
                .HasMaxLength(50)
                .HasColumnName("default_unit");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Login>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__login__3213E83FF086A306");

            entity.ToTable("login");

            entity.HasIndex(e => e.Username, "UQ__login__F3DBC572C9102AE6").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Login)
                .HasForeignKey<Login>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_login_users");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ratings__3213E83FCB85D89F");

            entity.ToTable("ratings");

            entity.HasIndex(e => e.RecipeId, "IX_ratings_recipe_id");

            entity.HasIndex(e => e.UserId, "IX_ratings_user_id");

            entity.HasIndex(e => new { e.UserId, e.RecipeId }, "UQ_ratings_user_recipe").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.RatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("rated_at");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("recipe_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ratings_recipes");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ratings_users");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recipes__3213E83F00A6AF5E");

            entity.ToTable("recipes");

            entity.HasIndex(e => e.UserId, "IX_recipes_user_id");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.CookTimeMinutes).HasColumnName("cook_time_minutes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PrepTimeMinutes).HasColumnName("prep_time_minutes");
            entity.Property(e => e.Servings).HasColumnName("servings");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TotalCalories).HasColumnName("total_calories");
            entity.Property(e => e.TotalCarbs).HasColumnName("total_carbs");
            entity.Property(e => e.TotalFat).HasColumnName("total_fat");
            entity.Property(e => e.TotalProtein).HasColumnName("total_protein");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipes_users");
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recipe_i__3213E83F61927005");

            entity.ToTable("recipe_ingredients");

            entity.HasIndex(e => e.IngredientId, "IX_recipe_ingredients_ingredient_id");

            entity.HasIndex(e => e.RecipeId, "IX_recipe_ingredients_recipe_id");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.Carbs).HasColumnName("carbs");
            entity.Property(e => e.Fat).HasColumnName("fat");
            entity.Property(e => e.IngredientId)
                .HasMaxLength(50)
                .HasColumnName("ingredient_id");
            entity.Property(e => e.Protein).HasColumnName("protein");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("recipe_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_ingredients_ingredients");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_ingredients_recipes");
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recipe_t__3213E83FA4B4E6A1");

            entity.ToTable("recipe_tags");

            entity.HasIndex(e => e.RecipeId, "IX_recipe_tags_recipe_id");

            entity.HasIndex(e => e.TagId, "IX_recipe_tags_tag_id");

            entity.HasIndex(e => new { e.RecipeId, e.TagId }, "UQ_recipe_tags_recipe_tag").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("recipe_id");
            entity.Property(e => e.TagId)
                .HasMaxLength(50)
                .HasColumnName("tag_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeTags)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_tags_recipes");

            entity.HasOne(d => d.Tag).WithMany(p => p.RecipeTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_tags_tags");
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__steps__3213E83FE2DEC465");

            entity.ToTable("steps");

            entity.HasIndex(e => e.RecipeId, "IX_steps_recipe_id");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("recipe_id");
            entity.Property(e => e.StepNumber).HasColumnName("step_number");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Steps)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_steps_recipes");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tags__3213E83FECAC7F1B");

            entity.ToTable("tags");

            entity.HasIndex(e => e.Name, "UQ__tags__72E12F1BCA82C50C").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");

            // Map các cột theo đúng database
            b.Property(u => u.Id)
                .HasMaxLength(50)
                .HasColumnName("id");

            b.Property(u => u.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            b.Property(u => u.Email)
                .HasMaxLength(255)
                .HasColumnName("email");

            b.Property(u => u.NormalizedEmail)
                .HasMaxLength(255)
                .HasColumnName("normalized_email");

          
            b.Property(u => u.NormalizedUserName)
                .HasMaxLength(100)
                .HasColumnName("normalized_name");

            b.Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            b.Property(u => u.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");

            b.Property(u => u.DietaryPreferences)
                .HasMaxLength(500)
                .HasColumnName("dietary_preferences"); // Sửa thành dietary_preferences

            b.Property(u => u.DietaryRestrictions)
                .HasMaxLength(500)
                .HasColumnName("dietary_restrictions"); // Sửa thành dietary_restrictions

            b.Property(u => u.ActivityHistory)
                .HasColumnName("activity_history");

            b.Property(u => u.Role)
                .HasMaxLength(20)
                .HasDefaultValue("user")
                .HasColumnName("role");

            b.Property(u => u.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");

          

            // Cấu hình indexes từ database
            b.HasIndex(u => u.PasswordHash)
                .HasDatabaseName("UQ__users__6F0B1C3108071BD")
                .IsUnique();

            b.HasIndex(u => u.Name)
                .HasDatabaseName("UQ__users__72E12F1B7DEFB9E6")
                .IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
