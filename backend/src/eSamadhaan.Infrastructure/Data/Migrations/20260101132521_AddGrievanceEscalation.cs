using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eSamadhaan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrievanceEscalation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedAt",
                table: "Grievances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalationReason",
                table: "Grievances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEscalated",
                table: "Grievances",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscalatedAt",
                table: "Grievances");

            migrationBuilder.DropColumn(
                name: "EscalationReason",
                table: "Grievances");

            migrationBuilder.DropColumn(
                name: "IsEscalated",
                table: "Grievances");
        }
    }
}
