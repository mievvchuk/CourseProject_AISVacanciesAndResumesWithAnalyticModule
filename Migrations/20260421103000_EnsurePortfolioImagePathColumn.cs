using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AisVacanciesAndResumes.Migrations
{
    [Migration("20260421103000_EnsurePortfolioImagePathColumn")]
    public partial class EnsurePortfolioImagePathColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'PortfolioItems'
                          AND column_name = 'ImagePath'
                    ) THEN
                        ALTER TABLE "PortfolioItems"
                        ADD COLUMN "ImagePath" text NOT NULL DEFAULT '';
                    END IF;
                END
                $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'PortfolioItems'
                          AND column_name = 'ImagePath'
                    ) THEN
                        ALTER TABLE "PortfolioItems"
                        DROP COLUMN "ImagePath";
                    END IF;
                END
                $$;
                """);
        }
    }
}
