using System.ComponentModel.DataAnnotations;

namespace Blood_Donations_Project.ViewModels
{
    public class HospitalCreate
    {
        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? MobileNo { get; set; }
        public string? Address { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
