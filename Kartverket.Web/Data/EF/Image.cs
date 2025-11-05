using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class Image
{
    public int ImageID { get; set; }

    public string ImageURL { get; set; } = null!;

    public int? ImageHeight { get; set; }

    public int? ImageLength { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
