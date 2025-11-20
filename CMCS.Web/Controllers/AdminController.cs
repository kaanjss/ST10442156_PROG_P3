using CMCS.Web.Models;
using CMCS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CMCS.Web.Controllers;

[Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
public class AdminController : Controller
{
	private readonly ClaimsService _claimsService;
	private readonly ClaimValidationService _validationService;

	public AdminController(ClaimsService claimsService, ClaimValidationService validationService)
	{
		_claimsService = claimsService;
		_validationService = validationService;
	}

	[Authorize(Roles = "ProgrammeCoordinator")]
	public IActionResult Coordinator()
	{
		var pendingClaims = _claimsService.GetPendingClaimsForCoordinator().ToList();
		
		// Perform automated validation for each claim
		var validationResults = pendingClaims.ToDictionary(
			claim => claim.Id,
			claim => _validationService.ValidateClaim(claim)
		);
		
		ViewBag.ValidationResults = validationResults;
		return View(pendingClaims);
	}

	[Authorize(Roles = "AcademicManager")]
	public IActionResult Manager()
	{
		var verifiedClaims = _claimsService.GetPendingClaimsForManager().ToList();
		
		// Perform automated validation for each claim
		var validationResults = verifiedClaims.ToDictionary(
			claim => claim.Id,
			claim => _validationService.ValidateClaim(claim)
		);
		
		ViewBag.ValidationResults = validationResults;
		return View(verifiedClaims);
	}

	public IActionResult Review(int id)
	{
		var claim = _claimsService.GetClaimById(id);
		if (claim == null)
		{
			TempData["ErrorMessage"] = "Claim not found.";
			return RedirectToAction(nameof(Coordinator));
		}
		
		// Perform automated validation
		var validationResult = _validationService.ValidateClaim(claim);
		ViewBag.ValidationResult = validationResult;
		
		return View(claim);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult VerifyClaim(int id, string comment)
	{
		try
		{
			var claim = _claimsService.GetClaimById(id);
			if (claim == null)
			{
				TempData["ErrorMessage"] = "Claim not found.";
				return RedirectToAction(nameof(Coordinator));
			}

			if (_claimsService.VerifyClaim(id, comment))
			{
				TempData["SuccessMessage"] = $"Claim #{id} has been verified successfully.";
			}
			else
			{
				TempData["ErrorMessage"] = "Failed to verify the claim.";
			}
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
		}

		return RedirectToAction(nameof(Coordinator));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult ApproveClaim(int id, string comment)
	{
		try
		{
			var claim = _claimsService.GetClaimById(id);
			if (claim == null)
			{
				TempData["ErrorMessage"] = "Claim not found.";
				return RedirectToAction(nameof(Manager));
			}

			if (_claimsService.ApproveClaim(id, comment))
			{
				TempData["SuccessMessage"] = $"Claim #{id} has been approved successfully.";
			}
			else
			{
				TempData["ErrorMessage"] = "Failed to approve the claim.";
			}
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
		}

		return RedirectToAction(nameof(Manager));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult RejectClaim(int id, string comment, string returnTo = "Coordinator")
	{
		try
		{
			if (string.IsNullOrWhiteSpace(comment))
			{
				TempData["ErrorMessage"] = "A comment is required when rejecting a claim.";
				return RedirectToAction(returnTo);
			}

			var claim = _claimsService.GetClaimById(id);
			if (claim == null)
			{
				TempData["ErrorMessage"] = "Claim not found.";
				return RedirectToAction(returnTo);
			}

			if (_claimsService.RejectClaim(id, comment))
			{
				TempData["SuccessMessage"] = $"Claim #{id} has been rejected.";
			}
			else
			{
				TempData["ErrorMessage"] = "Failed to reject the claim.";
			}
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
		}

		return RedirectToAction(returnTo);
	}
}


