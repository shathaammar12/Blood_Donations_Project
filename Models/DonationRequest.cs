namespace Blood_Donations_Project.Models
{
    public partial class DonationRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateOnly RequestDate { get; set; }
        public string Status { get; set; } = "Pending";

        public int? ApprovedBy { get; set; }
        public DateOnly? ApprovedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual User? ApprovedByNavigation { get; set; }
    }


}
