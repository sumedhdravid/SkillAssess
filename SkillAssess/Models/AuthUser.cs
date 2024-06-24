using System.ComponentModel.DataAnnotations;

namespace SkillAssess.Models
{
    public class AuthUser
    {
        [Key]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RestrictedEmailDomain(ErrorMessage = "Only @prorigosoftware.com email addresses are allowed")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, one special character, and be at least 8 characters long")]
        public string Password { get; set; }

        public string Role { get; set; }
    }

    public class RestrictedEmailDomainAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var email = value as string;
            if (email != null && email.EndsWith("@prorigosoftware.com"))
            {
                return true;
            }
            return false;
        }
    }
}
