using System;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Common.Auth;

public class OneLoginRequest
{
    
        [Required]
        [EmailAddress]
        public string Email { get; set; }
}
