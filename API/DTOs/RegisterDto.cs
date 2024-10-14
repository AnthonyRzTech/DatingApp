using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public required string UserName { get; set; }
        
        [Required]
        public required string Password { get; set; }
    }
}