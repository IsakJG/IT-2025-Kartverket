using System;
using System.Collections.Generic;

namespace Kartverket.Web.Data.EF;

public partial class DateTimestamp
{
    public int DateID { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateOfLastChange { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
