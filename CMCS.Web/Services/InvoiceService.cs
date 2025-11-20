using CMCS.Web.Models;
using System.Text;

namespace CMCS.Web.Services;

/// <summary>
/// Service for generating invoices and reports for approved claims
/// </summary>
public class InvoiceService
{
	private readonly ClaimsService _claimsService;
	private readonly LecturerService _lecturerService;

	public InvoiceService(ClaimsService claimsService, LecturerService lecturerService)
	{
		_claimsService = claimsService;
		_lecturerService = lecturerService;
	}

	public Invoice GenerateInvoice(int claimId)
	{
		var claim = _claimsService.GetClaimById(claimId);
		if (claim == null)
			throw new ArgumentException($"Claim {claimId} not found");

		var lecturer = _lecturerService.GetLecturerById(claim.LecturerId);

		var invoice = new Invoice
		{
			InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{claimId:D6}",
			ClaimId = claimId,
			LecturerId = claim.LecturerId,
			LecturerName = lecturer?.FullName ?? $"Lecturer {claim.LecturerId}",
			Month = claim.Month,
			Year = claim.Year,
			TotalHours = claim.TotalHours,
			HourlyRate = claim.HourlyRate,
			GrossAmount = claim.Amount,
			TaxAmount = claim.Amount * 0.2m, // 20% tax
			NetAmount = claim.Amount * 0.8m,
			GeneratedDate = DateTime.UtcNow,
			Status = InvoiceStatus.Generated
		};

		return invoice;
	}

	public MonthlyReport GenerateMonthlyReport(int month, int year)
	{
		var allClaims = _claimsService.GetAllClaims().ToList();
		var monthClaims = allClaims.Where(c => c.Month == month && c.Year == year).ToList();

		var approvedClaims = monthClaims.Where(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Settled).ToList();

		var report = new MonthlyReport
		{
			Month = month,
			Year = year,
			GeneratedDate = DateTime.UtcNow,
			TotalClaims = monthClaims.Count,
			SubmittedClaims = monthClaims.Count(c => c.Status == ClaimStatus.Submitted),
			VerifiedClaims = monthClaims.Count(c => c.Status == ClaimStatus.Verified),
			ApprovedClaims = monthClaims.Count(c => c.Status == ClaimStatus.Approved),
			RejectedClaims = monthClaims.Count(c => c.Status == ClaimStatus.Rejected),
			SettledClaims = monthClaims.Count(c => c.Status == ClaimStatus.Settled),
			TotalHours = approvedClaims.Sum(c => c.TotalHours),
			TotalAmount = approvedClaims.Sum(c => c.Amount),
			TaxAmount = approvedClaims.Sum(c => c.Amount) * 0.2m,
			NetPayable = approvedClaims.Sum(c => c.Amount) * 0.8m,
			Claims = approvedClaims
		};

		return report;
	}

	public PaymentBatch GeneratePaymentBatch(List<int> claimIds)
	{
		var claims = claimIds.Select(id => _claimsService.GetClaimById(id)).Where(c => c != null).ToList();
		var approvedClaims = claims.Where(c => c!.Status == ClaimStatus.Approved).ToList();

		var batch = new PaymentBatch
		{
			BatchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
			GeneratedDate = DateTime.UtcNow,
			ClaimCount = approvedClaims.Count,
			TotalGrossAmount = approvedClaims.Sum(c => c!.Amount),
			TotalTaxAmount = approvedClaims.Sum(c => c!.Amount) * 0.2m,
			TotalNetAmount = approvedClaims.Sum(c => c!.Amount) * 0.8m,
			Status = PaymentBatchStatus.Pending,
			ClaimIds = approvedClaims.Select(c => c!.Id).ToList()
		};

		return batch;
	}

