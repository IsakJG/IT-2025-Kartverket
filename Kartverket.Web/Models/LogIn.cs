using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models;
public class LogIn //Definerer strukturen for log-in 
{
    [Required(ErrorMessage = "Field is required")]
    [MaxLength(100)]
    public string Username { get; set; }

    [Required(ErrorMessage = "Field is required")]
    [MinLength(8, ErrorMessage = "Password must be between 8-100 characters")]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

}
