using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Web.Models
{
    // Renamed to avoid duplicate type definitions.
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}