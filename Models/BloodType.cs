using System;
using System.Collections.Generic;

namespace Blood_Donations_Project.Models;

public partial class BloodType
{
    public int BloodTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Donor> Donors { get; set; } = new List<Donor>();
}
