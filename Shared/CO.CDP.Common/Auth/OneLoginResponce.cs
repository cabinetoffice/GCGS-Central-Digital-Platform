using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CO.CDP.Common.Auth;

public class OneLoginResponce
{
        [Required]
        [JsonPropertyName("sub")]
        public string Sub { get; set; }
        
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [Required]
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
        
        [Required]
        [Phone]
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }
        
        [Required]
        [JsonPropertyName("phone_number_verified")]
        public bool PhoneNumberVerified { get; set; }
        
        [Required]
        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; set; }

        public OneLoginResponce(string sub, string email, bool emailVerified, string phoneNumber, bool phoneNumberVerified, long updatedAt)
        {
            Sub = sub;
            Email = email;
            EmailVerified = emailVerified;
            PhoneNumber = phoneNumber;
            PhoneNumberVerified = phoneNumberVerified;
            UpdatedAt = updatedAt;
        }
}
