using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace netCoreWepApiAuthJWT.Model
{

    
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please give a password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Firstname can not be empty")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Please give the Lastname")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Location can not be empty")]
        public string Location { get; set; }
    }
}

