using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        public string TechnicalStack { get; set; } = string.Empty;

        [Required]
        public string ResearchArea { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string StudentId { get; set; } = string.Empty;

        public string? SupervisorId { get; set; }

        public bool IsRevealed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}