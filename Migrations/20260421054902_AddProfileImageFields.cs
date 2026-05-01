using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AisVacanciesAndResumes.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileImageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "EmployerProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "CandidateProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "EmployerProfiles");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "CandidateProfiles");
        }
    }
}
