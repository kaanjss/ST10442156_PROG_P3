using CMCS.Web.Services;
using CMCS.Web.Data;
using CMCS.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	// Password settings
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
	
	// Lockout settings
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = true;
	
	// User settings
	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login";
	options.LogoutPath = "/Account/Logout";
	options.AccessDeniedPath = "/Account/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromHours(24);
	options.SlidingExpiration = true;
});

// Change to Scoped services since they'll use DbContext
builder.Services.AddScoped<ClaimsService>();
builder.Services.AddScoped<LecturerService>();
builder.Services.AddScoped<ClaimValidationService>();
builder.Services.AddScoped<InvoiceService>();

var app = builder.Build();

// Ensure database is created, apply migrations, and seed roles/users
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var dbContext = services.GetRequiredService<ApplicationDbContext>();
	var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
	
	// Apply migrations
	dbContext.Database.Migrate();
	
	// Seed roles and users
	await SeedRolesAndUsers(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// HTTPS redirection disabled for local HTTP-only development
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Helper method to seed roles and users
static async Task SeedRolesAndUsers(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
	// Define the roles
	string[] roleNames = { "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };
	
	// Create roles if they don't exist
	foreach (var roleName in roleNames)
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}
	
	// Create sample users for each role
	await CreateUserIfNotExists(userManager, "lecturer@university.ac.za", "Lecturer123", "Dr. Sarah Johnson", "Lecturer", 1);
	await CreateUserIfNotExists(userManager, "coordinator@university.ac.za", "Coordinator123", "John Smith", "ProgrammeCoordinator");
	await CreateUserIfNotExists(userManager, "manager@university.ac.za", "Manager123", "Jane Williams", "AcademicManager");
	await CreateUserIfNotExists(userManager, "hr@university.ac.za", "HrStaff123", "Mary Anderson", "HR");
	
	// Create additional lecturers
	await CreateUserIfNotExists(userManager, "michael.chen@university.ac.za", "Lecturer123", "Prof. Michael Chen", "Lecturer", 2);
	await CreateUserIfNotExists(userManager, "thandi.nkosi@university.ac.za", "Lecturer123", "Dr. Thandi Nkosi", "Lecturer", 3);
}

static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string email, string password, string fullName, string role, int? lecturerId = null)
{
	var user = await userManager.FindByEmailAsync(email);
	
	if (user == null)
	{
		user = new ApplicationUser
		{
			UserName = email,
			Email = email,
			FullName = fullName,
			LecturerId = lecturerId,
			EmailConfirmed = true
		};
		
		var result = await userManager.CreateAsync(user, password);
		
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(user, role);
		}
	}
	else
	{
		// Reset password for existing user (useful for fixing password issues)
		var token = await userManager.GeneratePasswordResetTokenAsync(user);
		await userManager.ResetPasswordAsync(user, token, password);
		
		// Ensure user is in the correct role
		var roles = await userManager.GetRolesAsync(user);
		if (!roles.Contains(role))
		{
			await userManager.AddToRoleAsync(user, role);
		}
	}
}
