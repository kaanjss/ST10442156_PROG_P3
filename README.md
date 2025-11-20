# Contract Monthly Claim System (CMCS)

**Student:** ST10442156  
**Module:** PROG6212  
**Assessment:** POE Part 3  
**GitHub Repository:** https://github.com/kaanjss/ST10442156_PROG_P3.git

---

## Overview

A web application for managing monthly claims for contract lecturers. Built with ASP.NET Core MVC, featuring automated validation, role-based access control, and invoice generation.

---

## Quick Start

### 1. Database Setup

```bash
# Open SQL Server Management Studio or Azure Data Studio
# Run the setup_database.sql file
# This creates CMCS_Database with all tables and sample data
```

### 2. Configure Connection

Update `CMCS.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CMCS_Database;Integrated Security=True;TrustServerCertificate=True"
}
```

### 3. Run Application

```bash
cd CMCS.Web
dotnet run
```

Navigate to: `http://localhost:5042`

---

## Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Lecturer | lecturer@university.ac.za | Lecturer123 |
| Coordinator | coordinator@university.ac.za | Coordinator123 |
| Manager | manager@university.ac.za | Manager123 |
| HR | hr@university.ac.za | HrStaff123 |

---

## Features

### Lecturer
- Submit claims with multiple activities
- Auto-calculation (Hours × Rate = Total)
- Upload supporting documents
- Track claim status

### Programme Coordinator
- Review submitted claims
- Automated validation with color-coded results
- Verify or reject with comments

### Academic Manager
- Review verified claims
- Approve or reject with comments
- View validation history

### HR
- Process approved claims
- Generate invoices (auto tax calculation)
- Monthly reports
- Manage lecturer information

---

## Automation Features (POE Requirements)

✅ **Auto-Calculation:** Real-time calculation as lecturer types  
✅ **Auto-Validation:** Checks rates, hours, amounts, documents  
✅ **Auto-Invoices:** One-click generation with tax (20%)  
✅ **Auto-Reports:** Monthly statistics and financial summaries  

---

## Lecturer Feedback Addressed

### POE Part 1: "GUI requires improvement"
- Complete UI redesign with Bootstrap 5
- Modern color scheme and navigation
- Professional dashboards and forms

### POE Part 2: "Add supporting documents"
- Document upload functionality added
- Multiple file types supported (PDF, Word, Excel)
- File validation and management

### POE Part 2: "GitHub link missing"
- Repository: https://github.com/kaanjss/ST10442156_PROG_P3.git
- 13+ descriptive commits

---

## Technology Stack

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- ASP.NET Core Identity
- SQL Server
- Bootstrap 5
- jQuery

---

## Validation Rules

- **Hourly Rate:** R200 - R1,000
- **Hours per Month:** Max 200
- **Hours per Activity:** Max 50
- **Claim Amount:** R500 - R150,000
- **Activities:** At least 1 required
- **Documents:** Recommended

---

## Workflow

```
Lecturer → Submit Claim
    ↓
Coordinator → Verify (Auto-validation checks)
    ↓
Manager → Approve
    ↓
HR → Generate Invoice & Process Payment
```

---

## Project Structure

```
CMCS.Web/
├── Controllers/     # MVC Controllers
├── Models/         # Data models
├── Views/          # Razor views
├── Services/       # Business logic
├── Data/           # Database context
└── wwwroot/        # Static files

CMCS.Tests/         # Unit tests
setup_database.sql  # Database setup
```

---

## Testing Instructions

1. Run `setup_database.sql` in SQL Server
2. Update connection string
3. Run `dotnet run` from CMCS.Web folder
4. Login with demo accounts
5. Test complete workflow:
   - Lecturer: Submit claim
   - Coordinator: Verify claim
   - Manager: Approve claim
   - HR: Generate invoice

---

## Version Control

- **Repository:** https://github.com/kaanjss/ST10442156_PROG_P3.git
- **Commits:** 13+ descriptive commits documenting development process
- **Branch:** main

---

## Submission Files

✅ Source code (GitHub)  
✅ Database script (setup_database.sql)  
✅ Documentation (README, summaries)  
✅ Unit tests (39 tests, all passing)  

---

## References

This project was developed using the following resources and documentation:

### **Framework & Library Documentation**
- Microsoft ASP.NET Core 8.0 Documentation - https://docs.microsoft.com/aspnet/core
- Entity Framework Core Documentation - https://docs.microsoft.com/ef/core
- ASP.NET Core Identity Documentation - https://docs.microsoft.com/aspnet/core/security/authentication/identity
- Bootstrap 5 Documentation - https://getbootstrap.com/docs/5.0
- jQuery Documentation - https://jquery.com
- xUnit Testing Framework - https://xunit.net

### **Academic Resources**
- PROG6212 Programming 2B Learning Unit Guide
- PROG6212 Lecture Notes and Materials
- IIE Module Content and Requirements

### **Development Tools**
- Visual Studio 2022 Documentation
- SQL Server Management Studio Documentation
- Git Version Control Documentation

All code implementations follow official documentation and best practices as outlined in the referenced materials.

---

## Contact

**Student:** ST10442156  
**Course:** PROG6212 - Programming 2B  
**Institution:** IIE (Emeris)  
**Year:** 2025

---

## License

MIT License - Academic coursework for PROG6212 POE Part 3

---

**End of README**
