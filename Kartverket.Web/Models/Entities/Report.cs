using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Web.Models.Entities
{
    /// <summary>
    /// Representerer en innrapportert hindring (Obstacle Report) i systemet.
    /// Inneholder all metadata, geolokasjon, statusflyt og saksbehandlingsinformasjon.
    /// </summary>
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        // --- Relasjoner og Nøkler ---

        /// <summary>
        /// ID til brukeren som opprinnelig sendte inn rapporten.
        /// Kan være null dersom brukeren slettes (avhengig av slette-strategi).
        /// </summary>
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// ID til saksbehandleren (Registar/Admin) som rapporten er tildelt.
        /// </summary>
        public int? AssignedToUserId { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual User? AssignedToUser { get; set; }

        /// <summary>
        /// ID til personen som tok den endelige avgjørelsen (Godkjent/Avvist).
        /// </summary>
        public int? DecisionByUserId { get; set; }

        [ForeignKey("DecisionByUserId")]
        public virtual User? DecisionByUser { get; set; }

        public int? ImageId { get; set; }

        [ForeignKey("ImageId")]
        public virtual Image? Image { get; set; }

        public int? StatusId { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status? Status { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int? DateId { get; set; }

        [ForeignKey("DateId")]
        public virtual TimestampEntry? TimestampEntry { get; set; }

        // --- Datafelt ---

        /// <summary>
        /// Kort tittel eller navn på hindringen.
        /// </summary>
        [StringLength(255, ErrorMessage = "Tittelen kan ikke overstige 255 tegn.")]
        public string? Title { get; set; }

        /// <summary>
        /// Geografisk posisjon og utstrekning lagret som en GeoJSON-streng.
        /// Inneholder koordinater for punkt, linje eller polygon.
        /// </summary>
        public string? GeoLocation { get; set; }

        /// <summary>
        /// Hindringens høyde i fot (Feet).
        /// </summary>
        [Range(0, 50000, ErrorMessage = "Høyde må være en positiv verdi.")]
        public double HeightInFeet { get; set; }

        /// <summary>
        /// Detaljert beskrivelse av hindringen og relevante observasjoner.
        /// </summary>
        public string? Description { get; set; }

        // --- Saksbehandlingsdata ---

        /// <summary>
        /// Tidspunkt for når rapporten ble tildelt en saksbehandler.
        /// </summary>
        public DateTime? AssignedAt { get; set; }

        /// <summary>
        /// Tilbakemelding fra saksbehandler ved avvisning eller behov for mer info.
        /// </summary>
        public string? Feedback { get; set; }

        /// <summary>
        /// Tidspunkt for når endelig vedtak (godkjenning/avvisning) ble fattet.
        /// </summary>
        public DateTime? DecisionAt { get; set; }
    }
}