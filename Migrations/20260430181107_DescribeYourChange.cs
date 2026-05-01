using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AisVacanciesAndResumes.Migrations
{
    /// <inheritdoc />
    public partial class DescribeYourChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 6);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Software Development" },
                    { 2, "Design" },
                    { 3, "Marketing" }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Software Development", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "C#" },
                    { 2, "Software Development", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", ".NET" },
                    { 3, "Database", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "PostgreSQL" },
                    { 4, "Database", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "SQL" },
                    { 5, "Software Development", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "JavaScript" },
                    { 6, "Frontend", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "Bootstrap" }
                });
        }
    }
}
