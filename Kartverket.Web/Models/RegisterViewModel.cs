using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for registrering av nye brukere.
    /// Inneholder valideringsregler som sikrer at dataene er korrekte før de sendes til kontrolleren.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        // StringLength is more concise than MinLength/MaxLength
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must confirm the password.")]
        // nameof(Password) ensures the code tolerates refactoring (variable name changes)
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must choose an organization.")]
        [Display(Name = "Organisasjo")]
        // Nullable int ensures the user actively chooses from the list (prevents default 0)
        public int? OrgId { get; set; } 
    }
}