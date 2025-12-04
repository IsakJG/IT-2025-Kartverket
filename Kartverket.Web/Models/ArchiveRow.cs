using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for visning av historiske rapporter i Arkivet.
    /// Brukes av både Admin, Registrar og Pilot (for egne rapporter).
    /// </summary>
    public class ArchiveRow
    {
        [Display(Name = "ID")]
        public int ReportId { get; set; } // Endret fra ReportID for konsekvens (CamelCase/PascalCase)

        [Display(Name = "Tittel")]
        public string? Title { get; set; }

        [Display(Name = "Innsender")]
        public string? Pilot { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Kategori")]
        public string? Category { get; set; }

        [Display(Name = "Høyde (ft)")]
        public double? HeightInFeet { get; set; }

        [Display(Name = "Beskrivelse")]
        public string? Description { get; set; }

        // Koordinater for enkel visning
        [Display(Name = "Breddegrad")]
        public double? Latitude { get; set; }

        [Display(Name = "Lengdegrad")]
        public double? Longitude { get; set; }

        [Display(Name = "Opprettet")]
        [DataType(DataType.Date)]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Tildelt")]
        [DataType(DataType.Date)]
        public DateTime? AssignedAt { get; set; }

        [Display(Name = "Avgjort")]
        [DataType(DataType.Date)]
        public DateTime? DecisionAt { get; set; }
        
        /// <summary>
        /// Rådata for kartvisning (GeoJSON).
        /// </summary>
        public string? GeometryGeoJson { get; set; }
    }
}