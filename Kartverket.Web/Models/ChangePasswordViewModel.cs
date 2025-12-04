using System.ComponentModel.DataAnnotations;
using Kartverket.Web.Models;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for sikker endring av passord.
    /// </summary>
    public class ChangePasswordViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        // StringLength kombinerer Min og Max på en ryddig måte
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        // Bruker nameof() for å sikre typesikkerhet ved refaktorering
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}