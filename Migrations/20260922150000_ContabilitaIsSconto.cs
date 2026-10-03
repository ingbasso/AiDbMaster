using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260922150000_ContabilitaIsSconto")]
    public class ContabilitaIsSconto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsSconto') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [IsSconto] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_IsSconto] DEFAULT 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsSconto') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [DF_CantiereContabilitaRighe_IsSconto];
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [IsSconto];
END
");
        }
    }
}
