using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AisVacanciesAndResumes.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateApplicationsPerVacancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_ResumeId_VacancyId_CandidateUserId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_VacancyId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ResumeId",
                table: "Applications",
                column: "ResumeId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_VacancyId_CandidateUserId",
                table: "Applications",
                columns: new[] { "VacancyId", "CandidateUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_ResumeId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_VacancyId_CandidateUserId",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ResumeId_VacancyId_CandidateUserId",
                table: "Applications",
                columns: new[] { "ResumeId", "VacancyId", "CandidateUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_VacancyId",
                table: "Applications",
                column: "VacancyId");
        }
    }
}
