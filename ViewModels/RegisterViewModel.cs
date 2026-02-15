using System.ComponentModel.DataAnnotations;

namespace Blood_Donations_Project.ViewModels
{
    public class RegisterViewModel
    {
        //[Required(ErrorMessage = "Role is required")]
        //public int RoleId { get; set; } // 2=Donor, 3=Hospital, 4=BloodBank

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Phone, StringLength(100)]
        public string MobileNo { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500, ErrorMessage = "Max 500 characters")]
        public string? HealthStatus { get; set; }

        [Required(ErrorMessage = "Blood type is required")]
        public int? BloodTypeId { get; set; }
    }
}
