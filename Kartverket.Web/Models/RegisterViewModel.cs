using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Brukernavn er påkrevd")]
        [StringLength(50, ErrorMessage = "Brukernavn kan ikke være lengre enn 50 tegn")]
        [Display(Name = "Brukernavn")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "E-post er påkrevd")]
        [EmailAddress(ErrorMessage = "Ugyldig e-post adresse")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Passord er påkrevd")]
        [MinLength(8, ErrorMessage = "Passord må være minst 8 tegn")]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Bekreft passord er påkrevd")]
        [Compare("Password", ErrorMessage = "Passordene må være like")]
        [DataType(DataType.Password)]
        [Display(Name = "Bekreft passord")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "Organisasjon er påkrevd")]
        [Display(Name = "Organisasjon")]
        public int OrgId { get; set; } = 1; // Default til Kartverket
    }
}