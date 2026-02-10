using System;
using System.Collections.Generic;

namespace Blood_Donations_Project.Models;

public partial class BloodRequest
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? BloodTypeId { get; set; }

    public DateOnly? RequestDate { get; set; }

    public string? Status { get; set; }

    public int? Quantity { get; set; }
}
