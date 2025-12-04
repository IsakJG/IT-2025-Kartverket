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

        [Display(Name = "Name of Obstacle")]
        [MaxLength(100, ErrorMessage = "Obstacle name cannot be longer than 100 characters.")]
        public string? ObstacleName { get; set; } // Nullable for å tillate lagring av uferdige utkast (Drafts)

        [Display(Name = "Height (feet)")]
        // Satte en realistisk maksgrense (Mount Everest er ca 29k fot). 50k dekker alt av hindre.
        [Range(0, 50000, ErrorMessage = "Height must be between 0 and 50,000 feet.")]
        public double? ObstacleHeight { get; set; }

        [Display(Name = "Description")]
        [MaxLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string? ObstacleDescription { get; set; }
        
        // Viktig: Geografisk validering sikrer at vi ikke lagrer ugyldige koordinater i databasen.
        
        [Display(Name = "Latitude")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; set; }

        [Display(Name = "Longitude")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
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