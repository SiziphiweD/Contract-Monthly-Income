using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PROG_POE.Data;
using PROG_POE.Models;

namespace PROG_POE.Controllers
{
    public class HRController : Controller
    {
        private readonly ILogger<HRController> _logger;
        private readonly FinalPoeContext _finalPoeContext;

        public HRController(ILogger<HRController> logger, FinalPoeContext context)
        {
            _logger = logger;
            _finalPoeContext = context;
        }

        public IActionResult HRDashboard()
        {
            return View();
        }

        public IActionResult VewClaims()
        {
            var claims = _finalPoeContext.Claims
                             .Include(c => c.Lecturer)
                             .Include(c => c.SupportingDocuments)
                             .AsQueryable();

            return View(claims.ToList());
        }
        public IActionResult VewLectures()
        {
            var lecturers = _finalPoeContext.Users
                              .Where(u => u.Role == "Lecturer");

            return View(lecturers.ToList());
        }

        public IActionResult ViewInvoice(int lecturerId)
        {
            // Include the Lecturer property to ensure it's loaded with Claims
            var claims = _finalPoeContext.Claims
                            .Include(c => c.Lecturer)  // Include Lecturer
                            .Where(c => c.Lecturer.UserId == lecturerId && c.ClaimStatus == "Approved")  // Filter based on Lecturer and Approved claims
                            .ToList();

            return View(claims);
        }



    }
}
