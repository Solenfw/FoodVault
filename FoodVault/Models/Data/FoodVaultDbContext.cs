using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

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


    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<RecipeTag> RecipeTags { get; set; }

    public virtual DbSet<Step> Steps { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: Call base configuration first to set up Identity entities
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__favorite__3213E83F257C4F7D");

            entity.ToTable("favorites");

            entity.HasIndex(e => e.RecipeId, "idx_favorites_recipe_id");

            entity.HasIndex(e => e.UserId, "idx_favorites_user_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.FavoritedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("favorited_at");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(450)
                .HasColumnName("recipe_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__favorites__recip__693CA210");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__favorites__user___68487DD7");
        });

        modelBuilder.Entity<Fridge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__fridges__3213E83F4803E95E");

            entity.ToTable("fridges");

            entity.HasIndex(e => e.UserId, "idx_fridges_user_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(450)
                .HasColumnName("name");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Fridges)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__fridges__user_id__6D0D32F4");
        });

        modelBuilder.Entity<FridgeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__fridge_i__3213E83F4092CECB");

            entity.ToTable("fridge_ingredients");

            entity.HasIndex(e => e.FridgeId, "idx_fridge_ingredients_fridge_id");

            entity.HasIndex(e => e.IngredientId, "idx_fridge_ingredients_ingredient_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.FridgeId)
                .HasMaxLength(450)
                .HasColumnName("fridge_id");
            entity.Property(e => e.IngredientId)
                .HasMaxLength(450)
                .HasColumnName("ingredient_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");

            entity.HasOne(d => d.Fridge).WithMany(p => p.FridgeIngredients)
                .HasForeignKey(d => d.FridgeId)
                .HasConstraintName("FK__fridge_in__fridg__6FE99F9F");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.FridgeIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__fridge_in__ingre__70DDC3D8");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ingredie__3213E83FEA6B7C18");

            entity.ToTable("ingredients");

            entity.HasIndex(e => e.Name, "UQ__ingredie__72E12F1B07EEA366").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.DefaultCalories).HasColumnName("default_calories");
            entity.Property(e => e.DefaultCarbs).HasColumnName("default_carbs");
            entity.Property(e => e.DefaultFat).HasColumnName("default_fat");
            entity.Property(e => e.DefaultProtein).HasColumnName("default_protein");
            entity.Property(e => e.DefaultUnit)
                .HasMaxLength(50)
                .HasColumnName("default_unit");
            entity.Property(e => e.Name)
                .HasMaxLength(450)
                .HasColumnName("name");
        });

        // ProfileUser removed: properties merged into Identity User

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ratings__3213E83F52B2222D");

            entity.ToTable("ratings");

            entity.HasIndex(e => e.RecipeId, "idx_ratings_recipe_id");

            entity.HasIndex(e => e.UserId, "idx_ratings_user_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.RatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("rated_at");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(450)
                .HasColumnName("recipe_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ratings__recipe___75A278F5");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ratings__user_id__74AE54BC");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recipes__3213E83F2EBDB950");

            entity.ToTable("recipes");

            entity.HasIndex(e => e.UserId, "idx_recipes_user_id");
            
            entity.Property(e => e.ImageUrl)    
            .HasMaxLength(500)    
            .HasColumnName("image_url");
            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.CookTimeMinutes).HasColumnName("cook_time_minutes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PrepTimeMinutes).HasColumnName("prep_time_minutes");
            entity.Property(e => e.Servings).HasColumnName("servings");
            entity.Property(e => e.Title)
                .HasMaxLength(450)
                .HasColumnName("title");
            entity.Property(e => e.TotalCalories).HasColumnName("total_calories");
            entity.Property(e => e.TotalCarbs).HasColumnName("total_carbs");
            entity.Property(e => e.TotalFat).HasColumnName("total_fat");
            entity.Property(e => e.TotalProtein).HasColumnName("total_protein");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recipes__user_id__5441852A");
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recipe_i__3213E83F4DF4833E");

            entity.ToTable("recipe_ingredients");

            entity.HasIndex(e => e.IngredientId, "idx_recipe_ingredients_ingredient_id");

            entity.HasIndex(e => e.RecipeId, "idx_recipe_ingredients_recipe_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.Carbs).HasColumnName("carbs");
            entity.Property(e => e.Fat).HasColumnName("fat");
            entity.Property(e => e.IngredientId)
                .HasMaxLength(450)
                .HasColumnName("ingredient_id");
            entity.Property(e => e.Protein).HasColumnName("protein");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(450)
                .HasColumnName("recipe_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.IngredientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recipe_in__ingre__5AEE82B9");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK__recipe_in__recip__59FA5E80");
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__recipe_t__3213E83F775FA163");

            entity.ToTable("recipe_tags");

            entity.HasIndex(e => e.RecipeId, "idx_recipe_tags_recipe_id");

            entity.HasIndex(e => e.TagId, "idx_recipe_tags_tag_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(450)
                .HasColumnName("recipe_id");
            entity.Property(e => e.TagId)
                .HasMaxLength(450)
                .HasColumnName("tag_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeTags)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK__recipe_ta__recip__6383C8BA");

            entity.HasOne(d => d.Tag).WithMany(p => p.RecipeTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__recipe_ta__tag_i__6477ECF3");
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__steps__3213E83F8FB6C176");

            entity.ToTable("steps");

            entity.HasIndex(e => e.RecipeId, "idx_steps_recipe_id");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.Instruction).HasColumnName("instruction");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(450)
                .HasColumnName("recipe_id");
            entity.Property(e => e.StepNumber).HasColumnName("step_number");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Steps)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK__steps__recipe_id__5DCAEF64");
        });
        // No one-to-one between User and ProfileUser anymore
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tags__3213E83F1D3567A9");

            entity.ToTable("tags");

            entity.HasIndex(e => e.Name, "UQ__tags__72E12F1B20937584").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(450)
                .HasColumnName("name");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder); }