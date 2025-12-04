using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    /// <summary>
    /// Håndterer revisjonsinformasjon (tidsstempling) for rapporter.
    /// Skilt ut i egen tabell for å standardisere sporing av opprettelse og endringer.
    /// </summary>
    public class TimestampEntry
    {
        [Key]
        public int DateId { get; set; }

        /// <summary>
        /// Tidspunkt for opprettelse.
        /// Settes automatisk av databasen (DEFAULT CURRENT_TIMESTAMP) hvis null ved lagring.
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// Tidspunkt for siste endring.
        /// Oppdateres ved redigering av rapporten.
        /// </summary>
        public DateTime? DateOfLastChange { get; set; }

        /// <summary>
        /// Relasjon til rapportene som bruker dette tidsstempelet.
        /// (I praksis ofte 1-til-1 i denne løsningen, men modellert som 1-til-mange).
        /// </summary>
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}