using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for skjemaet hvor piloter registrerer eller redigerer hindre.
    /// Inneholder både metadata (navn, høyde) og geografiske data (Lat/Lng, GeoJSON).
    /// </summary>
    public class ObstacleData
    {
        public int? ReportId { get; set; }

        [Display(Name = "Navn på hinder")]
        [MaxLength(100, ErrorMessage = "Navnet kan ikke være lengre enn 100 tegn.")]
        public string? ObstacleName { get; set; } // Nullable for å tillate lagring av uferdige utkast (Drafts)

        [Display(Name = "Høyde (fot)")]
        // Satte en realistisk maksgrense (Mount Everest er ca 29k fot). 50k dekker alt av hindre.
        [Range(0, 50000, ErrorMessage = "Høyde må være mellom 0 og 50 000 fot.")]
        public double? ObstacleHeight { get; set; }

        [Display(Name = "Beskrivelse")]
        [MaxLength(1000, ErrorMessage = "Beskrivelsen kan ikke være lengre enn 1000 tegn.")]
        public string? ObstacleDescription { get; set; }
        
        // Viktig: Geografisk validering sikrer at vi ikke lagrer ugyldige koordinater i databasen.
        
        [Display(Name = "Breddegrad")]
        [Range(-90, 90, ErrorMessage = "Breddegrad må være mellom -90 og 90.")]
        public double Latitude { get; set; }

        [Display(Name = "Lengdegrad")]
        [Range(-180, 180, ErrorMessage = "Lengdegrad må være mellom -180 og 180.")]
        public double Longitude { get; set; }

        /// <summary>
        /// Angir om dette er et utkast (true) eller innsending (false).
        /// </summary>
        public bool IsDraft { get; set; }
        
        /// <summary>
        /// Rådata for kartgeometri (Punkt, Linje eller Polygon) i GeoJSON-format.
        /// </summary>
        public string? GeometryGeoJson { get; set; }
    }
}