using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260922160000_ContabilitaAcconti")]
    public class ContabilitaAcconti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CantiereContabilitaAcconti', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CantiereContabilitaAcconti] (
        [ID] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CantiereContabilitaAcconti] PRIMARY KEY,
        [ContabilitaId] int NOT NULL,
        [Ordine] int NOT NULL,
        [Descrizione] nvarchar(100) NULL,
        [Imponibile] decimal(18,2) NOT NULL,
        CONSTRAINT [FK_CantiereContabilitaAcconti_Contabilita]
            FOREIGN KEY ([ContabilitaId]) REFERENCES [dbo].[CantiereContabilita]([ID]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CantiereContabilitaAcconti_Contabilita]
        ON [dbo].[CantiereContabilitaAcconti]([ContabilitaId], [Ordine]);
END

INSERT INTO [dbo].[CantiereContabilitaAcconti] ([ContabilitaId], [Ordine], [Descrizione], [Imponibile])
SELECT [c].[ID], 1, N'Acconto', [c].[ImponibileAcconto]
FROM [dbo].[CantiereContabilita] AS [c]
WHERE [c].[ImponibileAcconto] <> 0
  AND NOT EXISTS (
        SELECT 1 FROM [dbo].[CantiereContabilitaAcconti] AS [a]
        WHERE [a].[ContabilitaId] = [c].[ID]
      );
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.CantiereContabilitaAcconti', N'U') IS NOT NULL
    DROP TABLE [dbo].[CantiereContabilitaAcconti];
");
        }
    }
}
