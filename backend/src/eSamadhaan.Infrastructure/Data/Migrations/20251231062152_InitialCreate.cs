using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSamadhaan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrievanceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrievanceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Grievances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrievanceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CitizenId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grievances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grievances_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grievances_GrievanceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "GrievanceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grievances_Users_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrievanceId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Grievances_GrievanceId",
                        column: x => x.GrievanceId,
                        principalTable: "Grievances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrievanceAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrievanceId = table.Column<int>(type: "int", nullable: false),
                    OfficerId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrievanceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrievanceAssignments_Grievances_GrievanceId",
                        column: x => x.GrievanceId,
                        principalTable: "Grievances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrievanceAssignments_Users_OfficerId",
                        column: x => x.OfficerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrievanceResolutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrievanceId = table.Column<int>(type: "int", nullable: false),
                    ResolvedByOfficerId = table.Column<int>(type: "int", nullable: false),
                    ResolutionRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrievanceResolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrievanceResolutions_Grievances_GrievanceId",
                        column: x => x.GrievanceId,
                        principalTable: "Grievances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrievanceResolutions_Users_ResolvedByOfficerId",
                        column: x => x.ResolvedByOfficerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrievanceStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrievanceId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrievanceStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrievanceStatusHistories_Grievances_GrievanceId",
                        column: x => x.GrievanceId,
                        principalTable: "Grievances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrievanceStatusHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_GrievanceId",
                table: "Feedbacks",
                column: "GrievanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_Rating",
                table: "Feedbacks",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_SubmittedAt",
                table: "Feedbacks",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceAssignments_GrievanceId",
                table: "GrievanceAssignments",
                column: "GrievanceId");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceAssignments_GrievanceId_IsActive",
                table: "GrievanceAssignments",
                columns: new[] { "GrievanceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceAssignments_OfficerId",
                table: "GrievanceAssignments",
                column: "OfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceAssignments_OfficerId_IsActive",
                table: "GrievanceAssignments",
                columns: new[] { "OfficerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceCategories_Name",
                table: "GrievanceCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceResolutions_GrievanceId",
                table: "GrievanceResolutions",
                column: "GrievanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceResolutions_ResolvedAt",
                table: "GrievanceResolutions",
                column: "ResolvedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceResolutions_ResolvedByOfficerId",
                table: "GrievanceResolutions",
                column: "ResolvedByOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_CategoryId",
                table: "Grievances",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_CitizenId",
                table: "Grievances",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_CreatedAt",
                table: "Grievances",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_CurrentStatus",
                table: "Grievances",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_DepartmentId",
                table: "Grievances",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_DepartmentId_CurrentStatus",
                table: "Grievances",
                columns: new[] { "DepartmentId", "CurrentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Grievances_GrievanceNumber",
                table: "Grievances",
                column: "GrievanceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceStatusHistories_ChangedAt",
                table: "GrievanceStatusHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceStatusHistories_ChangedByUserId",
                table: "GrievanceStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceStatusHistories_GrievanceId",
                table: "GrievanceStatusHistories",
                column: "GrievanceId");

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceStatusHistories_GrievanceId_ChangedAt",
                table: "GrievanceStatusHistories",
                columns: new[] { "GrievanceId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "GrievanceAssignments");

            migrationBuilder.DropTable(
                name: "GrievanceResolutions");

            migrationBuilder.DropTable(
                name: "GrievanceStatusHistories");

            migrationBuilder.DropTable(
                name: "Grievances");

            migrationBuilder.DropTable(
                name: "GrievanceCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
