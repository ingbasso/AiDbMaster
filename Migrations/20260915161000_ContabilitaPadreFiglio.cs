using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915161000_ContabilitaPadreFiglio")]
    public class ContabilitaPadreFiglio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PadreId') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [PadreId] int NULL;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'Quantita') IS NOT NULL
    EXEC sp_rename N'dbo.CantiereContabilitaRighe.Quantita', N'QuantitaPosata', N'COLUMN';

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMateriale') IS NOT NULL
    EXEC sp_rename N'dbo.CantiereContabilitaRighe.CostoMateriale', N'CostoMaterialeServizio', N'COLUMN';

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NULL
   AND COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoCliente') IS NOT NULL
    EXEC sp_rename N'dbo.CantiereContabilitaRighe.PrezzoCliente', N'PrezzoVenditaCliente', N'COLUMN';

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'QuantitaPosata') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [QuantitaPosata] decimal(18,4) NULL;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoMaterialeServizio') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [CostoMaterialeServizio] decimal(18,4) NULL;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoVenditaCliente') IS NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] ADD [PrezzoVenditaCliente] decimal(18,4) NULL;

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoPietrisco') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [CostoPietrisco];

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoSabbia') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [CostoSabbia];

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'CostoGeotessuto') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [CostoGeotessuto];

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoPosaVendita') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [PrezzoPosaVendita];

IF COL_LENGTH(N'dbo.CantiereContabilitaRighe', N'PrezzoPosaAcquisto') IS NOT NULL
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP COLUMN [PrezzoPosaAcquisto];

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_CantiereContabilitaRighe_Padre'
      AND parent_object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
)
    ALTER TABLE [dbo].[CantiereContabilitaRighe] WITH CHECK
    ADD CONSTRAINT [FK_CantiereContabilitaRighe_Padre]
        FOREIGN KEY ([PadreId]) REFERENCES [dbo].[CantiereContabilitaRighe] ([ID]);

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_CantiereContabilitaRighe_PadreId'
      AND object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
)
    CREATE INDEX [IX_CantiereContabilitaRighe_PadreId]
        ON [dbo].[CantiereContabilitaRighe] ([PadreId]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_CantiereContabilitaRighe_Padre'
)
    ALTER TABLE [dbo].[CantiereContabilitaRighe] DROP CONSTRAINT [FK_CantiereContabilitaRighe_Padre];

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_CantiereContabilitaRighe_PadreId'
      AND object_id = OBJECT_ID(N'dbo.CantiereContabilitaRighe')
)
    DROP INDEX [IX_CantiereContabilitaRighe_PadreId] ON [dbo].[CantiereContabilitaRighe];
");
        }
    }
}
