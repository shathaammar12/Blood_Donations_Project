namespace Blood_Donations_Project.Models
{
    public class BloodInventory
    {
        public int Id { get; set; }

        public int BloodTypeId { get; set; }
        public BloodType? BloodType { get; set; }

        public int UnitsAvailable { get; set; }
    }

}
