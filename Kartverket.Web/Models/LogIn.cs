using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for innlogging. 
    /// Inneholder legitimasjon som sendes fra klient til server ved autentisering.
    /// </summary>
    public class LogIn
    {
        [Required(ErrorMessage = "Username must be filled in")]
        [MaxLength(100, ErrorMessage = "Username cannot be longer than 100 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password must be filled in.")]
        // StringLength kombinerer sjekk av minimum og maksimum lengde
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}