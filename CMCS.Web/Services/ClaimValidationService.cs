using CMCS.Web.Models;

namespace CMCS.Web.Services;

/// <summary>
/// Service for automated claim validation against predefined policies
/// </summary>
public class ClaimValidationService
{
	// Validation Policy Criteria
	private const decimal MIN_HOURLY_RATE = 200m;
	private const decimal MAX_HOURLY_RATE = 1000m;
	private const decimal MAX_HOURS_PER_MONTH = 200m;
	private const decimal MAX_HOURS_PER_ACTIVITY = 50m;
	private const decimal MIN_CLAIM_AMOUNT = 500m;
	private const decimal MAX_CLAIM_AMOUNT = 150000m;

	public ClaimValidationResult ValidateClaim(Claim claim)
	{
		var result = new ClaimValidationResult
		{
			ClaimId = claim.Id,
			IsValid = true,
			ValidationIssues = new List<ValidationIssue>()
		};

		// Validate hourly rate
		if (claim.HourlyRate < MIN_HOURLY_RATE)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Error,
				Category = "Hourly Rate",
				Message = $"Hourly rate (R {claim.HourlyRate:N2}) is below the minimum allowed rate of R {MIN_HOURLY_RATE:N2}",
				FieldName = nameof(claim.HourlyRate)
			});
			result.IsValid = false;
		}
		else if (claim.HourlyRate > MAX_HOURLY_RATE)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Warning,
				Category = "Hourly Rate",
				Message = $"Hourly rate (R {claim.HourlyRate:N2}) exceeds typical maximum of R {MAX_HOURLY_RATE:N2}. Requires additional approval.",
				FieldName = nameof(claim.HourlyRate)
			});
		}

		// Validate total hours
		if (claim.TotalHours > MAX_HOURS_PER_MONTH)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Error,
				Category = "Total Hours",
				Message = $"Total hours ({claim.TotalHours:N2}) exceeds maximum allowed of {MAX_HOURS_PER_MONTH:N2} hours per month",
				FieldName = nameof(claim.TotalHours)
			});
			result.IsValid = false;
		}
		else if (claim.TotalHours > MAX_HOURS_PER_MONTH * 0.8m) // 80% threshold
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Warning,
				Category = "Total Hours",
				Message = $"Total hours ({claim.TotalHours:N2}) is approaching the maximum limit of {MAX_HOURS_PER_MONTH:N2} hours",
				FieldName = nameof(claim.TotalHours)
			});
		}

		// Validate individual activity hours
		foreach (var line in claim.Lines)
		{
			if (line.Hours > MAX_HOURS_PER_ACTIVITY)
			{
				result.ValidationIssues.Add(new ValidationIssue
				{
					Severity = ValidationSeverity.Warning,
					Category = "Activity Hours",
					Message = $"Activity '{line.ActivityDescription}' has {line.Hours:N2} hours, which exceeds typical maximum of {MAX_HOURS_PER_ACTIVITY:N2} hours per activity",
					FieldName = "ActivityHours"
				});
			}
		}

		// Validate claim amount
		if (claim.Amount < MIN_CLAIM_AMOUNT)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Info,
				Category = "Claim Amount",
				Message = $"Claim amount (R {claim.Amount:N2}) is below typical minimum of R {MIN_CLAIM_AMOUNT:N2}",
				FieldName = nameof(claim.Amount)
			});
		}
		else if (claim.Amount > MAX_CLAIM_AMOUNT)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Error,
				Category = "Claim Amount",
				Message = $"Claim amount (R {claim.Amount:N2}) exceeds maximum allowed of R {MAX_CLAIM_AMOUNT:N2}",
				FieldName = nameof(claim.Amount)
			});
			result.IsValid = false;
		}

		// Validate calculation accuracy
		var calculatedAmount = claim.TotalHours * claim.HourlyRate;
		if (Math.Abs(claim.Amount - calculatedAmount) > 0.01m)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Error,
				Category = "Calculation Error",
				Message = $"Claim amount (R {claim.Amount:N2}) does not match calculated amount (R {calculatedAmount:N2})",
				FieldName = nameof(claim.Amount)
			});
			result.IsValid = false;
		}

		// Validate total hours match sum of activity hours
		var totalActivityHours = claim.Lines.Sum(l => l.Hours);
		if (Math.Abs(claim.TotalHours - totalActivityHours) > 0.01m)
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Error,
				Category = "Calculation Error",
				Message = $"Total hours ({claim.TotalHours:N2}) does not match sum of activity hours ({totalActivityHours:N2})",
				FieldName = nameof(claim.TotalHours)
			});
			result.IsValid = false;
		}

		// Check for missing documents (warning only)
		if (!claim.Documents.Any())
		{
			result.ValidationIssues.Add(new ValidationIssue
			{
				Severity = ValidationSeverity.Warning,
				Category = "Documentation",
				Message = "No supporting documents have been uploaded for this claim",
				FieldName = "Documents"
			});
		}

		// Check for adequate activity descriptions
		foreach (var line in claim.Lines)
		{
			if (string.IsNullOrWhiteSpace(line.ActivityDescription) || line.ActivityDescription.Length < 10)
			{
				result.ValidationIssues.Add(new ValidationIssue
				{
					Severity = ValidationSeverity.Warning,
					Category = "Activity Description",
					Message = $"Activity description '{line.ActivityDescription}' is too brief. Please provide more details.",
					FieldName = "ActivityDescription"
				});
			}
		}

		// Set overall recommendation
		result.Recommendation = DetermineRecommendation(result);

		return result;
	}

	private string DetermineRecommendation(ClaimValidationResult result)
	{
		if (!result.IsValid)
		{
			return "Reject - Contains validation errors that must be corrected";
		}

		var errorCount = result.ValidationIssues.Count(i => i.Severity == ValidationSeverity.Error);
		var warningCount = result.ValidationIssues.Count(i => i.Severity == ValidationSeverity.Warning);

		if (errorCount > 0)
		{
			return "Reject - Contains validation errors";
		}
		else if (warningCount > 3)
		{
			return "Review Required - Multiple warnings need attention";
		}
		else if (warningCount > 0)
		{
			return "Approve with Caution - Some warnings present";
		}
		else
		{
			return "Approve - All automated checks passed";
		}
	}

	public string GetValidationSummary(ClaimValidationResult result)
	{
		var errorCount = result.ValidationIssues.Count(i => i.Severity == ValidationSeverity.Error);
		var warningCount = result.ValidationIssues.Count(i => i.Severity == ValidationSeverity.Warning);
		var infoCount = result.ValidationIssues.Count(i => i.Severity == ValidationSeverity.Info);

		if (errorCount == 0 && warningCount == 0 && infoCount == 0)
		{
			return "✓ All automated validation checks passed";
		}

		var parts = new List<string>();
		if (errorCount > 0) parts.Add($"{errorCount} error(s)");
		if (warningCount > 0) parts.Add($"{warningCount} warning(s)");
		if (infoCount > 0) parts.Add($"{infoCount} info");

		return $"Validation: {string.Join(", ", parts)}";
	}
}

public class ClaimValidationResult
{
	public int ClaimId { get; set; }
	public bool IsValid { get; set; }
	public List<ValidationIssue> ValidationIssues { get; set; } = new();
	public string Recommendation { get; set; } = string.Empty;
}

public class ValidationIssue
{
	public ValidationSeverity Severity { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public string FieldName { get; set; } = string.Empty;
}

public enum ValidationSeverity
{
	Info,
	Warning,
	Error
}

