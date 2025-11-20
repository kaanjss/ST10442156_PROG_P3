using CMCS.Web.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CMCS.Tests.ViewModels;

public class ClaimSubmissionViewModelTests
{
	[Fact]
	public void TotalHours_WithMultipleItems_ShouldSumCorrectly()
	{
		// Arrange
		var viewModel = new ClaimSubmissionViewModel
		{
			Items = new List<ClaimLineItem>
			{
				new ClaimLineItem { Hours = 5 },
				new ClaimLineItem { Hours = 3.5m },
				new ClaimLineItem { Hours = 2 }
			}
		};

		// Act
		var totalHours = viewModel.TotalHours;

		// Assert
		Assert.Equal(10.5m, totalHours);
	}

	[Fact]
	public void TotalAmount_ShouldCalculateCorrectly()
	{
		// Arrange
		var viewModel = new ClaimSubmissionViewModel
		{
			HourlyRate = 500,
			Items = new List<ClaimLineItem>
			{
				new ClaimLineItem { Hours = 10 }
			}
		};

		// Act
		var totalAmount = viewModel.TotalAmount;

		// Assert
		Assert.Equal(5000, totalAmount);
	}

	[Fact]
	public void Month_WithInvalidValue_ShouldFailValidation()
	{
		// Arrange
		var viewModel = new ClaimSubmissionViewModel
		{
			Month = 13, // Invalid month
			Year = 2025,
			HourlyRate = 500
		};

		// Act
		var validationResults = new List<ValidationResult>();
		var context = new ValidationContext(viewModel);
		var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

		// Assert
		Assert.False(isValid);
		Assert.Contains(validationResults, v => v.MemberNames.Contains("Month"));
	}

	[Fact]
	public void HourlyRate_WithZeroValue_ShouldFailValidation()
	{
		// Arrange
		var viewModel = new ClaimSubmissionViewModel
		{
			Month = 10,
			Year = 2025,
			HourlyRate = 0 // Invalid rate
		};

		// Act
		var validationResults = new List<ValidationResult>();
		var context = new ValidationContext(viewModel);
		var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

		// Assert
		Assert.False(isValid);
	}

	[Fact]
	public void AdditionalNotes_WithTooLongText_ShouldFailValidation()
	{
		// Arrange
		var viewModel = new ClaimSubmissionViewModel
		{
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			AdditionalNotes = new string('A', 501) // Exceeds 500 character limit
		};

		// Act
		var validationResults = new List<ValidationResult>();
		var context = new ValidationContext(viewModel);
		var isValid = Validator.TryValidateObject(viewModel, context, validationResults, true);

		// Assert
		Assert.False(isValid);
		Assert.Contains(validationResults, v => v.MemberNames.Contains("AdditionalNotes"));
	}

	[Fact]
	public void ClaimLineItem_WithInvalidHours_ShouldFailValidation()
	{
		// Arrange
		var item = new ClaimLineItem
		{
			ActivityDescription = "Lecture",
			Hours = 0 // Invalid hours
		};

		// Act
		var validationResults = new List<ValidationResult>();
		var context = new ValidationContext(item);
		var isValid = Validator.TryValidateObject(item, context, validationResults, true);

		// Assert
		Assert.False(isValid);
		Assert.Contains(validationResults, v => v.MemberNames.Contains("Hours"));
	}

	[Fact]
	public void ClaimLineItem_WithValidData_ShouldPassValidation()
	{
		// Arrange
		var item = new ClaimLineItem
		{
			ActivityDescription = "Lecture: PROG6212",
			Hours = 5
		};

		// Act
		var validationResults = new List<ValidationResult>();
		var context = new ValidationContext(item);
		var isValid = Validator.TryValidateObject(item, context, validationResults, true);

		// Assert
		Assert.True(isValid);
		Assert.Empty(validationResults);
	}
}

