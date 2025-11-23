namespace Kartverket.Web.Models.Entities;
public class Status
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = "";
    public List<Report> Reports { get; set; } = new();
}
