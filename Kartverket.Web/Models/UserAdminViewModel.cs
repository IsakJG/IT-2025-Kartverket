using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    public class UserAdminViewModel
    {
        public int Id { get; set; }               // used by listing
        public string Name { get; set; }
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }         // required by Create view

        [DataType(DataType.Password)]
        public string Password { get; set; }      // required by Create view

        [Required]
        public int? RoleId { get; set; }          // required by Create view

        public string Role { get; set; }          // used for display in list
    }
}