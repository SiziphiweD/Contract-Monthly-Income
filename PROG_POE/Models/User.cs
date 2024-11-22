using System;
using System.Collections.Generic;

namespace PROG_POE.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Claim> ClaimApprovedByNavigations { get; set; } = new List<Claim>();

    public virtual ICollection<Claim> ClaimLecturers { get; set; } = new List<Claim>();
}
