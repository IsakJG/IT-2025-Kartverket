using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    /// <summary>
    /// Koblingstabell (Junction Table) for mange-til-mange forholdet mellom User og Role.
    /// Definerer hvilke roller en spesifikk bruker har tilgang til.
    /// </summary>
    public class UserRole
    {
        // Del av Composite Primary Key + Foreign Key til User-tabellen
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Del av Composite Primary Key + Foreign Key til Role-tabellen
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
}