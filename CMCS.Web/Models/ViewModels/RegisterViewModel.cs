using System.ComponentModel.DataAnnotations;

namespace CMCS.Web.Models.ViewModels;

public class RegisterViewModel
{
	[Required(ErrorMessage = "Full name is required")]
	[StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
	[Display(Name = "Full Name")]
	public string FullName { get; set; } = string.Empty;

	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Invalid email address")]
	[Display(Name = "Email Address")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Password is required")]
	[StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
	[DataType(DataType.Password)]
	[Display(Name = "Password")]
	public string Password { get; set; } = string.Empty;

	[DataType(DataType.Password)]
	[Display(Name = "Confirm password")]
	[Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
	public string ConfirmPassword { get; set; } = string.Empty;

	[Required(ErrorMessage = "Please select a role")]
	[Display(Name = "Register as")]
	public string Role { get; set; } = "Lecturer";
}

