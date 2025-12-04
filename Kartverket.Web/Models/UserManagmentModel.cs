using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for listing users in the administration interface.
    /// Designed as a lightweight DTO for read-only tables.
    /// </summary>
    public class UserListViewModel
    {
        [Display(Name = "User ID")]
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}