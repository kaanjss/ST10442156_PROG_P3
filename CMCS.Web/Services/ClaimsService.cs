using CMCS.Web.Models;
using CMCS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Web.Services;

/// <summary>
/// Claims service using Entity Framework Core with SQL Server
/// </summary>
public class ClaimsService
{
	private readonly ApplicationDbContext _context;

	public ClaimsService(ApplicationDbContext context)
	{
		_context = context;
	}

	public IEnumerable<Claim> GetAllClaims()
	{
		return _context.Claims
			.Include(c => c.Lecturer)
			.Include(c => c.Lines)
			.Include(c => c.Documents)
			.Include(c => c.Approvals)
			.OrderByDescending(c => c.Id)
			.ToList();
	}

	public Claim? GetClaimById(int id)
	{
		return _context.Claims
			.Include(c => c.Lecturer)
			.Include(c => c.Lines)
			.Include(c => c.Documents)
			.Include(c => c.Approvals)
			.FirstOrDefault(c => c.Id == id);
	}

	public IEnumerable<Claim> GetClaimsByStatus(ClaimStatus status)
	{
		return _context.Claims
			.Include(c => c.Lecturer)
			.Include(c => c.Lines)
			.Include(c => c.Documents)
			.Where(c => c.Status == status)
			.OrderByDescending(c => c.Id)
			.ToList();
	}

	public IEnumerable<Claim> GetPendingClaimsForCoordinator()
	{
		// Coordinators verify submitted claims
		return _context.Claims
			.Include(c => c.Lecturer)
			.Include(c => c.Lines)
			.Include(c => c.Documents)
			.Where(c => c.Status == ClaimStatus.Submitted)
			.OrderByDescending(c => c.Id)
			.ToList();
	}

	public IEnumerable<Claim> GetPendingClaimsForManager()
	{
		// Managers approve verified claims
		return _context.Claims
			.Include(c => c.Lecturer)
			.Include(c => c.Lines)
			.Include(c => c.Documents)
			.Where(c => c.Status == ClaimStatus.Verified)
			.OrderByDescending(c => c.Id)
			.ToList();
	}

	public Claim AddClaim(Claim claim)
	{
		_context.Claims.Add(claim);
		_context.SaveChanges();
		return claim;
	}

	public bool UpdateClaimStatus(int claimId, ClaimStatus newStatus, string? comment = null)
	{
		var claim = _context.Claims.Find(claimId);
		if (claim == null)
			return false;

		claim.Status = newStatus;
		_context.SaveChanges();
		return true;
	}

	public bool VerifyClaim(int claimId, string comment)
	{
		return UpdateClaimStatus(claimId, ClaimStatus.Verified, comment);
	}

	public bool ApproveClaim(int claimId, string comment)
	{
		return UpdateClaimStatus(claimId, ClaimStatus.Approved, comment);
	}

	public bool RejectClaim(int claimId, string comment)
	{
		return UpdateClaimStatus(claimId, ClaimStatus.Rejected, comment);
	}

	public bool AddDocumentToClaim(int claimId, Document document)
	{
		var claim = _context.Claims.Find(claimId);
		if (claim == null)
			return false;

		document.ClaimId = claimId;
		_context.Documents.Add(document);
		_context.SaveChanges();
		return true;
	}

	public bool RemoveDocumentFromClaim(int claimId, int documentId)
	{
		var document = _context.Documents.FirstOrDefault(d => d.Id == documentId && d.ClaimId == claimId);
		if (document == null)
			return false;

		_context.Documents.Remove(document);
		_context.SaveChanges();
		return true;
	}
}
