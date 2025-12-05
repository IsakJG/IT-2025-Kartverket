using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    /// <summary>
    /// Representerer tilstanden en rapport kan befinne seg i (f.eks. Pending, Approved, Rejected, Draft).
    /// Styrer arbeidsflyten i applikasjonen.
    /// </summary>
    public class Status
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Liste over alle rapporter som har denne statusen.
        /// </summary>
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}