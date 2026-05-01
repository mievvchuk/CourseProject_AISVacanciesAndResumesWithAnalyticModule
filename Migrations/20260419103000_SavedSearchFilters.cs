using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AisVacanciesAndResumes.Migrations
{
    public partial class SavedSearchFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "SavedSearches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                table: "SavedSearches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceLevel",
                table: "SavedSearches",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "SavedSearches");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "SavedSearches");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "SavedSearches");
        }
    }
}
