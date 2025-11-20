namespace CMCS.Web.Models.ViewModels;

public class ClaimStatusViewModel
{
	public int ClaimId { get; set; }
	public ClaimStatus CurrentStatus { get; set; }
	public string StatusText { get; set; } = string.Empty;
	public string StatusColor { get; set; } = string.Empty;
	public int ProgressPercentage { get; set; }
	public List<StatusStep> StatusSteps { get; set; } = new();
}

public class StatusStep
{
	public string StepName { get; set; } = string.Empty;
	public bool IsCompleted { get; set; }
	public bool IsCurrent { get; set; }
	public string Icon { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}

public static class ClaimStatusHelper
{
	public static ClaimStatusViewModel GetStatusViewModel(Claim claim)
	{
		var viewModel = new ClaimStatusViewModel
		{
			ClaimId = claim.Id,
			CurrentStatus = claim.Status
		};

		// Set status text and color
		switch (claim.Status)
		{
			case ClaimStatus.Draft:
				viewModel.StatusText = "Draft";
				viewModel.StatusColor = "secondary";
				viewModel.ProgressPercentage = 0;
				break;
			case ClaimStatus.Submitted:
				viewModel.StatusText = "Pending Verification";
				viewModel.StatusColor = "primary";
				viewModel.ProgressPercentage = 25;
				break;
			case ClaimStatus.Verified:
				viewModel.StatusText = "Pending Approval";
				viewModel.StatusColor = "info";
				viewModel.ProgressPercentage = 50;
				break;
			case ClaimStatus.Approved:
				viewModel.StatusText = "Approved";
				viewModel.StatusColor = "success";
				viewModel.ProgressPercentage = 75;
				break;
			case ClaimStatus.Rejected:
				viewModel.StatusText = "Rejected";
				viewModel.StatusColor = "danger";
				viewModel.ProgressPercentage = 100;
				break;
			case ClaimStatus.Settled:
				viewModel.StatusText = "Settled";
				viewModel.StatusColor = "dark";
				viewModel.ProgressPercentage = 100;
				break;
		}

		// Build status steps
		viewModel.StatusSteps = new List<StatusStep>
		{
			new StatusStep
			{
				StepName = "Submitted",
				IsCompleted = claim.Status >= ClaimStatus.Submitted,
				IsCurrent = claim.Status == ClaimStatus.Submitted,
				Icon = "📝",
				Description = "Claim submitted by lecturer"
			},
			new StatusStep
			{
				StepName = "Verified",
				IsCompleted = claim.Status >= ClaimStatus.Verified && claim.Status != ClaimStatus.Rejected,
				IsCurrent = claim.Status == ClaimStatus.Verified,
				Icon = "🔍",
				Description = "Verified by Programme Coordinator"
			},
			new StatusStep
			{
				StepName = "Approved",
				IsCompleted = claim.Status >= ClaimStatus.Approved && claim.Status != ClaimStatus.Rejected,
				IsCurrent = claim.Status == ClaimStatus.Approved,
				Icon = "✅",
				Description = "Approved by Academic Manager"
			},
			new StatusStep
			{
				StepName = "Settled",
				IsCompleted = claim.Status == ClaimStatus.Settled,
				IsCurrent = claim.Status == ClaimStatus.Settled,
				Icon = "💰",
				Description = "Payment processed"
			}
		};

		// Handle rejected status
		if (claim.Status == ClaimStatus.Rejected)
		{
			viewModel.StatusSteps.Add(new StatusStep
			{
				StepName = "Rejected",
				IsCompleted = true,
				IsCurrent = true,
				Icon = "❌",
				Description = "Claim rejected"
			});
		}

		return viewModel;
	}

	public static string GetStatusBadgeClass(ClaimStatus status)
	{
		return status switch
		{
			ClaimStatus.Draft => "bg-secondary",
			ClaimStatus.Submitted => "bg-primary",
			ClaimStatus.Verified => "bg-info",
			ClaimStatus.Approved => "bg-success",
			ClaimStatus.Rejected => "bg-danger",
			ClaimStatus.Settled => "bg-dark",
			_ => "bg-secondary"
		};
	}

	public static string GetStatusIcon(ClaimStatus status)
	{
		return status switch
		{
			ClaimStatus.Draft => "📄",
			ClaimStatus.Submitted => "📝",
			ClaimStatus.Verified => "🔍",
			ClaimStatus.Approved => "✅",
			ClaimStatus.Rejected => "❌",
			ClaimStatus.Settled => "💰",
			_ => "📄"
		};
	}
}

