using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models;
public class ObstacleData
{
    [MaxLength(100)]
    public string ObstacleName { get; set; }

    [Required(ErrorMessage = "Field is required")]
    [Range(0, 200)]
    public double ObstacleHeight { get; set; }

    [MaxLength(1000)]
    public string ObstacleDescription { get; set; }
    
    //La til lat og long for kartintegrasjon
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsDraft { get; set; }
}
