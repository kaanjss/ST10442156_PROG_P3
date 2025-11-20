using CMCS.Web.Models;
using CMCS.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Web.Services;

/// <summary>
/// Service for managing lecturer data using Entity Framework Core
/// </summary>
public class LecturerService
{
	private readonly ApplicationDbContext _context;

	public LecturerService(ApplicationDbContext context)
	{
		_context = context;
	}

	public IEnumerable<Lecturer> GetAllLecturers()
	{
		return _context.Lecturers
			.OrderBy(l => l.FullName)
			.ToList();
	}

	public Lecturer? GetLecturerById(int id)
	{
		return _context.Lecturers
			.Include(l => l.Claims)
			.FirstOrDefault(l => l.Id == id);
	}

	public Lecturer AddLecturer(Lecturer lecturer)
	{
		lecturer.CreatedAt = DateTime.UtcNow;
		lecturer.UpdatedAt = DateTime.UtcNow;
		_context.Lecturers.Add(lecturer);
		_context.SaveChanges();
		return lecturer;
	}

	public bool UpdateLecturer(Lecturer lecturer)
	{
		var existing = _context.Lecturers.Find(lecturer.Id);
		if (existing == null)
			return false;

		existing.FullName = lecturer.FullName;
		existing.Email = lecturer.Email;
		existing.PhoneNumber = lecturer.PhoneNumber;
		existing.Department = lecturer.Department;
		existing.EmployeeNumber = lecturer.EmployeeNumber;
		existing.BankName = lecturer.BankName;
		existing.AccountNumber = lecturer.AccountNumber;
		existing.TaxNumber = lecturer.TaxNumber;
		existing.DefaultHourlyRate = lecturer.DefaultHourlyRate;
		existing.IsActive = lecturer.IsActive;
		existing.UpdatedAt = DateTime.UtcNow;

		_context.SaveChanges();
		return true;
	}

	public bool DeleteLecturer(int id)
	{
		var lecturer = _context.Lecturers.Find(id);
		if (lecturer == null)
			return false;

		_context.Lecturers.Remove(lecturer);
		_context.SaveChanges();
		return true;
	}
}
