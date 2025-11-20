namespace Kartverket.Web.Models
{
    public class ActiveReportRow
    {
        public int ReportId { get; set; }
       
        public string Position { get; set; } = "";
        public string Height { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
}