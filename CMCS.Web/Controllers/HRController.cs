using CMCS.Web.Models;
using CMCS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CMCS.Web.Controllers;

[Authorize(Roles = "HR")]
public class HRController : Controller
{
	private readonly ClaimsService _claimsService;
	private readonly LecturerService _lecturerService;
	private readonly InvoiceService _invoiceService;

	public HRController(ClaimsService claimsService, LecturerService lecturerService, InvoiceService invoiceService)
	{
		_claimsService = claimsService;
		_lecturerService = lecturerService;
		_invoiceService = invoiceService;
	}

	// Main HR Dashboard
	public IActionResult Dashboard()
	{
		var allClaims = _claimsService.GetAllClaims().ToList();
		var approvedClaims = allClaims.Where(c => c.Status == ClaimStatus.Approved).ToList();
		var settledClaims = allClaims.Where(c => c.Status == ClaimStatus.Settled).ToList();

		ViewBag.ApprovedClaimsCount = approvedClaims.Count;
		ViewBag.SettledClaimsCount = settledClaims.Count;
		ViewBag.TotalPendingPayment = approvedClaims.Sum(c => c.Amount);
		ViewBag.TotalPaidOut = settledClaims.Sum(c => c.Amount);
		ViewBag.ActiveLecturers = _lecturerService.GetAllLecturers().Count(l => l.IsActive);

		return View();
	}

	// Approved Claims for Payment Processing
	public IActionResult ApprovedClaims()
	{
		var approvedClaims = _claimsService.GetClaimsByStatus(ClaimStatus.Approved);
		return View(approvedClaims);
	}

	// Generate Invoice for a Claim
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult GenerateInvoice(int claimId)
	{
		try
		{
			var invoice = _invoiceService.GenerateInvoice(claimId);
			var htmlContent = _invoiceService.GenerateInvoiceHtml(invoice);

			// Mark claim as settled
			_claimsService.UpdateClaimStatus(claimId, ClaimStatus.Settled);

			TempData["SuccessMessage"] = $"Invoice {invoice.InvoiceNumber} generated successfully for Claim #{claimId}";
			TempData["InvoiceHtml"] = htmlContent;
			TempData["InvoiceNumber"] = invoice.InvoiceNumber;

			return RedirectToAction(nameof(ViewInvoice), new { claimId });
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error generating invoice: {ex.Message}";
			return RedirectToAction(nameof(ApprovedClaims));
		}
	}

	// View Invoice
	public IActionResult ViewInvoice(int claimId)
	{
		try
		{
			var invoice = _invoiceService.GenerateInvoice(claimId);
			ViewBag.InvoiceHtml = _invoiceService.GenerateInvoiceHtml(invoice);
			return View(invoice);
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error viewing invoice: {ex.Message}";
			return RedirectToAction(nameof(ApprovedClaims));
		}
	}

	// Monthly Reports
	public IActionResult Reports()
	{
		var currentMonth = DateTime.UtcNow.Month;
		var currentYear = DateTime.UtcNow.Year;
		var report = _invoiceService.GenerateMonthlyReport(currentMonth, currentYear);
		return View(report);
	}

	// Generate Monthly Report
	[HttpPost]
	public IActionResult GenerateReport(int month, int year)
	{
		var report = _invoiceService.GenerateMonthlyReport(month, year);
		return View("Reports", report);
	}

	// Payment Batch Processing
	public IActionResult ProcessPayments()
	{
		var approvedClaims = _claimsService.GetClaimsByStatus(ClaimStatus.Approved).ToList();
		return View(approvedClaims);
	}

	// Create Payment Batch
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult CreatePaymentBatch(List<int> selectedClaims)
	{
		try
		{
			if (selectedClaims == null || !selectedClaims.Any())
			{
				TempData["ErrorMessage"] = "Please select at least one claim to process";
				return RedirectToAction(nameof(ProcessPayments));
			}

			var batch = _invoiceService.GeneratePaymentBatch(selectedClaims);

			// Mark all claims as settled
			foreach (var claimId in selectedClaims)
			{
				_claimsService.UpdateClaimStatus(claimId, ClaimStatus.Settled);
			}

			TempData["SuccessMessage"] = $"Payment batch {batch.BatchNumber} created successfully with {batch.ClaimCount} claim(s). Total: R {batch.TotalNetAmount:N2}";
			return RedirectToAction(nameof(ApprovedClaims));
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error creating payment batch: {ex.Message}";
			return RedirectToAction(nameof(ProcessPayments));
		}
	}

	// Lecturer Management
	public IActionResult Lecturers()
	{
		var lecturers = _lecturerService.GetAllLecturers();
		return View(lecturers);
	}

	// View Lecturer Details
	public IActionResult LecturerDetails(int id)
	{
		var lecturer = _lecturerService.GetLecturerById(id);
		if (lecturer == null)
		{
			TempData["ErrorMessage"] = "Lecturer not found";
			return RedirectToAction(nameof(Lecturers));
		}

		// Get lecturer's claims
		var lecturerClaims = _claimsService.GetAllClaims()
			.Where(c => c.LecturerId == id)
			.OrderByDescending(c => c.Year)
			.ThenByDescending(c => c.Month)
			.ToList();

		ViewBag.Claims = lecturerClaims;
		return View(lecturer);
	}

	// Edit Lecturer
	[HttpGet]
	public IActionResult EditLecturer(int id)
	{
		var lecturer = _lecturerService.GetLecturerById(id);
		if (lecturer == null)
		{
			TempData["ErrorMessage"] = "Lecturer not found";
			return RedirectToAction(nameof(Lecturers));
		}
		return View(lecturer);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult EditLecturer(Lecturer lecturer)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return View(lecturer);
			}

			lecturer.UpdatedAt = DateTime.UtcNow;

			if (_lecturerService.UpdateLecturer(lecturer))
			{
				TempData["SuccessMessage"] = "Lecturer information updated successfully";
				return RedirectToAction(nameof(LecturerDetails), new { id = lecturer.Id });
			}
			else
			{
				TempData["ErrorMessage"] = "Failed to update lecturer information";
				return View(lecturer);
			}
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error updating lecturer: {ex.Message}";
			return View(lecturer);
		}
	}

	// Add New Lecturer
	[HttpGet]
	public IActionResult AddLecturer()
	{
		return View(new Lecturer());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult AddLecturer(Lecturer lecturer)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return View(lecturer);
			}

			_lecturerService.AddLecturer(lecturer);
			TempData["SuccessMessage"] = "Lecturer added successfully";
			return RedirectToAction(nameof(Lecturers));
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = $"Error adding lecturer: {ex.Message}";
			return View(lecturer);
		}
	}
}

