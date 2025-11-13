using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Web.Models
{
    public class UserAdminViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}