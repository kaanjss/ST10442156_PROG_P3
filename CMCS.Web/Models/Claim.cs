namespace CMCS.Web.Models;

public class Claim
{
	public int Id { get; set; }
	public int LecturerId { get; set; }
	public int Month { get; set; }
	public int Year { get; set; }
	public decimal HourlyRate { get; set; }
	public decimal TotalHours { get; set; }
	public decimal Amount { get; set; }
	public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
	public string? AdditionalNotes { get; set; }
	
	// Navigation properties
	public virtual Lecturer? Lecturer { get; set; }
	public virtual ICollection<ClaimLine> Lines { get; set; } = new List<ClaimLine>();
	public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
	public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
}

public class ClaimLine
{
	public int Id { get; set; }
	public int ClaimId { get; set; }
	public string ActivityDescription { get; set; } = string.Empty;
	public decimal Hours { get; set; }
	
	// Navigation property
	public virtual Claim? Claim { get; set; }
}

public class Document
{
	public int Id { get; set; }
	public int ClaimId { get; set; }
	public string FileName { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
	
	// Navigation property
	public virtual Claim? Claim { get; set; }
}

public class Approval
{
	public int Id { get; set; }
	public int ClaimId { get; set; }
	public int ApproverId { get; set; }
	public string Decision { get; set; } = string.Empty; // e.g., Verified, Approved, Rejected
	public string Comment { get; set; } = string.Empty;
	public DateTime DecisionAt { get; set; } = DateTime.UtcNow;
	
	// Navigation property
	public virtual Claim? Claim { get; set; }
}


