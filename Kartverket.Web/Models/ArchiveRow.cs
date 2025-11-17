namespace Kartverket.Web.Models;

public class ArchiveRow
{
    public int ReportID { get; set; }
    public string? Title { get; set; }
    public string? Pilot { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public double? HeightInFeet { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? DecisionAt { get; set; }
}