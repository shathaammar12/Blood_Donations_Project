using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Blood_Donations_Project.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }
    public string? FullName { get; set; }

    public string? Email { get; set; }
    public bool IsAvailable { get; set; }
    public string? Address { get; set; }

    public string? MobileNo { get; set; }

    public string? Password { get; set; }

    public int RoleId { get; set; }
    public DateTime DateOfBirth { get; set; }

    public virtual ICollection<Donation> DonationApprovedByNavigations { get; set; } = new List<Donation>();

    public virtual ICollection<Donation> DonationUsers { get; set; } = new List<Donation>();

    public virtual ICollection<Donor> Donors { get; set; } = new List<Donor>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
