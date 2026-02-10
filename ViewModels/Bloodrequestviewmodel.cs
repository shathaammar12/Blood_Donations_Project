using System.ComponentModel.DataAnnotations;

namespace Blood_Donations_Project.ViewModels
{
    public class BloodRequestViewModel
    {
        [Required(ErrorMessage = "Blood type is required")]
        [Display(Name = "Blood Type")]
        public int BloodTypeId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100)]
        [Display(Name = "Quantity (units)")]
        public int Quantity { get; set; }

    }
}