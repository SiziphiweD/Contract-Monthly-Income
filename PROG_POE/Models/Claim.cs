using System;
using System.Collections.Generic;

namespace PROG_POE.Models;

public partial class Claim
{
    public int ClaimId { get; set; }

    public int LecturerId { get; set; }

    public decimal HoursWorked { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal? TotalPayment { get; set; }

    public string? AdditionalNotes { get; set; }

    public string? ClaimStatus { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User Lecturer { get; set; } = null!;

    public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();
}
