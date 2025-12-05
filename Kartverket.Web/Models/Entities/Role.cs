using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    /// <summary>
    /// Representerer en tilgangsrolle i systemet (f.eks. Pilot, Registrar, Admin).
    /// </summary>
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        /// <summary>
        /// Navnet på rollen. Må matche systemdefinerte roller (Admin, Pilot, osv).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Beskrivelse av hva rollen innebærer og hvilke tilganger den gir.
        /// (Clean Code note: Burde ideelt sett hete 'Description' for å unngå forkortelser).
        /// </summary>
        [StringLength(255)]
        public string? RoleDesc { get; set; }

        // Navigasjonsegenskap til koblingstabellen UserRole.
        // Virtual muliggjør Lazy Loading.
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}