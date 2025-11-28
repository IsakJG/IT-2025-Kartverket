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

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(100, ErrorMessage = "Password cannot be longer than 100 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }      // required by Create view

        [Required]
        public int? RoleId { get; set; }          // required by Create view

        public string Role { get; set; }          // used for display in list
    }
}