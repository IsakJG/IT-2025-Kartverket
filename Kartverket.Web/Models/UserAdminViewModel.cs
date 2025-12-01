using System.ComponentModel.DataAnnotations;

namespace Kartverket.Web.Models
{
    // Modell for å vise/redigere brukere
    public class UserAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name/Username is required")]
        public string UserName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        public int? RoleId { get; set; }
        public string? Role { get; set; } 
        
        [Display(Name = "Organization")]
        [Required(ErrorMessage = "Please select an organization")]
        public int? OrgId { get; set; }
        
        public string? OrganizationName { get; set; } 
    }

    // Modell for å opprette ny organisasjon
    public class CreateOrgViewModel 
    {
        [Required(ErrorMessage = "Organization Name is required")]
        [Display(Name = "Organization Name")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters")]
        public string OrgName { get; set; } = string.Empty;
    }

    // Modell for listen over organisasjoner (ManageOrganizations)
    public class OrganizationListViewModel
    {
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public int UserCount { get; set; }
    }
}