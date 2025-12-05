using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel som representerer en enkelt rad i oversikten over aktive rapporter.
    /// Optimalisert for visning i "Registar"-dashboardet.
    /// </summary>
    public class ActiveReportRow
    {
        [Display(Name = "ID")]
        public int ReportId { get; set; }
       
        [Display(Name = "Posisjon")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Høyde")]
        public string Height { get; set; } = string.Empty;

        [Display(Name = "Innsender")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Opprettet dato")]
        [DataType(DataType.Date)] // Hjelper Razor å formatere datoen pent
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
    }
}