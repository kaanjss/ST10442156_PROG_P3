using CMCS.Web.Models;
using CMCS.Web.Models.ViewModels;
using Xunit;

namespace CMCS.Tests.ViewModels;

public class ClaimStatusHelperTests
{
	[Fact]
	public void GetStatusViewModel_SubmittedClaim_ShouldReturn25PercentProgress()
	{
		// Arrange
		var claim = new Claim
		{
			Id = 1,
			Status = ClaimStatus.Submitted,
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			TotalHours = 10,
			Amount = 5000
		};

		// Act
		var result = ClaimStatusHelper.GetStatusViewModel(claim);

		// Assert
		Assert.Equal(ClaimStatus.Submitted, result.CurrentStatus);
		Assert.Equal("Pending Verification", result.StatusText);
		Assert.Equal("primary", result.StatusColor);
		Assert.Equal(25, result.ProgressPercentage);
		Assert.Equal(4, result.StatusSteps.Count);
	}

	[Fact]
	public void GetStatusViewModel_VerifiedClaim_ShouldReturn50PercentProgress()
	{
		// Arrange
		var claim = new Claim
		{
			Id = 1,
			Status = ClaimStatus.Verified,
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			TotalHours = 10,
			Amount = 5000
		};

		// Act
		var result = ClaimStatusHelper.GetStatusViewModel(claim);

		// Assert
		Assert.Equal(ClaimStatus.Verified, result.CurrentStatus);
		Assert.Equal("Pending Approval", result.StatusText);
		Assert.Equal("info", result.StatusColor);
		Assert.Equal(50, result.ProgressPercentage);
	}

	[Fact]
	public void GetStatusViewModel_ApprovedClaim_ShouldReturn75PercentProgress()
	{
		// Arrange
		var claim = new Claim
		{
			Id = 1,
			Status = ClaimStatus.Approved,
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			TotalHours = 10,
			Amount = 5000
		};

		// Act
		var result = ClaimStatusHelper.GetStatusViewModel(claim);

		// Assert
		Assert.Equal(ClaimStatus.Approved, result.CurrentStatus);
		Assert.Equal("Approved", result.StatusText);
		Assert.Equal("success", result.StatusColor);
		Assert.Equal(75, result.ProgressPercentage);
	}

	[Fact]
	public void GetStatusViewModel_RejectedClaim_ShouldHaveRejectedStep()
	{
		// Arrange
		var claim = new Claim
		{
			Id = 1,
			Status = ClaimStatus.Rejected,
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			TotalHours = 10,
			Amount = 5000
		};

		// Act
		var result = ClaimStatusHelper.GetStatusViewModel(claim);

		// Assert
		Assert.Equal(ClaimStatus.Rejected, result.CurrentStatus);
		Assert.Equal("Rejected", result.StatusText);
		Assert.Equal("danger", result.StatusColor);
		Assert.Contains(result.StatusSteps, s => s.StepName == "Rejected");
	}

	[Theory]
	[InlineData(ClaimStatus.Draft, "bg-secondary")]
	[InlineData(ClaimStatus.Submitted, "bg-primary")]
	[InlineData(ClaimStatus.Verified, "bg-info")]
	[InlineData(ClaimStatus.Approved, "bg-success")]
	[InlineData(ClaimStatus.Rejected, "bg-danger")]
	[InlineData(ClaimStatus.Settled, "bg-dark")]
	public void GetStatusBadgeClass_ShouldReturnCorrectClass(ClaimStatus status, string expectedClass)
	{
		// Act
		var result = ClaimStatusHelper.GetStatusBadgeClass(status);

		// Assert
		Assert.Equal(expectedClass, result);
	}

	[Theory]
	[InlineData(ClaimStatus.Draft, "📄")]
	[InlineData(ClaimStatus.Submitted, "📝")]
	[InlineData(ClaimStatus.Verified, "🔍")]
	[InlineData(ClaimStatus.Approved, "✅")]
	[InlineData(ClaimStatus.Rejected, "❌")]
	[InlineData(ClaimStatus.Settled, "💰")]
	public void GetStatusIcon_ShouldReturnCorrectIcon(ClaimStatus status, string expectedIcon)
	{
		// Act
		var result = ClaimStatusHelper.GetStatusIcon(status);

		// Assert
		Assert.Equal(expectedIcon, result);
	}

	[Fact]
	public void GetStatusViewModel_SubmittedClaim_FirstStepShouldBeCompleted()
	{
		// Arrange
		var claim = new Claim
		{
			Id = 1,
			Status = ClaimStatus.Submitted,
			Month = 10,
			Year = 2025
		};

		// Act
		var result = ClaimStatusHelper.GetStatusViewModel(claim);

		// Assert
		var submittedStep = result.StatusSteps.First(s => s.StepName == "Submitted");
		Assert.True(submittedStep.IsCompleted);
		Assert.True(submittedStep.IsCurrent);
	}

	[Fact]
	public void GetStatusViewModel_ApprovedClaim_ThreeStepsShouldBeCompleted()
	{
		// Arrange
		var claim = new Claim
		{
			Id = 1,
			Status = ClaimStatus.Approved,
			Month = 10,
			Year = 2025
		};

		// Act
		var result = ClaimStatusHelper.GetStatusViewModel(claim);

		// Assert
		var completedSteps = result.StatusSteps.Count(s => s.IsCompleted);
		Assert.Equal(3, completedSteps); // Submitted, Verified, Approved
	}
}

