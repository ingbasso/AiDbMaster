using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260929180000_ContabilitaMostraInProforma")]
    public class ContabilitaMostraInProforma : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'MostraInProforma') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [MostraInProforma] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_MostraInProforma] DEFAULT 1;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'MostraInProforma') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [DF_CantiereContabilitaRighe_MostraInProforma];
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [MostraInProforma];
END
");
        }
    }
}
