using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Models;

namespace PROG_POE.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly FinalPoeContext _finalPoeContext;

        public AccountController(ILogger<AccountController> logger, FinalPoeContext context)
        {
            _logger = logger;
            _finalPoeContext = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                var userExist = _finalPoeContext.Users.FirstOrDefault(a => a.Email == user.Email);

                if (userExist == null)
                {
                    // Hash the password
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                    // Save the user to the database
                    _finalPoeContext.Users.Add(user);
                    _finalPoeContext.SaveChanges();

                    TempData["Message"] = "User registered successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Login");
                }
                else
                {
                    TempData["Message"] = "User already exists.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Register");
                }


            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            // Find the user by email
            var existingUser = _finalPoeContext.Users.FirstOrDefault(a => a.Email == user.Email);

            if (existingUser != null)
            {
                // Verify the provided password against the hashed password
                var isPasswordValid = BCrypt.Net.BCrypt.Verify(user.PasswordHash, existingUser.PasswordHash);

                if (isPasswordValid)
                {
                    // Successful login
                    var userRole = existingUser.Role; // Get the role of the logged-in user

                    // Store the UserId in a cookie for future use
                    Response.Cookies.Append("UserId", existingUser.UserId.ToString(), new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(1), // Set expiration to 1 day (you can change this)
                        HttpOnly = true // Makes the cookie only accessible via HTTP request (helps with security)
                    });

                    // Check user role and redirect based on role
                    if (userRole == "Lecturer")
                    {
                        TempData["Message"] = "Welcome, Lecturer!";
                        TempData["MessageType"] = "success";
                       return RedirectToAction("LectureDashboard", "Lecture");
                    }
                    else if (userRole == "Coordinator")
                    {
                        TempData["Message"] = "Welcome, Coordinator!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("ManagerDashboard", "Managers");
                    }
                    else if (userRole == "Manager")
                    {
                        TempData["Message"] = "Welcome, Manager!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("ManagerDashboard", "Managers");
                    }
                    else if (userRole == "HR")
                    {
                        return RedirectToAction("HRDashboard", "HR");
                    }
                    else
                    {
                        TempData["Message"] = "Invalid role assigned to user.";
                        TempData["MessageType"] = "error";
                        // Redirect back to login if role is invalid
                    }

                    return RedirectToAction("Index", "Home"); // or another page based on role
                }
                else
                {
                    // Incorrect password
                    TempData["Message"] = "Invalid credentials. Please try again.";
                    TempData["MessageType"] = "error";
                }
            }
            else
            {
                // User not found
                TempData["Message"] = "User not found. Please register.";
                TempData["MessageType"] = "error";
            }

            return View(user); // Return to the login page with the user model
        }
    }
    }
