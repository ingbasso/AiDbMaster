using AiDbMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [DbContextAttribute(typeof(ApplicationDbContext))]
    [Migration("20261027130000_AddMargineFittizioToCantieri")]
    public class AddMargineFittizioToCantieri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Cantieri', N'MargineFittizio') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [MargineFittizio] decimal(6,2) NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Cantieri', N'MargineFittizio') IS NOT NULL
    ALTER TABLE [dbo].[Cantieri] DROP COLUMN [MargineFittizio];
");
        }
    }
}
