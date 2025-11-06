using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Kartverket.Web.Data.EF;

public partial class Report
{
    public int ReportID { get; set; }

    public string Title { get; set; } = null!;

    public int UserID { get; set; }

    public Point? GeoLocation { get; set; }

    public decimal HeightInFeet { get; set; }

    public string? ReportDescription { get; set; }
    

    public int? ImageID { get; set; }

    public int? StatusID { get; set; }

    public int? CategoryID { get; set; }

    public string? Feedback { get; set; }

    public int? DateID { get; set; }

    public int? AssignedToUser { get; set; }

    public DateTime? AssignedAt { get; set; }

    public int? DecisionByUser { get; set; }

    public DateTime? DecisionAt { get; set; }

    public virtual User? AssignedToUserNavigation { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual DateTimestamp? Date { get; set; }

    public virtual User? DecisionByUserNavigation { get; set; }

    public virtual Image? Image { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
