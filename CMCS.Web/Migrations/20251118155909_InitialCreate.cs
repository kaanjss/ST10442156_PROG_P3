using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CMCS.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lecturers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultHourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lecturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LecturerId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Lecturers_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Approvals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    ApproverId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Approvals_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    ActivityDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Hours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimLines_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "Id", "AccountNumber", "BankName", "CreatedAt", "DefaultHourlyRate", "Department", "Email", "EmployeeNumber", "FullName", "IsActive", "PhoneNumber", "TaxNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "62********89", "First National Bank", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9636), 500m, "Computer Science", "sarah.johnson@university.ac.za", "EMP001", "Dr. Sarah Johnson", true, "011-234-5678", "9*******3", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9637) },
                    { 2, "25********67", "Standard Bank", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9653), 550m, "Information Technology", "michael.chen@university.ac.za", "EMP002", "Prof. Michael Chen", true, "011-234-5679", "8*******2", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9654) },
                    { 3, "40********12", "ABSA", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9662), 450m, "Software Engineering", "thandi.nkosi@university.ac.za", "EMP003", "Dr. Thandi Nkosi", true, "011-234-5680", "7*******5", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9663) },
                    { 4, "18********34", "Nedbank", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9670), 600m, "Data Science", "james.anderson@university.ac.za", "EMP004", "Prof. James Anderson", true, "011-234-5681", "6*******8", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9671) },
                    { 5, "14********56", "Capitec", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9678), 480m, "Information Systems", "lerato.molefe@university.ac.za", "EMP005", "Dr. Lerato Molefe", true, "011-234-5682", "5*******1", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9678) },
                    { 6, "62********91", "First National Bank", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9692), 700m, "Computer Science", "david.williams@university.ac.za", "EMP006", "Prof. David Williams", true, "011-234-5683", "4*******9", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9693) },
                    { 7, "40********78", "ABSA", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9700), 650m, "Cybersecurity", "nomsa.dlamini@university.ac.za", "EMP007", "Dr. Nomsa Dlamini", true, "011-234-5684", "3*******4", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9701) },
                    { 8, "25********45", "Standard Bank", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9707), 520m, "Software Engineering", "kevin.patel@university.ac.za", "EMP008", "Dr. Kevin Patel", true, "011-234-5685", "2*******7", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9708) },
                    { 9, "18********92", "Nedbank", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9715), 490m, "Web Development", "amanda.brown@university.ac.za", "EMP009", "Prof. Amanda Brown", true, "011-234-5686", "1*******6", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9715) },
                    { 10, "14********23", "Capitec", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9725), 530m, "Database Management", "sipho.khumalo@university.ac.za", "EMP010", "Dr. Sipho Khumalo", true, "011-234-5687", "9*******0", new DateTime(2025, 11, 18, 15, 59, 9, 79, DateTimeKind.Utc).AddTicks(9726) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Role" },
                values: new object[,]
                {
                    { 1, "admin@university.ac.za", "Admin User", 2 },
                    { 2, "coordinator@university.ac.za", "Coordinator User", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_ClaimId",
                table: "Approvals",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimLines_ClaimId",
                table: "ClaimLines",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_LecturerId",
                table: "Claims",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClaimId",
                table: "Documents",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_Email",
                table: "Lecturers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_EmployeeNumber",
                table: "Lecturers",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Approvals");

            migrationBuilder.DropTable(
                name: "ClaimLines");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "Lecturers");
        }
    }
}
