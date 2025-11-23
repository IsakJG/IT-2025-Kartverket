namespace Kartverket.Web.Models.Entities;
public class TimestampEntry
{
    public int DateId { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateOfLastChange { get; set; }
    public List<Report> Reports { get; set; } = new();
}