using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CMCS.Web.Models;

namespace CMCS.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	// DbSets
	public DbSet<Claim> Claims { get; set; }
	public DbSet<ClaimLine> ClaimLines { get; set; }
	public DbSet<Document> Documents { get; set; }
	public DbSet<Approval> Approvals { get; set; }
	public DbSet<Lecturer> Lecturers { get; set; }
	public new DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Configure ApplicationUser entity
		modelBuilder.Entity<ApplicationUser>(entity =>
		{
			entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
			
			// Relationship with Lecturer (optional)
			entity.HasOne(u => u.Lecturer)
				.WithMany()
				.HasForeignKey(u => u.LecturerId)
				.OnDelete(DeleteBehavior.SetNull);
		});

		// Configure Claim entity
		modelBuilder.Entity<Claim>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.HourlyRate).HasPrecision(18, 2);
			entity.Property(e => e.TotalHours).HasPrecision(18, 2);
			entity.Property(e => e.Amount).HasPrecision(18, 2);

			// Relationships
			entity.HasOne(c => c.Lecturer)
				.WithMany(l => l.Claims)
				.HasForeignKey(c => c.LecturerId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(c => c.Lines)
				.WithOne(l => l.Claim)
				.HasForeignKey(l => l.ClaimId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasMany(c => c.Documents)
				.WithOne(d => d.Claim)
				.HasForeignKey(d => d.ClaimId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasMany(c => c.Approvals)
				.WithOne(a => a.Claim)
				.HasForeignKey(a => a.ClaimId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		// Configure ClaimLine entity
		modelBuilder.Entity<ClaimLine>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Hours).HasPrecision(18, 2);
			entity.Property(e => e.ActivityDescription).HasMaxLength(500);
		});

		// Configure Document entity
		modelBuilder.Entity<Document>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.FileName).HasMaxLength(255);
			entity.Property(e => e.FilePath).HasMaxLength(1000);
		});

		// Configure Approval entity
		modelBuilder.Entity<Approval>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Decision).HasMaxLength(50);
			entity.Property(e => e.Comment).HasMaxLength(1000);
		});

		// Configure Lecturer entity
		modelBuilder.Entity<Lecturer>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
			entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
			entity.Property(e => e.PhoneNumber).HasMaxLength(50);
			entity.Property(e => e.Department).HasMaxLength(100);
			entity.Property(e => e.EmployeeNumber).HasMaxLength(50);
			entity.Property(e => e.BankName).HasMaxLength(100);
			entity.Property(e => e.AccountNumber).HasMaxLength(50);
			entity.Property(e => e.TaxNumber).HasMaxLength(50);
			entity.Property(e => e.DefaultHourlyRate).HasPrecision(18, 2);

			entity.HasIndex(e => e.Email).IsUnique();
			entity.HasIndex(e => e.EmployeeNumber).IsUnique();
		});

		// Configure User entity
		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
			entity.Property(e => e.Email).HasMaxLength(200).IsRequired();

			entity.HasIndex(e => e.Email).IsUnique();
		});

		// Seed initial data
		SeedData(modelBuilder);
	}

	private void SeedData(ModelBuilder modelBuilder)
	{
		// Seed sample lecturers
		var lecturers = new List<Lecturer>
		{
			new Lecturer
			{
				Id = 1,
				FullName = "Dr. Sarah Johnson",
				Email = "sarah.johnson@university.ac.za",
				PhoneNumber = "011-234-5678",
				Department = "Computer Science",
				EmployeeNumber = "EMP001",
				BankName = "First National Bank",
				AccountNumber = "62********89",
				TaxNumber = "9*******3",
				DefaultHourlyRate = 500m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 2,
				FullName = "Prof. Michael Chen",
				Email = "michael.chen@university.ac.za",
				PhoneNumber = "011-234-5679",
				Department = "Information Technology",
				EmployeeNumber = "EMP002",
				BankName = "Standard Bank",
				AccountNumber = "25********67",
				TaxNumber = "8*******2",
				DefaultHourlyRate = 550m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 3,
				FullName = "Dr. Thandi Nkosi",
				Email = "thandi.nkosi@university.ac.za",
				PhoneNumber = "011-234-5680",
				Department = "Software Engineering",
				EmployeeNumber = "EMP003",
				BankName = "ABSA",
				AccountNumber = "40********12",
				TaxNumber = "7*******5",
				DefaultHourlyRate = 450m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 4,
				FullName = "Prof. James Anderson",
				Email = "james.anderson@university.ac.za",
				PhoneNumber = "011-234-5681",
				Department = "Data Science",
				EmployeeNumber = "EMP004",
				BankName = "Nedbank",
				AccountNumber = "18********34",
				TaxNumber = "6*******8",
				DefaultHourlyRate = 600m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 5,
				FullName = "Dr. Lerato Molefe",
				Email = "lerato.molefe@university.ac.za",
				PhoneNumber = "011-234-5682",
				Department = "Information Systems",
				EmployeeNumber = "EMP005",
				BankName = "Capitec",
				AccountNumber = "14********56",
				TaxNumber = "5*******1",
				DefaultHourlyRate = 480m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 6,
				FullName = "Prof. David Williams",
				Email = "david.williams@university.ac.za",
				PhoneNumber = "011-234-5683",
				Department = "Computer Science",
				EmployeeNumber = "EMP006",
				BankName = "First National Bank",
				AccountNumber = "62********91",
				TaxNumber = "4*******9",
				DefaultHourlyRate = 700m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 7,
				FullName = "Dr. Nomsa Dlamini",
				Email = "nomsa.dlamini@university.ac.za",
				PhoneNumber = "011-234-5684",
				Department = "Cybersecurity",
				EmployeeNumber = "EMP007",
				BankName = "ABSA",
				AccountNumber = "40********78",
				TaxNumber = "3*******4",
				DefaultHourlyRate = 650m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 8,
				FullName = "Dr. Kevin Patel",
				Email = "kevin.patel@university.ac.za",
				PhoneNumber = "011-234-5685",
				Department = "Software Engineering",
				EmployeeNumber = "EMP008",
				BankName = "Standard Bank",
				AccountNumber = "25********45",
				TaxNumber = "2*******7",
				DefaultHourlyRate = 520m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 9,
				FullName = "Prof. Amanda Brown",
				Email = "amanda.brown@university.ac.za",
				PhoneNumber = "011-234-5686",
				Department = "Web Development",
				EmployeeNumber = "EMP009",
				BankName = "Nedbank",
				AccountNumber = "18********92",
				TaxNumber = "1*******6",
				DefaultHourlyRate = 490m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new Lecturer
			{
				Id = 10,
				FullName = "Dr. Sipho Khumalo",
				Email = "sipho.khumalo@university.ac.za",
				PhoneNumber = "011-234-5687",
				Department = "Database Management",
				EmployeeNumber = "EMP010",
				BankName = "Capitec",
				AccountNumber = "14********23",
				TaxNumber = "9*******0",
				DefaultHourlyRate = 530m,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		modelBuilder.Entity<Lecturer>().HasData(lecturers);

		// Seed sample users
		var users = new List<User>
		{
			new User { Id = 1, FullName = "Admin User", Email = "admin@university.ac.za", Role = UserRole.AcademicManager },
			new User { Id = 2, FullName = "Coordinator User", Email = "coordinator@university.ac.za", Role = UserRole.ProgrammeCoordinator }
		};

		modelBuilder.Entity<User>().HasData(users);
	}
}

