using Blood_Donations_Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Blood_Donations_Project.ViewModels
{
    public class Profile
    {
        public int UserId { get; set; }

        [Required]
        public string? FullName { get; set; }

        public string? UserName { get; set; } 

        [EmailAddress]
        public string? Email { get; set; }

        public string? MobileNo { get; set; }

        public string? Address { get; set; }

        public string? RoleName { get; set; } 
        public int? BloodTypeId { get; set; }
        public string? BloodTypeName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? HealthStatus { get; set; }
        public string? Gender { get; set; }
    }
}
