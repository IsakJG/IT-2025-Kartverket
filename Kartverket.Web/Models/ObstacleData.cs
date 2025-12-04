using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models;
public class ObstacleData
{
    public int? ReportId {get; set;}
    [MaxLength(100)]
    public string ObstacleName { get; set; }

    
    [Range(0, 20000000)]// sette  max verdi til noe fornuftig
    public double? ObstacleHeight { get; set; }

    [MaxLength(1000)]
    public string ObstacleDescription { get; set; }
    
   
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsDraft { get; set; }
    
    // Add this property to store the GeoJSON geometry
    public string? GeometryGeoJson { get; set; }
}
