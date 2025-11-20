using CMCS.Web.Models;
using CMCS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CMCS.Web.Controllers;

public class AccountController : Controller
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<AccountController> _logger;

	public AccountController(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		ILogger<AccountController> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
	}

	// GET: /Account/Login
	[HttpGet]
	[AllowAnonymous]
	public IActionResult Login(string? returnUrl = null)
	{
		if (User.Identity?.IsAuthenticated == true)
		{
			return RedirectToAction("Index", "Home");
		}

		ViewData["ReturnUrl"] = returnUrl;
		return View(new LoginViewModel { ReturnUrl = returnUrl });
	}

	// POST: /Account/Login
	[HttpPost]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(LoginViewModel model)
	{
		ViewData["ReturnUrl"] = model.ReturnUrl;

		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var user = await _userManager.FindByEmailAsync(model.Email);
		if (user == null)
		{
			ModelState.AddModelError(string.Empty, "Invalid login attempt.");
			return View(model);
		}

		var result = await _signInManager.PasswordSignInAsync(
			user.UserName!,
			model.Password,
			model.RememberMe,
			lockoutOnFailure: true);

		if (result.Succeeded)
		{
			_logger.LogInformation("User {Email} logged in.", model.Email);
			
			// Redirect based on user role
			var roles = await _userManager.GetRolesAsync(user);
			if (roles.Contains("Lecturer"))
			{
				return RedirectToAction("Dashboard", "Lecturer");
			}
			else if (roles.Contains("ProgrammeCoordinator"))
			{
				return RedirectToAction("Coordinator", "Admin");
			}
			else if (roles.Contains("AcademicManager"))
			{
				return RedirectToAction("Manager", "Admin");
			}
			else if (roles.Contains("HR"))
			{
				return RedirectToAction("Dashboard", "HR");
			}

			return RedirectToLocal(model.ReturnUrl);
		}

		if (result.IsLockedOut)
		{
			_logger.LogWarning("User {Email} account locked out.", model.Email);
			TempData["ErrorMessage"] = "Your account has been locked due to too many failed login attempts. Please try again in 5 minutes.";
			return View(model);
		}

		ModelState.AddModelError(string.Empty, "Invalid login attempt.");
		return View(model);
	}

	// GET: /Account/Register
	[HttpGet]
	[AllowAnonymous]
	public IActionResult Register()
	{
		return View(new RegisterViewModel());
	}

	// POST: /Account/Register
	[HttpPost]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(RegisterViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		var user = new ApplicationUser
		{
			UserName = model.Email,
			Email = model.Email,
			FullName = model.FullName,
			EmailConfirmed = true // Auto-confirm for demo purposes
		};

		var result = await _userManager.CreateAsync(user, model.Password);

		if (result.Succeeded)
		{
			// Add user to the selected role
			await _userManager.AddToRoleAsync(user, model.Role);

			_logger.LogInformation("User {Email} created a new account with role {Role}.", model.Email, model.Role);

			// Sign in the user
			await _signInManager.SignInAsync(user, isPersistent: false);

			TempData["SuccessMessage"] = $"Account created successfully! Welcome, {user.FullName}!";

			// Redirect based on role
			if (model.Role == "Lecturer")
			{
				return RedirectToAction("Dashboard", "Lecturer");
			}
			else if (model.Role == "ProgrammeCoordinator")
			{
				return RedirectToAction("Coordinator", "Admin");
			}
			else if (model.Role == "AcademicManager")
			{
				return RedirectToAction("Manager", "Admin");
			}
			else if (model.Role == "HR")
			{
				return RedirectToAction("Dashboard", "HR");
			}

			return RedirectToAction("Index", "Home");
		}

		foreach (var error in result.Errors)
		{
			ModelState.AddModelError(string.Empty, error.Description);
		}

		return View(model);
	}

	// POST: /Account/Logout
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();
		_logger.LogInformation("User logged out.");
		TempData["SuccessMessage"] = "You have been logged out successfully.";
		return RedirectToAction("Index", "Home");
	}

	// GET: /Account/AccessDenied
	[HttpGet]
	public IActionResult AccessDenied()
	{
		return View();
	}

	#region Helpers

	private IActionResult RedirectToLocal(string? returnUrl)
	{
		if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
		{
			return Redirect(returnUrl);
		}
		return RedirectToAction("Index", "Home");
	}

	#endregion
}

