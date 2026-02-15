using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blood_Donations_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalVerificationFieldsToDonor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMedicalVerified",
                table: "Donors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MedicalVerifiedBy",
                table: "Donors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MedicalVerifiedDate",
                table: "Donors",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMedicalVerified",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "MedicalVerifiedBy",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "MedicalVerifiedDate",
                table: "Donors");
        }
    }
}
