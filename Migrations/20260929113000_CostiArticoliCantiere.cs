using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260929113000_CostiArticoliCantiere")]
    public class CostiArticoliCantiere : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CostiArticoliCantiere', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CostiArticoliCantiere] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CostiArticoliCantiere] PRIMARY KEY,
        [CodiceArticolo] nvarchar(50) NOT NULL,
        [Descrizione] nvarchar(255) NOT NULL,
        [CostoUnitario] decimal(18,4) NOT NULL,
        [PrezzoMedioVendita] decimal(18,4) NOT NULL,
        [DataUltimaModifica] datetime NOT NULL CONSTRAINT [DF_CostiArticoliCantiere_DataUltimaModifica] DEFAULT (GETDATE()),
        [UtenteModifica] nvarchar(100) NULL
    );
    CREATE UNIQUE INDEX [IX_CostiArticoliCantiere_CodiceArticolo]
        ON [dbo].[CostiArticoliCantiere]([CodiceArticolo]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CostiArticoliCantiere', N'U') IS NOT NULL
    DROP TABLE [dbo].[CostiArticoliCantiere];
");
        }
    }
}
