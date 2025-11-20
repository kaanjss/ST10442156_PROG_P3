using CMCS.Web.Models;
using CMCS.Web.Services;
using CMCS.Web.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CMCS.Tests.Services;

public class ClaimsServiceTests
{
	private ApplicationDbContext CreateInMemoryContext()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
		
		return new ApplicationDbContext(options);
	}

	[Fact]
	public void AddClaim_ShouldAssignIdAndAddToDatabase()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		var claim = new Claim
		{
			LecturerId = 1,
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			TotalHours = 10,
			Amount = 5000,
			Status = ClaimStatus.Submitted
		};

		// Act
		var result = service.AddClaim(claim);

		// Assert
		Assert.NotEqual(0, result.Id);
		Assert.Equal(claim.LecturerId, result.LecturerId);
		Assert.Contains(result, service.GetAllClaims());
		
		context.Dispose();
	}

	[Fact]
	public void GetClaimById_ExistingClaim_ShouldReturnClaim()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		var claim = new Claim
		{
			LecturerId = 1,
			Month = 10,
			Year = 2025,
			HourlyRate = 500,
			TotalHours = 10,
			Amount = 5000,
			Status = ClaimStatus.Submitted
		};
		var addedClaim = service.AddClaim(claim);

		// Act
		var result = service.GetClaimById(addedClaim.Id);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(addedClaim.Id, result.Id);
		
		context.Dispose();
	}

	[Fact]
	public void GetClaimById_NonExistingClaim_ShouldReturnNull()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);

		// Act
		var result = service.GetClaimById(999);

		// Assert
		Assert.Null(result);
		
		context.Dispose();
	}

	[Fact]
	public void GetPendingClaimsForCoordinator_ShouldReturnSubmittedClaims()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Submitted });
		service.AddClaim(new Claim { LecturerId = 2, Month = 10, Year = 2025, HourlyRate = 600, TotalHours = 15, Amount = 9000, Status = ClaimStatus.Verified });

		// Act
		var result = service.GetPendingClaimsForCoordinator();

		// Assert
		Assert.Single(result);
		Assert.All(result, c => Assert.Equal(ClaimStatus.Submitted, c.Status));
		
		context.Dispose();
	}

	[Fact]
	public void GetPendingClaimsForManager_ShouldReturnVerifiedClaims()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Submitted });
		service.AddClaim(new Claim { LecturerId = 2, Month = 10, Year = 2025, HourlyRate = 600, TotalHours = 15, Amount = 9000, Status = ClaimStatus.Verified });

		// Act
		var result = service.GetPendingClaimsForManager();

		// Assert
		Assert.Single(result);
		Assert.All(result, c => Assert.Equal(ClaimStatus.Verified, c.Status));
		
		context.Dispose();
	}

	[Fact]
	public void VerifyClaim_ShouldChangeStatusToVerified()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		var claim = service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Submitted });

		// Act
		var result = service.VerifyClaim(claim.Id, "Verified by coordinator");

		// Assert
		Assert.True(result);
		var updatedClaim = service.GetClaimById(claim.Id);
		Assert.Equal(ClaimStatus.Verified, updatedClaim!.Status);
		
		context.Dispose();
	}

	[Fact]
	public void ApproveClaim_ShouldChangeStatusToApproved()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		var claim = service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Verified });

		// Act
		var result = service.ApproveClaim(claim.Id, "Approved by manager");

		// Assert
		Assert.True(result);
		var updatedClaim = service.GetClaimById(claim.Id);
		Assert.Equal(ClaimStatus.Approved, updatedClaim!.Status);
		
		context.Dispose();
	}

	[Fact]
	public void RejectClaim_ShouldChangeStatusToRejected()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		var claim = service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Submitted });

		// Act
		var result = service.RejectClaim(claim.Id, "Rejected due to missing documents");

		// Assert
		Assert.True(result);
		var updatedClaim = service.GetClaimById(claim.Id);
		Assert.Equal(ClaimStatus.Rejected, updatedClaim!.Status);
		
		context.Dispose();
	}

	[Fact]
	public void AddDocumentToClaim_ShouldAddDocumentToClaimsList()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		var claim = service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Submitted });
		var document = new Document { FileName = "test.pdf", FilePath = "/uploads/test.pdf", UploadedAt = DateTime.UtcNow };

		// Act
		service.AddDocumentToClaim(claim.Id, document);

		// Assert
		var updatedClaim = service.GetClaimById(claim.Id);
		Assert.Single(updatedClaim!.Documents);
		Assert.Equal("test.pdf", updatedClaim.Documents.First().FileName);
		
		context.Dispose();
	}

	[Fact]
	public void GetClaimsByStatus_ShouldReturnClaimsWithSpecificStatus()
	{
		// Arrange
		var context = CreateInMemoryContext();
		var service = new ClaimsService(context);
		service.AddClaim(new Claim { LecturerId = 1, Month = 10, Year = 2025, HourlyRate = 500, TotalHours = 10, Amount = 5000, Status = ClaimStatus.Approved });
		service.AddClaim(new Claim { LecturerId = 2, Month = 10, Year = 2025, HourlyRate = 600, TotalHours = 15, Amount = 9000, Status = ClaimStatus.Approved });
		service.AddClaim(new Claim { LecturerId = 3, Month = 10, Year = 2025, HourlyRate = 700, TotalHours = 20, Amount = 14000, Status = ClaimStatus.Rejected });

		// Act
		var result = service.GetClaimsByStatus(ClaimStatus.Approved);

		// Assert
		Assert.Equal(2, result.Count());
		Assert.All(result, c => Assert.Equal(ClaimStatus.Approved, c.Status));
		
		context.Dispose();
	}
}
