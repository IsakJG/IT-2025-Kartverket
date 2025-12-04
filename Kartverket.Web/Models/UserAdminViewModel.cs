using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    /// <summary>
    /// ViewModel for managing users (Create/Edit) in the Admin panel.
    /// Combines user data with selection of Role and Organization.
    /// </summary>
    public class UserAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        // This might be redundant if UserName is used for display, 
        // but kept to prevent breaking existing views.
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password is optional when editing (null means "do not change").
        /// </summary>
        [Display(Name = "Password (leave blank to keep unchanged)")]
        [DataType(DataType.Password)] // Masks input in the browser
        public string? Password { get; set; }

        // --- Role ---

        [Display(Name = "Role")]
        [Required(ErrorMessage = "Please select a role.")]
        public int? RoleId { get; set; }
        
        // For display only (not input)
        public string? Role { get; set; } 

        // --- Organization ---
        
        [Display(Name = "Organization")]
        [Required(ErrorMessage = "Please select an organization.")]
        public int? OrgId { get; set; }
        
        // For display only (not input)
        public string? OrganizationName { get; set; } 
    }

    /// <summary>
    /// ViewModel for creating a new organization.
    /// </summary>
    public class CreateOrgViewModel 
    {
        [Required(ErrorMessage = "Organization Name is required.")]
        [Display(Name = "Organization Name")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string OrgName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simple DTO for listing organizations with user statistics.
    /// </summary>
    public class OrganizationListViewModel
    {
        public int OrgId { get; set; }
        
        [Display(Name = "Organization")]
        public string OrgName { get; set; } = string.Empty;

        [Display(Name = "User Count")]
        public int UserCount { get; set; }
    }
}