	public string GenerateInvoiceHtml(Invoice invoice)
	{
		var monthName = new DateTime(invoice.Year, invoice.Month, 1).ToString("MMMM yyyy");

		var html = new StringBuilder();
		html.AppendLine("<!DOCTYPE html>");
		html.AppendLine("<html><head><style>");
		html.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
		html.AppendLine(".header { text-align: center; margin-bottom: 30px; }");
		html.AppendLine(".invoice-details { margin: 20px 0; }");
		html.AppendLine(".invoice-table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
		html.AppendLine(".invoice-table th, .invoice-table td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
		html.AppendLine(".total { font-weight: bold; font-size: 1.2em; }");
		html.AppendLine("</style></head><body>");
		
		html.AppendLine("<div class='header'>");
		html.AppendLine("<h1>PAYMENT INVOICE</h1>");
		html.AppendLine($"<h2>Invoice Number: {invoice.InvoiceNumber}</h2>");
		html.AppendLine("</div>");
		
		html.AppendLine("<div class='invoice-details'>");
		html.AppendLine($"<p><strong>Lecturer:</strong> {invoice.LecturerName}</p>");
		html.AppendLine($"<p><strong>Period:</strong> {monthName}</p>");
		html.AppendLine($"<p><strong>Claim ID:</strong> #{invoice.ClaimId}</p>");
		html.AppendLine($"<p><strong>Invoice Date:</strong> {invoice.GeneratedDate:yyyy-MM-dd}</p>");
		html.AppendLine("</div>");
		
		html.AppendLine("<table class='invoice-table'>");
		html.AppendLine("<thead><tr><th>Description</th><th>Hours</th><th>Rate</th><th>Amount</th></tr></thead>");
		html.AppendLine("<tbody>");
		html.AppendLine($"<tr><td>Lecturing Services - {monthName}</td><td>{invoice.TotalHours:N2}</td><td>R {invoice.HourlyRate:N2}</td><td>R {invoice.GrossAmount:N2}</td></tr>");
		html.AppendLine("</tbody>");
		html.AppendLine("</table>");
		
		html.AppendLine("<div style='text-align: right; margin-top: 30px;'>");
		html.AppendLine($"<p><strong>Gross Amount:</strong> R {invoice.GrossAmount:N2}</p>");
		html.AppendLine($"<p><strong>Tax (20%):</strong> R {invoice.TaxAmount:N2}</p>");
		html.AppendLine($"<p class='total'><strong>Net Payable:</strong> R {invoice.NetAmount:N2}</p>");
		html.AppendLine("</div>");
		
		html.AppendLine("</body></html>");
		
		return html.ToString();
	}
}

public class Invoice
{
	public string InvoiceNumber { get; set; } = string.Empty;
	public int ClaimId { get; set; }
	public int LecturerId { get; set; }
	public string LecturerName { get; set; } = string.Empty;
	public int Month { get; set; }
	public int Year { get; set; }
	public decimal TotalHours { get; set; }
	public decimal HourlyRate { get; set; }
	public decimal GrossAmount { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal NetAmount { get; set; }
	public DateTime GeneratedDate { get; set; }
	public InvoiceStatus Status { get; set; }
}

public enum InvoiceStatus
{
	Generated,
	Sent,
	Paid
}

public class MonthlyReport
{
	public int Month { get; set; }
	public int Year { get; set; }
	public DateTime GeneratedDate { get; set; }
	public int TotalClaims { get; set; }
	public int SubmittedClaims { get; set; }
	public int VerifiedClaims { get; set; }
	public int ApprovedClaims { get; set; }
	public int RejectedClaims { get; set; }
	public int SettledClaims { get; set; }
	public decimal TotalHours { get; set; }
	public decimal TotalAmount { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal NetPayable { get; set; }
	public List<Claim> Claims { get; set; } = new();
}

public class PaymentBatch
{
	public string BatchNumber { get; set; } = string.Empty;
	public DateTime GeneratedDate { get; set; }
	public int ClaimCount { get; set; }
	public decimal TotalGrossAmount { get; set; }
	public decimal TotalTaxAmount { get; set; }
	public decimal TotalNetAmount { get; set; }
	public PaymentBatchStatus Status { get; set; }
	public List<int> ClaimIds { get; set; } = new();
}

public enum PaymentBatchStatus
{
	Pending,
	Processing,
	Completed,
	Failed
}

