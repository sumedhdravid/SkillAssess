using Microsoft.AspNetCore.Mvc;
using SkillAssess.Models;
using SkillAssess.Data;
using Microsoft.AspNetCore.Http;


namespace SkillAssess.Controllers
{
    public class AuthController : Controller
    {
        private readonly SkillAssessDbContext _context;

        public AuthController(SkillAssessDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AuthUser model)
        {
            if (ModelState.IsValid)
            {
                // Check if user email and password exists in AuthUsers table
                var user = _context.AuthUsers.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserEmail", model.Email);

                    // Redirect to respective dashboard based on user role
                    if (user.Role == "Employee")
                    {
                        return RedirectToAction("Index", "Employee");
                    }
                    else if (user.Role == "Manager")
                    {
                        return RedirectToAction("Index", "Manager");
                    }
                }
                else
                {

                    TempData["AlertMessage"] = "Invalid email or password";
                }
            }


            ViewBag.Error = "Invalid Email or Password";
            return View(model);
        }

        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(AuthUser model)
        {
            var user = _context.AuthUsers.FirstOrDefault(u => u.Email == model.Email);

            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    TempData["AlertMessage"] = "User Already Exists !!!";
                }
                else
                {
                    _context.AuthUsers.Add(model);
                    _context.SaveChanges();


                    return RedirectToAction("Login");
                }
            }

            
            return View(model);
        }
    }
}
