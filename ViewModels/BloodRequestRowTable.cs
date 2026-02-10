namespace Blood_Donations_Project.ViewModels
{
    public class BloodRequestRowTable
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? BloodTypeId { get; set; }
        public string? BloodTypeName { get; set; }
        public DateOnly? RequestDate { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
    }
}
