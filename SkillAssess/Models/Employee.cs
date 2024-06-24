using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillAssess.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        
        public string JobRole { get; set; }
        public string Gender { get; set; }

        [NotMapped]
        public IFormFile ProfileFile { get; set; }
        public string? Photo { get; set; }


        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(10, ErrorMessage = "The Contact Number must contain 10 digits", MinimumLength = 10)]
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public DateOnly DateOfJoining { get; set; }
        public int TotalExperience { get; set; }
        public string BachelorDegree { get; set; }
        public string BachelorSpecialization { get; set; }
        public string MastersDegree { get; set; }
        public string MastersSpecialization { get; set; }
        public string Certifications { get; set; }

        [Required]
        public string ReportingManagerEmail { get; set; }
    }

}
