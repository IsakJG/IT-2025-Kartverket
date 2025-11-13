namespace Kartverket.Web.Models.Entities;
public class Image
{
    public int ImageId { get; set; }
    public string ImageUrl { get; set; } = "";
    public int? ImageHeight { get; set; }
    public int? ImageLength { get; set; }
    public List<Report> Reports { get; set; } = new();
}