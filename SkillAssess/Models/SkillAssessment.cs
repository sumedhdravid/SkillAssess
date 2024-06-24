using System.ComponentModel.DataAnnotations;

namespace SkillAssess.Models
{
    public class SkillAssessment
    {
        [Key]
        public int AssessmentID { get; set; }

        public int EmpID { get; set; }

        //Technical Skills
        public int DatabaseRating { get; set; }
        public int ProgrammingRating { get; set; }
        public int JavaRating { get; set; }
        public int CSRating {  get; set; }
        public int PythonRating { get; set; }
        public int WebProgrammingRating { get; set; }
        public string? OtherTechnical { get; set; }

        //Soft Skills
        public string VerbalCommunication { get; set; }
        public string WrittenCommunication { get; set; }
        public string Teamwork {  get; set; }
        public string ProblemSolving { get; set; }
        public string DecisionMaking { get; set; }
        public string Leadership { get; set; }
        public string? AnyForeignLanguage { get; set; }



        // Default status is "Pending"
        public string Status { get; set; } = "Pending";
    }
}
