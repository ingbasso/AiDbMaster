using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260922114500_ContabilitaRicarico")]
    public class ContabilitaRicarico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'RicaricoPercentuale') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe]
        ADD [RicaricoPercentuale] decimal(9,4) NULL;

UPDATE [dbo].[CantiereContabilitaRighe]
SET [RicaricoPercentuale] = ROUND(([PrezzoVenditaCliente] / [CostoMaterialeServizio] - 1) * 100, 4)
WHERE [PadreId] IS NOT NULL
  AND [IsPosa] = 0
  AND [RicaricoPercentuale] IS NULL
  AND [CostoMaterialeServizio] > 0
  AND [PrezzoVenditaCliente] IS NOT NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'RicaricoPercentuale') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [RicaricoPercentuale];
");
        }
    }
}
