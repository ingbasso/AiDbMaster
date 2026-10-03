using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260922113000_ContabilitaIsPosa")]
    public class ContabilitaIsPosa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsPosa') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [IsPosa] bit NOT NULL CONSTRAINT [DF_CantiereContabilitaRighe_IsPosa] DEFAULT 0;

UPDATE [dbo].[CantiereContabilitaRighe]
SET [IsPosa] = 1
WHERE [PadreId] IS NOT NULL
  AND [IsPosa] = 0
  AND (
        [Descrizione] = N'Posa'
        OR [Descrizione] LIKE N'Posa %'
      );
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'IsPosa') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [DF_CantiereContabilitaRighe_IsPosa];
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [IsPosa];
END
");
        }
    }
}
