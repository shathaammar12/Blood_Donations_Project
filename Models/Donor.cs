using System;
using System.Collections.Generic;

namespace Blood_Donations_Project.Models;

public partial class Donor
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? BloodTypeId { get; set; }

    public DateOnly? LastDonationDate { get; set; }

    public bool? IsAvailable { get; set; }

    public bool IsMedicalVerified { get; set; } = false;
    public int? MedicalVerifiedBy { get; set; }
    public DateOnly? MedicalVerifiedDate { get; set; }
    public string? HealthStatus { get; set; }

    public virtual BloodType? BloodType { get; set; }

    public virtual User? User { get; set; }
}
