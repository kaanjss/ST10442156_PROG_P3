using Microsoft.AspNetCore.Identity;

namespace CMCS.Web.Models;

/// <summary>
/// Custom application user extending IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; } = string.Empty;
	
	// Link to Lecturer if user is a lecturer
	public int? LecturerId { get; set; }
	public virtual Lecturer? Lecturer { get; set; }
	
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

