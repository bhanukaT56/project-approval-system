using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlindMatchPAS.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Project title is required.")]
        [StringLength(200, MinimumLength = 5,
            ErrorMessage = "Title must be between 5 and 200 characters.")]
        [Display(Name = "Project Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Abstract is required.")]
        [StringLength(2000, MinimumLength = 50,
            ErrorMessage = "Abstract must be between 50 and 2000 characters.")]
        [Display(Name = "Project Abstract")]
        public string Abstract { get; set; } = string.Empty;

        [Required(ErrorMessage = "Technical stack is required.")]
        [StringLength(500, MinimumLength = 2,
            ErrorMessage = "Technical stack must be between 2 and 500 characters.")]
        [Display(Name = "Technical Stack")]
        public string TechnicalStack { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a research area.")]
        [Display(Name = "Research Area")]
        public string ResearchArea { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string StudentId { get; set; } = string.Empty;

        public string? SupervisorId { get; set; }

        public bool IsRevealed { get; set; } = false;

        [Display(Name = "Submitted On")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("StudentId")]
        public UserProfile? StudentProfile { get; set; }

        [ForeignKey("SupervisorId")]
        public UserProfile? SupervisorProfile { get; set; }
    }
}