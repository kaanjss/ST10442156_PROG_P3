using System.ComponentModel.DataAnnotations;

namespace CMCS.Web.Models.ViewModels;

public class ClaimSubmissionViewModel
{
	[Required(ErrorMessage = "Month is required")]
	[Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
	public int Month { get; set; }

	[Required(ErrorMessage = "Year is required")]
	[Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100")]
	public int Year { get; set; }

	[Required(ErrorMessage = "Hourly rate is required")]
	[Range(0.01, 10000, ErrorMessage = "Hourly rate must be between 0.01 and 10000")]
	[Display(Name = "Hourly Rate (R)")]
	public decimal HourlyRate { get; set; }

	[Display(Name = "Additional Notes")]
	[StringLength(500, ErrorMessage = "Additional notes cannot exceed 500 characters")]
	public string AdditionalNotes { get; set; } = string.Empty;

	public List<ClaimLineItem> Items { get; set; } = new();

	public decimal TotalHours => Items?.Sum(x => x.Hours) ?? 0;
	public decimal TotalAmount => TotalHours * HourlyRate;
}

public class ClaimLineItem
{
	[Required(ErrorMessage = "Activity description is required")]
	[StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
	public string ActivityDescription { get; set; } = string.Empty;

	[Required(ErrorMessage = "Hours are required")]
	[Range(0.01, 500, ErrorMessage = "Hours must be between 0.01 and 500")]
	public decimal Hours { get; set; }
}


