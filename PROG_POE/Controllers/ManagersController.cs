using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PROG_POE.Controllers
{
    public class ManagersController : Controller
    {
        private readonly ILogger<ManagersController> _logger;
        private readonly FinalPoeContext _finalPoeContext;

        public ManagersController(ILogger<ManagersController> logger, FinalPoeContext context)
        {
            _logger = logger;
            _finalPoeContext = context;
        }

        public IActionResult ManagerDashboard(string status)
        {
            // Retrieve claims and include the supporting documents
            var claims = _finalPoeContext.Claims
                                         .Include(c => c.Lecturer)
                                         .Include(c => c.SupportingDocuments)
                                         .AsQueryable();

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                claims = claims.Where(c => c.ClaimStatus == status);
            }

            return View(claims.ToList());
        }

        public IActionResult UpdateStatus(int claimId, string claimStatus)
        {
            // Retrieve the existing claim from the database
            var claim = _finalPoeContext.Claims.FirstOrDefault(c => c.ClaimId == claimId);

            // Get the UserId from the cookie and try to parse it as an integer
            var userIdString = Request.Cookies["UserId"];
            int? userId = null;

            if (!string.IsNullOrEmpty(userIdString))
            {
                int parsedUserId;
                if (int.TryParse(userIdString, out parsedUserId))
                {
                    userId = parsedUserId;
                }
                else
                {
                    // Handle the error case if UserId cannot be parsed to an integer
                    TempData["Message"] = "Invalid user ID.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("ManagerDashboard");
                }
            }

            if (claim == null)
            {
                TempData["Message"] = "Claim not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ManagerDashboard");
            }

            // Update the claim status
            claim.ClaimStatus = claimStatus;

            // Optionally update ApprovedAt and ApprovedBy if the claim is approved
            if (claimStatus == "Approved" && userId.HasValue)
            {
                claim.ApprovedAt = DateTime.Now;
                claim.ApprovedBy = userId.Value;
            }

            // Save the changes to the database
            _finalPoeContext.SaveChanges();

            TempData["Message"] = "Claim status updated successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("ManagerDashboard");
        }


        public IActionResult DownloadDocument(int id)
        {
            // Find the document by its ID
            var document = _finalPoeContext.SupportingDocuments.FirstOrDefault(d => d.DocumentId == id);

            if (document == null || !System.IO.File.Exists(document.FilePath))
            {
                TempData["Message"] = "Document not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ManagerDashboard");
            }

            // Return the file for download
            var fileBytes = System.IO.File.ReadAllBytes(document.FilePath);
            return File(fileBytes, "application/octet-stream", document.FileName);
        }
    }
}
