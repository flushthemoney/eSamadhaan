using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSamadhaan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGrievanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "GrievanceStatusHistories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Grievances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Grievances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "GrievanceStatusHistories");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Grievances");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Grievances");
        }
    }
}
