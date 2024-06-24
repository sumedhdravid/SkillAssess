using Microsoft.AspNetCore.Mvc;
using SkillAssess.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using SkillAssess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Web.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using PdfSharpCore.Pdf;
using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xceed.Words.NET;






namespace SkillAssess.Controllers
{
    public class ManagerController : Controller
    {
        private readonly SkillAssessDbContext _context;
        private readonly ILogger<ManagerController> _logger;
        private readonly IConverter _converter;
        private readonly IViewRenderService _viewRenderService;
        
       


        public ManagerController(SkillAssessDbContext context, ILogger<ManagerController> logger, IConverter converter, IViewRenderService viewRenderService)
        {
            _context = context;
            _logger = logger;
            _converter = converter;
            _viewRenderService = viewRenderService;
           
            
        }
        public IActionResult Index()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                ViewBag.WelcomeMessage = $"Welcome, {userEmail}!";
            }

            // Fetch the employee count for the manager
            int employeeCount = 0;
            if (!string.IsNullOrEmpty(userEmail))
            {
                employeeCount = _context.Employees.Count(e => e.ReportingManagerEmail == userEmail);
            }
            ViewBag.EmployeeCount = employeeCount;

            // Check for new entries
            List<Notification> notifications = GetNotificationMessages(userEmail);
            _logger.LogInformation("Number of notifications retrieved: {Count}", notifications.Count);
            ViewBag.Notifications = notifications;
            ViewBag.NotificationCount = notifications.Count;

