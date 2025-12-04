using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)] // Matcher det du satte i DbContext
        public string CategoryName { get; set; } = string.Empty;

        // Bruk virtual ICollection for relasjoner
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}