using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    /// <summary>
    /// Representerer en bruker i systemet (ansatt, pilot, administrator, etc.).
    /// Inneholder autentiseringsdata og relasjoner til rapporter de er involvert i.
    /// </summary>
    public class User
    {
        [Key]
        public int UserId { get; set; }

        // --- Tilhørighet ---
        
        public int OrgId { get; set; }

        [ForeignKey("OrgId")]
        public virtual Organization? Organization { get; set; }
        
        // --- Profil & Sikkerhet ---

        [Required]
        [StringLength(50, ErrorMessage = "Brukernavn kan ikke overstige 50 tegn.")]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Hashet passord (aldri lagre i klartekst!).
        /// </summary>
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // --- Navigasjonsegenskaper (Relasjoner) ---

        /// <summary>
        /// Rapporter som denne brukeren har opprettet (Forfatter).
        /// </summary>
        [InverseProperty("User")]
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

        /// <summary>
        /// Rapporter som er tildelt denne brukeren for behandling.
        /// </summary>
        [InverseProperty("AssignedToUser")]
        public virtual ICollection<Report> AssignedReports { get; set; } = new List<Report>();

        /// <summary>
        /// Rapporter hvor denne brukeren har tatt den endelige avgjørelsen (Godkjent/Avvist).
        /// </summary>
        [InverseProperty("DecisionByUser")]
        public virtual ICollection<Report> DecidedReports { get; set; } = new List<Report>();

        /// <summary>
        /// Brukerens roller (Many-to-Many via koblingstabell).
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}