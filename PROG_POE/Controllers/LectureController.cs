using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Models;

namespace PROG_POE.Controllers
{
    public class LectureController : Controller
    {
        private readonly ILogger<LectureController> _logger;
        private readonly FinalPoeContext _finalPoeContext;

        public LectureController(ILogger<LectureController> logger, FinalPoeContext context)
        {
            _logger = logger;
            _finalPoeContext = context;
        }
        public IActionResult LectureDashboard()
        {
            var userId = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User is not logged in. Please log in first.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login");
            }

            var lecturerId = int.Parse(userId);
            var claims = _finalPoeContext.Claims
                          .Include(c => c.SupportingDocuments) // Ensure related documents are loaded
                          .Where(c => c.LecturerId == lecturerId)
                          .ToList();

            return View(claims);
        }

        [HttpGet]
        public IActionResult NewClaim()
        {
            return View();
        }
        [HttpPost]
        public IActionResult NewClaim(Claim claim)
        {
            // Get the UserId from the cookie
            var userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "User is not logged in. Please log in first.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Login"); // Redirect to login page if not logged in
            }

            // Convert the UserId to an integer
            claim.LecturerId = int.Parse(userId);  // Set the LecturerId (or use it as the foreign key)

            // Additional logic to save the claim, for example:
            _finalPoeContext.Claims.Add(claim);
            _finalPoeContext.SaveChanges();

            TempData["Message"] = "Claim submitted successfully!";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Home"); // Redirect to some page after successful claim submission
        }

        // Edit action to display the edit form
        public IActionResult Edit(int id)
        {
            // Retrieve the claim by its Id (ClaimId)
            var claim = _finalPoeContext.Claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["Message"] = "Claim not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("LectureDashboard"); // Redirect to the dashboard if not found
            }

            return View(claim); // Pass the claim to the edit view
        }

        // Edit POST action to handle the updated claim
        [HttpPost]
        public IActionResult Edit(Claim claim)
        {
            // Ensure model state is valid
            if (ModelState.IsValid)
            {
                // Retrieve the existing claim from the database
                var existingClaim = _finalPoeContext.Claims.FirstOrDefault(c => c.ClaimId == claim.ClaimId);

                if (existingClaim == null)
                {
                    TempData["Message"] = "Claim not found.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("LectureDashboard");
                }

                // Debugging: Check if the values are being correctly passed
                Console.WriteLine($"Claim ID: {claim.ClaimId}, HoursWorked: {claim.HoursWorked}, HourlyRate: {claim.HourlyRate}");

                // Update the claim's properties
                existingClaim.HoursWorked = claim.HoursWorked;
                existingClaim.HourlyRate = claim.HourlyRate;
                existingClaim.TotalPayment = claim.TotalPayment;
                existingClaim.AdditionalNotes = claim.AdditionalNotes;
                existingClaim.ClaimStatus = claim.ClaimStatus;
                existingClaim.SubmittedAt = claim.SubmittedAt;
                existingClaim.ApprovedAt = claim.ApprovedAt;

                // Save the changes to the database
                try
                {
                    _finalPoeContext.SaveChanges();
                    TempData["Message"] = "Claim updated successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("LectureDashboard"); // Redirect to the dashboard after saving
                }
                catch (Exception ex)
                {
                    // Log the exception and show error message
                    Console.WriteLine($"Error while saving: {ex.Message}");
                    TempData["Message"] = "Error while updating the claim. Please try again.";
                    TempData["MessageType"] = "error";
                }
            }
            else
            {
                // Log model state errors to the console
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                // Return the view if model state is invalid
                TempData["Message"] = "Please correct the form errors and try again.";
                TempData["MessageType"] = "error";
            }

            return View(claim); // Return to the view if model state is not valid
        }


        // Delete action to show a confirmation prompt
        public IActionResult Delete(int id)
        {
            var claim = _finalPoeContext.Claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["Message"] = "Claim not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("LectureDashboard"); // Redirect if claim not found
            }

            return View(claim); // Show the delete confirmation view
        }

        // Delete POST action to remove the claim
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var claim = _finalPoeContext.Claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["Message"] = "Claim not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("LectureDashboard"); // Redirect if claim not found
            }

            _finalPoeContext.Claims.Remove(claim); // Remove the claim
            _finalPoeContext.SaveChanges(); // Save the changes to the database

            TempData["Message"] = "Claim deleted successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("LectureDashboard"); // Redirect to the dashboard after deletion
        }

        public async Task<IActionResult> UploadDocument(int claimId, IFormFile document)
        {
            if (document != null && document.Length > 0)
            {
                // Define the directory where you want to store the uploaded documents
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");

                // Ensure the directory exists
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Generate a unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(document.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }

                // Insert the file information into the SupportingDocuments table
                var supportingDocument = new SupportingDocument
                {
                    ClaimId = claimId, // Make sure this matches the claim ID
                    FileName = document.FileName,
                    FilePath = filePath,
                    UploadedAt = DateTime.Now
                };

                // Add and save the document information
                _finalPoeContext.SupportingDocuments.Add(supportingDocument);
                await _finalPoeContext.SaveChangesAsync();

                TempData["Message"] = "Document uploaded successfully!";
                TempData["MessageType"] = "success";

                // Optionally, delete the temporary uploaded file after it has been stored in the database
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

            }
            else
            {
                TempData["Message"] = "No document selected.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("LectureDashboard");  // Redirect back to the dashboard after upload
        }
        public IActionResult DownloadDocument(int id)
        {
            var document = _finalPoeContext.SupportingDocuments.FirstOrDefault(d => d.DocumentId == id);

            if (document == null || !System.IO.File.Exists(document.FilePath))
            {
                TempData["Message"] = "Document not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("LectureDashboard");
            }

            var fileBytes = System.IO.File.ReadAllBytes(document.FilePath);
            return File(fileBytes, "application/octet-stream", document.FileName);
        }





    }
}
