using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for innlogging. 
    /// Inneholder legitimasjon som sendes fra klient til server ved autentisering.
    /// </summary>
    public class LogIn
    {
        [Required(ErrorMessage = "Brukernavn må fylles ut.")]
        [MaxLength(100, ErrorMessage = "Brukernavnet kan ikke være lengre enn 100 tegn.")]
        [Display(Name = "Brukernavn")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passord må fylles ut.")]
        // StringLength kombinerer sjekk av minimum og maksimum lengde
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Passordet må være mellom 8 og 100 tegn.")]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; } = string.Empty;
    }
}