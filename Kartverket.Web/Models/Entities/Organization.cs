namespace Kartverket.Web.Models.Entities;

    public class Organization
    {
        public int OrgId { get; set; }
        public string OrgName { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
