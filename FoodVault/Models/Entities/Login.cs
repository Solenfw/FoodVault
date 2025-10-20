using System;
using System.Collections.Generic;

namespace FoodVault.Models.Entities;

public partial class Login
{
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual User IdNavigation { get; set; } = null!;
}
