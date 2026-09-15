using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915170000_ContabilitaCampiNumericiNull")]
    public class ContabilitaCampiNumericiNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @sqlDropDf nvarchar(max);

SELECT @sqlDropDf = STRING_AGG(
    N'ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [' + dc.name + N'];',
    CHAR(10))
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
  AND c.name IN (N'QuantitaPosata', N'CostoMaterialeServizio', N'PrezzoVenditaCliente');

IF @sqlDropDf IS NOT NULL
    EXEC sp_executesql @sqlDropDf;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [QuantitaPosata] decimal(18,4) NULL;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [CostoMaterialeServizio] decimal(18,4) NULL;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ALTER COLUMN [PrezzoVenditaCliente] decimal(18,4) NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
