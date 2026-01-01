using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSamadhaan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentIdToGrievanceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "GrievanceCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceCategories_DepartmentId",
                table: "GrievanceCategories",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_GrievanceCategories_Departments_DepartmentId",
                table: "GrievanceCategories",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrievanceCategories_Departments_DepartmentId",
                table: "GrievanceCategories");

            migrationBuilder.DropIndex(
                name: "IX_GrievanceCategories_DepartmentId",
                table: "GrievanceCategories");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "GrievanceCategories");
        }
    }
}
