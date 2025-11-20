namespace CMCS.Web.Models;

public class Lecturer
{
	public int Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public string Department { get; set; } = string.Empty;
	public string EmployeeNumber { get; set; } = string.Empty;
	public string BankName { get; set; } = string.Empty;
	public string AccountNumber { get; set; } = string.Empty;
	public string TaxNumber { get; set; } = string.Empty;
	public decimal DefaultHourlyRate { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	// Navigation property
	public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
}

