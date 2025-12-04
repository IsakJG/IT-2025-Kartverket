using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for innlogging.
    /// Inneholder data og valideringsregler for innloggingsskjemaet.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Brukerens brukernavn.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Brukerens passord.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)] // Forteller Viewet at dette skal være et passordfelt (skjult tekst)
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// En enkel brukerklasse brukt for midlertidig/mock data i AccountController.
    /// (Navngitt 'MockUser' for å unngå konflikt med database-entiteten 'Kartverket.Web.Models.Entities.User').
    /// </summary>
    public class MockUser
    {
        public string Username { get; set; } = string.Empty;
        
        // Merk: I produksjon skal passord aldri lagres i klartekst, men som hash.
        public string Password { get; set; } = string.Empty; 

        public string Role { get; set; } = string.Empty;
    }
}