using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;

namespace Kartverket.Web.Models
{
    public class LoginModel //Setter premisser for log-in
    {
        [Required(ErrorMessage = "Username is required")] //Brukernavn MÅ skrives for å gå videre
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")] // Basicly samme som nevnt ovenfor
        public string Password { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; } //Hash og salt etterhvert

        public string Role { get; set; }

    }


}