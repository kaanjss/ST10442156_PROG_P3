using System.ComponentModel.DataAnnotations;

namespace CMCS.Web.Models.ViewModels;

public class DocumentUploadViewModel
{
	public int ClaimId { get; set; }
	
	[Display(Name = "Supporting Documents")]
	public List<IFormFile> Files { get; set; } = new();

	public List<Document> ExistingDocuments { get; set; } = new();
}

