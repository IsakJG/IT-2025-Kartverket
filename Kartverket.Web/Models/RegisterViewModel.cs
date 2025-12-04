using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for registrering av nye brukere.
    /// Inneholder valideringsregler som sikrer at dataene er korrekte før de sendes til kontrolleren.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Brukernavn er påkrevd.")]
        [StringLength(50, ErrorMessage = "Brukernavn kan ikke være lengre enn 50 tegn.")]
        [Display(Name = "Brukernavn")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-post er påkrevd.")]
        [EmailAddress(ErrorMessage = "Vennligst oppgi en gyldig e-postadresse.")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passord er påkrevd.")]
        // StringLength er mer konsist enn MinLength/MaxLength
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Passordet må være mellom 8 og 100 tegn.")]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du må bekrefte passordet.")]
        // nameof(Password) sikrer at koden tåler refaktorering (endring av variabelnavn)
        [Compare(nameof(Password), ErrorMessage = "Passordene er ikke like.")]
        [DataType(DataType.Password)]
        [Display(Name = "Bekreft passord")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Du må velge en organisasjon.")]
        [Display(Name = "Organisasjon")]
        // Nullable int sikrer at brukeren aktivt må velge i listen (hindrer default 0)
        public int? OrgId { get; set; } 
    }
}