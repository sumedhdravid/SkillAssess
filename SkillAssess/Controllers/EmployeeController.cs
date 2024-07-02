using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SkillAssess.Data;
using SkillAssess.Models;
using System.Text.Json;
using System;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Web.WebPages.Html;
using System.Linq;
using NodaTime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using System.Text;



namespace SkillAssess.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly SkillAssessDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;
        public EmployeeController(SkillAssessDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _uploadsFolder = Path.Combine(environment.WebRootPath, "Uploads");
        }
        public IActionResult Index()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                ViewBag.WelcomeMessage = $"Welcome, {userEmail}!";
            }

            return View();
        }


        public IActionResult LMSRegistration()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var lmsFormSubmitted = CheckLMSRegistrationSubmission(userEmail);
            ViewBag.LMSFormSubmitted = lmsFormSubmitted;

            if (lmsFormSubmitted)
            {
                TempData["AlertMessage"] = "You have already filled out the LMS Registration form.";
                return RedirectToAction("Index");
            }

            ViewBag.Managers = _context.AuthUsers.Where(u => u.Role == "Manager").ToList();
           
            return View();
        }

        


        [HttpPost]
        public IActionResult LMSRegistration(Employee model)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var lmsFormSubmitted = CheckLMSRegistrationSubmission(userEmail);
            ViewBag.LMSFormSubmitted = lmsFormSubmitted;

            if (lmsFormSubmitted)
            {
                TempData["AlertMessage"] = "You have already filled out the LMS Registration form.";
                return RedirectToAction("Index");
            }



            if (ModelState.IsValid)
            {

                if (model.ProfileFile != null && model.ProfileFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileFile.FileName;
                    string filePath = Path.Combine(_uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfileFile.CopyTo(stream);
                    }
                    model.Photo = uniqueFileName; // Save the unique file name to the database
                }


                // Retrieve the reporting manager's email from the LMSRegistration form
                var reportingManagerEmail = model.ReportingManagerEmail;

                //if (!reportingManagerEmail.EndsWith("@prorigosoftware"))
                //{
                //    ModelState.AddModelError("ReportingManagerEmail", "Only '@prorigosoftware' email addresses are accepted.");
                //    ViewBag.Managers = _context.AuthUsers.Where(u => u.Role == "Manager").ToList();
                //    return View(model);
                //}
                // Check if the reporting manager's email is provided
                if (!string.IsNullOrEmpty(reportingManagerEmail))
                {
                    // Create an email message with the employee details
                    var message = new MailMessage();
                    message.To.Add(new MailAddress(reportingManagerEmail));
                    message.Subject = "New Employee Registration";
                    message.Body = $"Dear Manager,\n\nA new employee has registered with the following details:\n\n" +
                                   $"Employee ID: {model.EmployeeId}\n" +
                                   $"First Name: {model.FirstName}\n" +
                                   $"Last Name: {model.LastName}\n" +
                                   $"Job Role: {model.JobRole}\n" +
                                   // Add more employee details as needed
                                   "\n\nRegards,\nSkillAssess Team";

                    // Send the email using an email service or library
                    // For example, using SmtpClient to send the email:
                    // var smtpClient = new SmtpClient();
                    // smtpClient.Send(message);
                }


                _context.Employees.Add(model);
                _context.SaveChanges();

                    TempData["AlertMessage"] = "LMS Registration form submitted successfully";



                    return RedirectToAction("Index", "Employee");
                }

            foreach (var error in ModelState)
            {
                Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            ViewBag.Managers = _context.AuthUsers.Where(u => u.Role == "Manager").ToList(); // Ensure this is set again if the model is invalid
            return View(model);
            }



        public IActionResult SkillAssessment()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var loggedInEmployee = _context.Employees.FirstOrDefault(e => e.Email == userEmail);

            if (loggedInEmployee != null)
            {
                var lmsFormSubmitted = CheckLMSRegistrationSubmission(userEmail);
                ViewBag.LMSFormSubmitted = lmsFormSubmitted;

                var skillAssessmentSubmitted = CheckSkillAssessmentSubmission(loggedInEmployee.EmployeeId);
                ViewBag.SkillAssessmentSubmitted = skillAssessmentSubmitted;

                if (!lmsFormSubmitted)
                {
                    TempData["AlertMessage"] = "Please fill out the LMS Registration Form first.";
                    return RedirectToAction("Index");
                }

                if (skillAssessmentSubmitted)
                {
                    TempData["AlertMessage"] = "You have already filled out the Skill Assessment form.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["AlertMessage"] = "Please fill out the LMS Registration Form first.";
                return RedirectToAction("Index");
            }

            ViewBag.LoggedInEmployee = loggedInEmployee; // Pass loggedInEmployee to the view

            return View();
        }



        [HttpPost]
        public IActionResult SkillAssessment(SkillAssessment model, int EmpId, string[] OtherTechnical, int[] OtherTechnicalRating, string[] AnyForeignLanguage, string[] AnyForeignLanguageRating)
        {

            string userEmail = HttpContext.Session.GetString("UserEmail");
            var skillAssessmentSubmitted = CheckSkillAssessmentSubmission(EmpId);
            ViewBag.SkillAssessmentSubmitted = skillAssessmentSubmitted;

            if (skillAssessmentSubmitted)
            {
                TempData["AlertMessage"] = "You have already filled out the Skill Assessment form.";
                return RedirectToAction("Index");
            }

            var lmsFormSubmitted = CheckLMSRegistrationSubmission(userEmail);
            ViewBag.LMSFormSubmitted = lmsFormSubmitted;

            if (!lmsFormSubmitted)
            {
                TempData["AlertMessage"] = "Please fill out the LMS Registration Form first.";
                return RedirectToAction("Index");
            }

            // Check if OtherTechnical is empty, if so, assign "NA"
            if (string.IsNullOrWhiteSpace(model.OtherTechnical))
            {
                model.OtherTechnical = "NA";
            }

            // Check if AnyForeignLanguage is empty, if so, assign "NA"
            if (string.IsNullOrWhiteSpace(model.AnyForeignLanguage))
            {
                model.AnyForeignLanguage = "NA";
            }


            if (ModelState.IsValid)
            {

                //// Concatenate OtherTechnical skills and ratings
                //StringBuilder otherTechnicalBuilder = new StringBuilder();
                //for (int i = 0; i < OtherTechnical.Length; i++)
                //{
                //    if (i > 0) otherTechnicalBuilder.Append(", ");
                //    otherTechnicalBuilder.Append($"{OtherTechnical[i]}({OtherTechnicalRating[i]})");
                //}
                //model.OtherTechnical = otherTechnicalBuilder.ToString();

                //// Concatenate AnyForeignLanguage skills and proficiency levels
                //StringBuilder foreignLanguageBuilder = new StringBuilder();
                //for (int i = 0; i < AnyForeignLanguage.Length; i++)
                //{
                //    if (i > 0) foreignLanguageBuilder.Append(", ");
                //    foreignLanguageBuilder.Append($"{AnyForeignLanguage[i]}({AnyForeignLanguageRating[i]})");
                //}
                //model.AnyForeignLanguage = foreignLanguageBuilder.ToString();

                if (OtherTechnical != null && OtherTechnical.Length > 0)
                {
                    StringBuilder otherTechnicalBuilder = new StringBuilder();
                    for (int i = 0; i < OtherTechnical.Length; i++)
                    {
                        if (i > 0) otherTechnicalBuilder.Append(", ");
                        otherTechnicalBuilder.Append($"{OtherTechnical[i]}({OtherTechnicalRating[i]})");
                    }
                    model.OtherTechnical = otherTechnicalBuilder.ToString();
                }
                else
                {
                    model.OtherTechnical = "NA";
                }

                // Concatenate AnyForeignLanguage skills and proficiency levels if they exist
                if (AnyForeignLanguage != null && AnyForeignLanguage.Length > 0)
                {
                    StringBuilder foreignLanguageBuilder = new StringBuilder();
                    for (int i = 0; i < AnyForeignLanguage.Length; i++)
                    {
                        if (i > 0) foreignLanguageBuilder.Append(", ");
                        foreignLanguageBuilder.Append($"{AnyForeignLanguage[i]}({AnyForeignLanguageRating[i]})");
                    }
                    model.AnyForeignLanguage = foreignLanguageBuilder.ToString();
                }
                else
                {
                    model.AnyForeignLanguage = "NA";
                }

                model.EmpID = EmpId;

                _context.SkillAssessment.Add(model);
                _context.SaveChanges();

                TempData["AlertMessage"] = "Skill assessment form submitted successfully";

                return RedirectToAction("Index");
            }

            
            return View(model);
        }

        

        public IActionResult ViewLMSRegistration()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var employee = _context.Employees.FirstOrDefault(e => e.Email == userEmail);

            if (employee == null)
            {
                TempData["AlertMessage"] = "LMS Registration form not found.";
                return RedirectToAction("Index");
            }

            return View(employee);
        }

        // GET: Employee/EditLMSRegistration/5
        public IActionResult EditLMSRegistration(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
            {
                TempData["AlertMessage"] = "LMS Registration form not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Managers = _context.AuthUsers.Where(u => u.Role == "Manager").ToList();
            return View(employee);
        }

        // POST: Employee/EditLMSRegistration/5
        [HttpPost]
        public IActionResult EditLMSRegistration(Employee model)
        {
            if (ModelState.IsValid)
            {
                var employee = _context.Employees.Find(model.EmployeeId);
                if (employee != null)
                {
                    // Update editable fields only
                    employee.JobRole = model.JobRole;
                    employee.ContactNumber = model.ContactNumber;
                    employee.Address = model.Address;
                    employee.DateOfJoining = model.DateOfJoining;
                    employee.TotalExperience = model.TotalExperience;
                    employee.BachelorDegree = model.BachelorDegree;
                    employee.BachelorSpecialization = model.BachelorSpecialization;
                    employee.MastersDegree = model.MastersDegree;
                    employee.MastersSpecialization = model.MastersSpecialization;
                    employee.Certifications = model.Certifications;

                    // Handle file upload if any
                    if (model.ProfileFile != null && model.ProfileFile.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileFile.FileName;
                        string filePath = Path.Combine(_uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            model.ProfileFile.CopyTo(stream);
                        }
                        employee.Photo = uniqueFileName; // Update the photo in the database
                    }

                    _context.SaveChanges();

                    TempData["AlertMessage"] = "LMS Registration form updated successfully.";
                    return RedirectToAction("ViewLMSRegistration", new { id = employee.EmployeeId });
                }
            }

            ViewBag.Managers = _context.AuthUsers.Where(u => u.Role == "Manager").ToList();
            return View(model);
        }


        public IActionResult ViewSkillAssessment()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var employee = _context.Employees.FirstOrDefault(e => e.Email == userEmail);

            if (employee == null)
            {
                TempData["AlertMessage"] = "Please fill out the LMS Registration Form first.";
                return RedirectToAction("Index");
            }

            var skillAssessment = _context.SkillAssessment.FirstOrDefault(sa => sa.EmpID == employee.EmployeeId);

            if (skillAssessment == null)
            {
                TempData["AlertMessage"] = "Skill Assessment form not found.";
                return RedirectToAction("Index");
            }

            return View(skillAssessment);
        }

        private bool CheckLMSRegistrationSubmission(string userEmail)
        {
            // Check if the user's LMS Registration form exists in the database
            return _context.Employees.Any(e => e.Email == userEmail);
        }

        private bool CheckSkillAssessmentSubmission(int empId)
        {
            // Check if the user's Skill Assessment form exists in the database
            return _context.SkillAssessment.Any(s => s.EmpID == empId);
        }




        public IActionResult Reports()
        {

            string userEmail = HttpContext.Session.GetString("UserEmail");


            if (!string.IsNullOrEmpty(userEmail))
            {

                var loggedInEmployee = _context.Employees.FirstOrDefault(e => e.Email == userEmail);


                if (loggedInEmployee != null)
                {

                    var reports = _context.SkillAssessment.Where(r => r.EmpID == loggedInEmployee.EmployeeId).ToList();


                    if (reports.Any())
                    {

                        return View(reports);
                    }
                }
            }


            ViewBag.Message = "No reports found for the logged-in employee.";
            return View();
        }



        public IActionResult Logout()
        {
            return RedirectToAction("Login","Auth");
            
        }
    }
}
