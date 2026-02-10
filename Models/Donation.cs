using System;
using System.Collections.Generic;

namespace Blood_Donations_Project.Models;

public partial class Donation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ApprovedBy { get; set; }

    public DateOnly? DonationDate { get; set; }

    public string? Status { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
