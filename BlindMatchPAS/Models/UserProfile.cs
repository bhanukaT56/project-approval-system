using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Batch")]
        public string? Batch { get; set; }

        [Display(Name = "Degree Program")]
        public string? DegreeProgram { get; set; }

        [Display(Name = "Department")]
        public string? Department { get; set; }
    }
}