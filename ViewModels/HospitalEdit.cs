using System.ComponentModel.DataAnnotations;

namespace Blood_Donations_Project.ViewModels
{
    public class HospitalEdit
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? MobileNo { get; set; }
        public string? Address { get; set; }
    }
}
