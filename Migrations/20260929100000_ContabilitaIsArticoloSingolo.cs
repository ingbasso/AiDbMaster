using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260929100000_ContabilitaIsArticoloSingolo")]
    public class ContabilitaIsArticoloSingolo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsArticoloSingolo') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [IsArticoloSingolo] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_IsArticoloSingolo] DEFAULT 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsArticoloSingolo') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [DF_CantiereContabilitaRighe_IsArticoloSingolo];
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [IsArticoloSingolo];
END
");
        }
    }
}
