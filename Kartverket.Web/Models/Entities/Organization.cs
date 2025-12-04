using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    public class Organization
    {
        [Key]
        public int OrgId { get; set; }

        [Required(ErrorMessage = "Organization name is required.")]
        [StringLength(50, ErrorMessage = "Organization name cannot exceed 50 characters.")]
        public string OrgName { get; set; } = string.Empty;

        // Navigation property
        // Bruk virtual ICollection for korrekt EF Core oppførsel
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}