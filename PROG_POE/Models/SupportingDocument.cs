using System;
using System.Collections.Generic;

namespace PROG_POE.Models;

public partial class SupportingDocument
{
    public int DocumentId { get; set; }

    public int ClaimId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Claim Claim { get; set; } = null!;
}
