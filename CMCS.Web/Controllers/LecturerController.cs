using CMCS.Web.Models;
using CMCS.Web.Models.ViewModels;
using CMCS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CMCS.Web.Controllers;

[Authorize(Roles = "Lecturer")]
public class LecturerController : Controller
{
	private readonly ClaimsService _claimsService;

	public LecturerController(ClaimsService claimsService)
	{
		_claimsService = claimsService;
	}

	public IActionResult Dashboard()
	{
		var allClaims = _claimsService.GetAllClaims().ToList();
		
		// Calculate statistics
		ViewBag.TotalClaims = allClaims.Count;
		ViewBag.PendingClaims = allClaims.Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.Verified);
		ViewBag.ApprovedClaims = allClaims.Count(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Settled);
		ViewBag.RejectedClaims = allClaims.Count(c => c.Status == ClaimStatus.Rejected);
		ViewBag.TotalAmount = allClaims.Where(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Settled).Sum(c => c.Amount);
		
		// Recent claims
		ViewBag.RecentClaims = allClaims.OrderByDescending(c => c.Id).Take(5).ToList();
		
		return View();
	}

	[HttpGet]
	public IActionResult Submit()
	{
		// Clear any error messages from previous pages
		TempData.Remove("ErrorMessage");
		
		var vm = new ClaimSubmissionViewModel
		{
			Month = DateTime.UtcNow.Month,
			Year = DateTime.UtcNow.Year,
			HourlyRate = 0,
			Items = new List<ClaimLineItem>
			{
				new ClaimLineItem() // Pre-populate one empty row
			}
		};
		return View(vm);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Submit(ClaimSubmissionViewModel model)
	{
		try
		{
			// Remove empty items before validation
			model.Items = model.Items?.Where(x => !string.IsNullOrWhiteSpace(x.ActivityDescription) || x.Hours > 0).ToList() ?? new List<ClaimLineItem>();

			// Validate at least one activity is provided
			if (!model.Items.Any())
			{
				ModelState.AddModelError("Items", "At least one activity is required");
			}

			// Validate individual items
			for (int i = 0; i < model.Items.Count; i++)
			{
				if (string.IsNullOrWhiteSpace(model.Items[i].ActivityDescription))
				{
					ModelState.AddModelError($"Items[{i}].ActivityDescription", "Activity description is required");
				}
				if (model.Items[i].Hours <= 0)
				{
					ModelState.AddModelError($"Items[{i}].Hours", "Hours must be greater than 0");
				}
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Create and save the claim
			var claim = new Claim
			{
				LecturerId = 1, // Hardcoded for prototype
				Month = model.Month,
				Year = model.Year,
				HourlyRate = model.HourlyRate,
				TotalHours = model.TotalHours,
				Amount = model.TotalAmount,
				Status = ClaimStatus.Submitted,
				Lines = model.Items.Select(item => new ClaimLine
				{
					ActivityDescription = item.ActivityDescription,
					Hours = item.Hours
				}).ToList()
			};

			claim = _claimsService.AddClaim(claim);

			TempData["SuccessMessage"] = $"Claim submitted successfully! Claim ID: {claim.Id}. Total Amount: R{claim.Amount:N2}";
			return RedirectToAction(nameof(MyClaims));
		}
		catch (Exception ex)
		{
			ModelState.AddModelError(string.Empty, $"An error occurred while submitting your claim: {ex.Message}");
			return View(model);
		}
	}

	public IActionResult MyClaims()
	{
		var claims = _claimsService.GetAllClaims();
		return View(claims);
	}

	[HttpGet]
	public IActionResult TrackClaim(int id)
	{
		var claim = _claimsService.GetClaimById(id);
		if (claim == null)
		{
			TempData["ErrorMessage"] = "Claim not found.";
			return RedirectToAction(nameof(MyClaims));
		}

		var statusViewModel = ClaimStatusHelper.GetStatusViewModel(claim);
		ViewBag.Claim = claim;
		return View(statusViewModel);
	}

	[HttpGet]
	public IActionResult UploadDocuments(int id)
	{
		var claim = _claimsService.GetClaimById(id);
		if (claim == null)
		{
			TempData["ErrorMessage"] = "Claim not found.";
			return RedirectToAction(nameof(MyClaims));
		}

		var viewModel = new DocumentUploadViewModel
		{
			ClaimId = id,
		ExistingDocuments = claim.Documents?.ToList() ?? new List<Document>()
		};

		return View(viewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> UploadDocuments(int id, List<IFormFile> files)
	{
		try
		{
			var claim = _claimsService.GetClaimById(id);
			if (claim == null)
			{
				TempData["ErrorMessage"] = "Claim not found.";
				return RedirectToAction(nameof(MyClaims));
			}

			if (files == null || !files.Any())
			{
				TempData["ErrorMessage"] = "Please select at least one file to upload.";
				return RedirectToAction(nameof(UploadDocuments), new { id });
			}

			// Validate files
			var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls" };
			const long maxFileSize = 5 * 1024 * 1024; // 5 MB

			foreach (var file in files)
			{
				if (file.Length == 0)
					continue;

				// Check file size
				if (file.Length > maxFileSize)
				{
					TempData["ErrorMessage"] = $"File '{file.FileName}' exceeds the maximum size of 5 MB.";
					return RedirectToAction(nameof(UploadDocuments), new { id });
				}

				// Check file extension
				var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
				if (!allowedExtensions.Contains(extension))
				{
					TempData["ErrorMessage"] = $"File '{file.FileName}' has an invalid extension. Only PDF, DOCX, DOC, XLSX, and XLS files are allowed.";
					return RedirectToAction(nameof(UploadDocuments), new { id });
				}

				// Create uploads directory if it doesn't exist
				var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", id.ToString());
				Directory.CreateDirectory(uploadsPath);

				// Generate unique filename
				var uniqueFileName = $"{Guid.NewGuid()}{extension}";
				var filePath = Path.Combine(uploadsPath, uniqueFileName);

				// Save file to disk
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// Add document to claim
				var document = new Document
				{
					FileName = file.FileName,
					FilePath = $"/uploads/{id}/{uniqueFileName}",
					UploadedAt = DateTime.UtcNow
				};

				_claimsService.AddDocumentToClaim(id, document);
			}

			TempData["SuccessMessage"] = $"{files.Count} document(s) uploaded successfully!";
			return RedirectToAction(nameof(UploadDocuments), new { id });
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"An error occurred while uploading documents: {ex.Message}";
			return RedirectToAction(nameof(UploadDocuments), new { id });
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult DeleteDocument(int claimId, int documentId, string filePath)
	{
		try
		{
			// Delete file from disk
			if (!string.IsNullOrEmpty(filePath))
			{
				var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
				if (System.IO.File.Exists(fullPath))
				{
					System.IO.File.Delete(fullPath);
				}
			}

			// Remove from claim
			_claimsService.RemoveDocumentFromClaim(claimId, documentId);

			TempData["SuccessMessage"] = "Document deleted successfully.";
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"An error occurred while deleting the document: {ex.Message}";
		}

		return RedirectToAction(nameof(UploadDocuments), new { id = claimId });
	}
}


