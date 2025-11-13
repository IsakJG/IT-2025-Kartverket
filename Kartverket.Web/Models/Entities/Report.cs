namespace Kartverket.Web.Models.Entities;

public class Report
{
    public int ReportId { get; set; }

    // Foreign keys (kan være null hvis ikke satt ennå)
    public int? UserId { get; set; }
    public User? User { get; set; }                 // Brukeren som opprettet rapporten

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }       // Hvem den er tildelt

    public int? DecisionByUserId { get; set; }
    public User? DecisionByUser { get; set; }       // Hvem som tok avgjørelsen

    public int? ImageId { get; set; }
    public Image? Image { get; set; }

    public int? StatusId { get; set; }
    public Status? Status { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? DateId { get; set; }
    public TimestampEntry? TimestampEntry { get; set; }

    // Vanlige felter
    public string? Title { get; set; }              // TinyText i ER-diagrammet
    public string? GeoLocation { get; set; }        // GeoJSON som string
    public short? HeightInFeet { get; set; }        // SmallInt
    public string? Description { get; set; }        // MediumText

    public DateTime? AssignedAt { get; set; }       // TIMESTAMP
    public string? Feedback { get; set; }           // MediumText
    public DateTime? DecisionAt { get; set; }       // DATETIME
}
