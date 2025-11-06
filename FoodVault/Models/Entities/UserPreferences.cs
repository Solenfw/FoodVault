using System;

namespace FoodVault.Models.Entities;

public sealed class UserPreferences
{
	public string Id { get; set; } = null!;

	public string UserId { get; set; } = null!;

	// light | dark | auto
	public string Theme { get; set; } = "auto";

	public DateTime? UpdatedAt { get; set; }

	public User User { get; set; } = null!;
}