            return View();
        }

        public IActionResult Notifications()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var notifications = GetNotificationMessages(userEmail);
            _logger.LogInformation("Number of notifications for Notifications page: {Count}", notifications.Count);
            return View(notifications);
        }

        private List<Notification> GetNotificationMessages(string managerEmail)
        {
            var notifications = new List<Notification>();

            int latestEmployeeId = HttpContext.Session.GetInt32(managerEmail + "_LatestEmployeeId") ?? 0;
            int latestLmsId = HttpContext.Session.GetInt32(managerEmail + "_LatestLmsId") ?? 0;
            int latestSkillAssessmentId = HttpContext.Session.GetInt32(managerEmail + "_LatestSkillAssessmentId") ?? 0;

            _logger.LogInformation("Latest Employee ID from session: {LatestEmployeeId}", latestEmployeeId);
            _logger.LogInformation("Latest Skill Assessment ID from session: {LatestSkillAssessmentId}", latestSkillAssessmentId);

            var newEmployees = _context.Employees
                .Where(e => e.ReportingManagerEmail == managerEmail)
                .ToList();
            _logger.LogInformation("Number of new employees found: {Count}", newEmployees.Count);

            var newSkillAssessments = _context.SkillAssessment
                .Where(sa => _context.Employees.Any(e => e.EmployeeId == sa.EmpID && e.ReportingManagerEmail == managerEmail) && sa.AssessmentID > latestSkillAssessmentId)
                .ToList();
            _logger.LogInformation("Number of new skill assessments found: {Count}", newSkillAssessments.Count);

            var newLmsApplications = _context.Employees
        .Where(e => e.ReportingManagerEmail == managerEmail)
        .ToList();
            _logger.LogInformation("Number of new LMS Registration found: {Count}", newEmployees.Count);

            foreach (var employee in newEmployees)
            {
                notifications.Add(new Notification
                {
                    Message = $"New Employee registered: {employee.FirstName} {employee.LastName}",
                    DateCreated = DateTime.Now
                });
                _logger.LogInformation("Added notification for new employee: {EmployeeName}", employee.FirstName);
            }

            foreach (var assessment in newSkillAssessments)
            {
                var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == assessment.EmpID);
                if (employee != null)
                {
                    var employeeName = $"{employee.FirstName} {employee.LastName}";
                    _logger.LogInformation("Adding notification for new skill assessment by: {EmployeeName}", employeeName);
                    notifications.Add(new Notification
                    {
                        Message = $"Skill assessment form submitted by {employeeName} (Employee ID: {assessment.EmpID})",
                        DateCreated = DateTime.Now,
                        IsNew = true
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to find employee for skill assessment with Employee ID: {EmployeeId}", assessment.EmpID);
                }
            }



            foreach (var lms in newLmsApplications)
            {

                notifications.Add(new Notification
                {
                    Message = $"LMS form submitted by {lms.FirstName} {lms.LastName} (Employee ID: {lms.EmployeeId})",
                    DateCreated = DateTime.Now
                });

            }

            // Update the latest IDs in the session
            if (newEmployees.Any())
            {
                HttpContext.Session.SetInt32(managerEmail + "_LatestEmployeeId", newEmployees.Max(e => e.EmployeeId));
            }

            if (newSkillAssessments.Any())
            {
                HttpContext.Session.SetInt32(managerEmail + "_LatestSkillAssessmentId", newSkillAssessments.Max(sa => sa.AssessmentID));
            }

            if (newLmsApplications.Any())
            {
                HttpContext.Session.SetInt32(managerEmail + "_LatestLmsEmployeeId", newLmsApplications.Max(e => e.EmployeeId));
            }

            _logger.LogInformation("Number of notifications to return: {Count}", notifications.Count);
            return notifications.OrderByDescending(n => n.DateCreated).ToList();
        }




       


        public IActionResult EmployeeDetails(string employeeId, string name)
        {
            
            var managerEmail = HttpContext.Session.GetString("UserEmail");

            
            if (!string.IsNullOrEmpty(managerEmail))
            {
                // Filter the list of employees to only include those associated with the manager's email
                var employees = _context.Employees.Where(e => e.ReportingManagerEmail == managerEmail).AsQueryable();

                if (!string.IsNullOrEmpty(employeeId) && int.TryParse(employeeId, out int empId))
                {
                    employees = employees.Where(e => e.EmployeeId == empId);
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    employees = employees.Where(e => e.FirstName.Contains(name) || e.LastName.Contains(name));
                }

                return View(employees.ToList());
            }

            // Redirect to login page or handle unauthorized access
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult SkillAssessment()
        {
            
            string managerEmail = HttpContext.Session.GetString("UserEmail");

            
            if (!string.IsNullOrEmpty(managerEmail))
            {
                // Retrieve employee IDs for employees of the reporting manager
                var employeeIds = _context.Employees
                    .Where(e => e.ReportingManagerEmail == managerEmail)
                    .Select(e => e.EmployeeId)
                    .ToList();

                // Retrieve skill assessments for employees of the reporting manager
                var skillAssessments = _context.SkillAssessment
                    .Where(sa => employeeIds.Contains(sa.EmpID))
                    .ToList();

                return View(skillAssessments);
                
            }

            
            return RedirectToAction("Login", "Auth");


        }


        [HttpPost]
        public IActionResult ApproveSkillAssessment(int assessmentId)
        {
            var assessment = _context.SkillAssessment.FirstOrDefault(sa => sa.AssessmentID == assessmentId);
            if (assessment != null)
            {
                assessment.Status = "Approved";
                _context.SaveChanges();

                HttpContext.Session.SetString("AlertMessage", "Your skill assessment form has been approved.");
            }
            return RedirectToAction("SkillAssessment", "Manager");
        }

        [HttpPost]
        public IActionResult RejectSkillAssessment(int assessmentId)
        {
            var assessment = _context.SkillAssessment.FirstOrDefault(sa => sa.AssessmentID == assessmentId);
            if (assessment != null)
            {
                assessment.Status = "Rejected";
                _context.SaveChanges();

                HttpContext.Session.SetString("AlertMessage", "Your skill assessment form has been rejected.");
            }
            return RedirectToAction("SkillAssessment", "Manager");
        }

        public IActionResult Reports(int? employeeId)
        {

            string managerEmail = HttpContext.Session.GetString("UserEmail");


            if (!string.IsNullOrEmpty(managerEmail))
            {

                var employeeIds = _context.Employees
                    .Where(e => e.ReportingManagerEmail == managerEmail)
                    .Select(e => e.EmployeeId)
                    .ToList();

                // Query the assessments of these employees
                var assessments = _context.SkillAssessment
                    .Where(a => employeeIds.Contains(a.EmpID));

                // Apply the search filter if an employeeId is provided
                if (employeeId.HasValue)
                {
                    assessments = assessments.Where(a => a.EmpID == employeeId.Value);
                }

                return View(assessments.ToList());




            }

            return RedirectToAction("Login", "Auth");

        }
        


        public IActionResult ViewReport(int EmpID)
        {
            

            var assessment = _context.SkillAssessment.FirstOrDefault(a => a.EmpID == EmpID);
            if (assessment == null)
            {
                return NotFound("Assessment not found");
            }

            // Generate the report content
            var reportContent = GenerateReport(assessment);

            // Pass the report content to the view
            ViewBag.ReportContent = reportContent;

            return View(assessment);
        }




        public async Task<IActionResult> DownloadReport(int assessmentId)
        {
            // Retrieve assessment data from the database
            var assessment = _context.SkillAssessment.FirstOrDefault(a => a.AssessmentID == assessmentId);
            if (assessment == null)
            {
                return NotFound("Assessment not found");
            }

            
            string reportContent = GenerateReport(assessment);

            
            using (var document = DocX.Create("SkillAssessmentReport.docx"))
            {
                
                string[] lines = reportContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                
                foreach (var line in lines)
                {
                    document.InsertParagraph(line).FontSize(12).Font("Arial");
                }

                
                MemoryStream memoryStream = new MemoryStream();
                document.SaveAs(memoryStream);
                memoryStream.Position = 0;

                
                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "SkillAssessmentReport.docx");
            }
        }


        //public IActionResult DownloadReport(int assessmentId)
        //{


        //    var assessment = _context.SkillAssessment.FirstOrDefault(a => a.AssessmentID == assessmentId);
        //    if (assessment == null)
        //    {
        //        return NotFound("Assessment not found");
        //    }



        //    // Generate the report content
        //    var reportContent = GenerateReport(assessment);
        //    return Content(reportContent, "application/pdf");
        //    // Check if report content is valid
        //    //if (string.IsNullOrEmpty(reportContent))
        //    //{
        //    //    return BadRequest("Failed to generate report content");
        //    //}

        //    //try
        //    //{
        //    //    var reportData = Encoding.UTF8.GetBytes(reportContent);
        //    //    return File(reportData, "application/pdf", "SkillAssessmentReport.pdf");
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    // Log the exception or handle it appropriately
        //    //    return StatusCode(500, $"Failed to download report: {ex.Message}");
        //    //}

        //    //var reportData = Encoding.UTF8.GetBytes(reportContent);

        //    //// Return the report as a file download
        //    //return File(reportData, "application/pdf", "SkillAssessmentReport.pdf");
        //}


        //private string GenerateReport(SkillAssessment assessment)
        //{
        //    var sb = new StringBuilder();

        //    sb.AppendLine("<html>");
        //    sb.AppendLine("<head>");
        //    sb.AppendLine("<style>");
        //    sb.AppendLine("body { font-family: Arial, sans-serif; }");
        //    sb.AppendLine("h1 { text-align: center; }");
        //    sb.AppendLine(".container { margin: 0 auto; padding: 20px; max-width: 800px; background-color: #f4f4f4; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }");
        //    sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
        //    sb.AppendLine("table, th, td { border: 1px solid black; }");
        //    sb.AppendLine("th, td { padding: 8px; text-align: left; }");
        //    sb.AppendLine("th { background-color: #f2f2f2; }");
        //    sb.AppendLine("</style>");
        //    sb.AppendLine("</head>");
        //    sb.AppendLine("<body>");
        //    sb.AppendLine("<div class='container'>");
        //    sb.AppendLine("<h1>Skill Assessment Report</h1>");
        //    sb.AppendLine("<table>");
        //    sb.AppendLine("<tr><th>Field</th><th>Value</th></tr>");
        //    sb.AppendLine($"<tr><td>Employee ID</td><td>{assessment.EmpID}</td></tr>");
        //    sb.AppendLine($"<tr><td>Status</td><td>{assessment.Status}</td></tr>");
        //    sb.AppendLine($"<tr><td>Database Rating</td><td>{assessment.DatabaseRating}</td></tr>");
        //    sb.AppendLine($"<tr><td>Programming Rating</td><td>{assessment.ProgrammingRating}</td></tr>");
        //    sb.AppendLine($"<tr><td>Java Rating</td><td>{assessment.JavaRating}</td></tr>");
        //    sb.AppendLine($"<tr><td>C# Rating</td><td>{assessment.CSRating}</td></tr>");
        //    sb.AppendLine($"<tr><td>Python Rating</td><td>{assessment.PythonRating}</td></tr>");
        //    sb.AppendLine($"<tr><td>Web Programming Rating</td><td>{assessment.WebProgrammingRating}</td></tr>");
        //    sb.AppendLine($"<tr><td>Other Technical</td><td>{assessment.OtherTechnical}</td></tr>");
        //    sb.AppendLine($"<tr><td>Verbal Communication</td><td>{assessment.VerbalCommunication}</td></tr>");
        //    sb.AppendLine($"<tr><td>Written Communication</td><td>{assessment.WrittenCommunication}</td></tr>");
        //    sb.AppendLine($"<tr><td>Teamwork</td><td>{assessment.Teamwork}</td></tr>");
        //    sb.AppendLine($"<tr><td>Problem Solving</td><td>{assessment.ProblemSolving}</td></tr>");
        //    sb.AppendLine($"<tr><td>Decision Making</td><td>{assessment.DecisionMaking}</td></tr>");
        //    sb.AppendLine($"<tr><td>Leadership</td><td>{assessment.Leadership}</td></tr>");
        //    sb.AppendLine($"<tr><td>Any Foreign Language</td><td>{assessment.AnyForeignLanguage}</td></tr>");
        //    sb.AppendLine("</table>");
        //    sb.AppendLine("</div>");
        //    sb.AppendLine("</body>");
        //    sb.AppendLine("</html>");

        //    return sb.ToString();
        //}



        private string GenerateReport(SkillAssessment assessment)
        {


            StringBuilder sb = new();
            sb.AppendLine("Skill Assessment Report");
            sb.AppendLine($"Employee ID: {assessment.EmpID}");
            sb.AppendLine($"Status: {assessment.Status}");
            sb.AppendLine($"Database Rating: {assessment.DatabaseRating}");
            sb.AppendLine($"Programming Rating: {assessment.ProgrammingRating}");
            sb.AppendLine($"Java Rating: {assessment.JavaRating}");
            sb.AppendLine($"C# Rating: {assessment.CSRating}");
            sb.AppendLine($"Python Rating: {assessment.PythonRating}");
            sb.AppendLine($"Web Programming Rating: {assessment.WebProgrammingRating}");
            sb.AppendLine($"Other: {assessment.OtherTechnical}");
            sb.AppendLine($"Verbal Communication: {assessment.VerbalCommunication}");
            sb.AppendLine($"Written Communication: {assessment.WrittenCommunication}");
            sb.AppendLine($"Teamwork: {assessment.Teamwork}");
            sb.AppendLine($"Problem Solving: {assessment.ProblemSolving}");
            sb.AppendLine($"Decision Making: {assessment.DecisionMaking}");
            sb.AppendLine($"Leadership: {assessment.Leadership}");
            sb.AppendLine($"Foreign Language: {assessment.AnyForeignLanguage}");



            return sb.ToString();
        }





        [HttpGet]
        public IActionResult SkillAssessmentSearch(int? technicalEmployeeId, string technicalSkillName, int? technicalMinimumRating, string softSkillName, string softMinimumRating, string status)
        {
            string managerEmail = HttpContext.Session.GetString("UserEmail");

            
            var loggedInUser = _context.AuthUsers.FirstOrDefault(u => u.Email == managerEmail);
            if (loggedInUser != null)
            {
                var assessments = _context.SkillAssessment
                    .Where(a => _context.Employees
                        .Any(e => e.EmployeeId == a.EmpID && e.ReportingManagerEmail == managerEmail))
                    .AsEnumerable();

                // Filter by employee ID 
                if (technicalEmployeeId != null)
                {
                    assessments = assessments.Where(a => a.EmpID == technicalEmployeeId);
                }

                // Filter by technical skill name and minimum rating 
                if (!string.IsNullOrEmpty(technicalSkillName) && technicalMinimumRating != null)
                {
                    assessments = assessments.Where(a => GetTechnicalRatingBySkillName(a, technicalSkillName) >= technicalMinimumRating);
                }

                // Filter by soft skill name and minimum rating
                if (!string.IsNullOrEmpty(softSkillName))
                {
                    assessments = assessments.Where(a => GetSoftRatingBySkillName(a, softSkillName) == softMinimumRating);
                }



                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    assessments = assessments.Where(a => a.Status == status);
                }

                return View("SkillAssessment", assessments.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Manager");
            }
        }

        
        private int GetTechnicalRatingBySkillName(SkillAssessment assessment, string skillName)
        {
            //logic to map technical skill names to rating properties in the SkillAssessment model
            switch (skillName)
            {
                case "Database":
                    return assessment.DatabaseRating;
                case "Programming":
                    return assessment.ProgrammingRating;
                case "Java":
                    return assessment.JavaRating;
                case "C#":
                    return assessment.CSRating;
                case "Python":
                    return assessment.PythonRating;
                case "Web Programming":
                    return assessment.WebProgrammingRating;
                //case "OtherTechnical":
                //    return assessment.OtherTechnical;

                
                default:
                    return 0; 
            }
        }

        
        private string GetSoftRatingBySkillName(SkillAssessment assessment, string skillName)
        {
            // logic to map soft skill names to rating properties in the SkillAssessment model
            switch (skillName)
            {
                case "Verbal Communication":
                    return assessment.VerbalCommunication;
                case "Written Communication":
                    return assessment.WrittenCommunication;
                case "Teamwork":
                    return assessment.Teamwork;
                case "Problem Solving":
                    return assessment.ProblemSolving;
                case "Decision Making":
                    return assessment.DecisionMaking;
                case "Leadership":
                    return assessment.Leadership;
                case "Any Foreign Language":
                    return assessment.AnyForeignLanguage;

                // Add more cases for other soft skills
                default:
                    return null; 
            }
        }

        public IActionResult Logout()
        {
          
            return RedirectToAction("Login", "Auth");
            //return View();
        }
    }
}
