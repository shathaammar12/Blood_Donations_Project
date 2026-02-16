using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blood_Donations_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Donors");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "HealthStatus",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Donors",
                type: "datetime2",
                nullable: true);
        }
    }
}
