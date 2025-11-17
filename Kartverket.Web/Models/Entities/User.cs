using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public int OrgId { get; set; }
        public Organization? Organization { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = "";
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        [Required]
        public string PasswordHash { get; set; } = ""; // Endret fra Password
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties (behold eksisterende)
        public List<Report> Reports { get; set; } = new();
        public List<Report> AssignedReports { get; set; } = new();
        public List<Report> DecidedReports { get; set; } = new();
        public List<UserRole> UserRoles { get; set; } = new();
    }
}