using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    [Migration("20260915091500_AddCostoTrasbordoToDestinazioni")]
    public class AddCostoTrasbordoToDestinazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: in produzione la colonna può già esistere (SQL manuale).
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ViaggioConsegnaDestinazioni', N'CostoTrasbordo') IS NULL
BEGIN
    ALTER TABLE [ViaggioConsegnaDestinazioni]
    ADD [CostoTrasbordo] decimal(18,2) NOT NULL
        CONSTRAINT [DF_ViaggioConsegnaDestinazioni_CostoTrasbordo] DEFAULT 0;
END

UPDATE [ViaggioConsegnaDestinazioni]
SET [CostoTrasbordo] = [PrezzoVendita]
WHERE [CostoTrasbordo] = 0 AND [PrezzoVendita] <> 0;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoTrasbordo",
                table: "ViaggioConsegnaDestinazioni");
        }
    }
}
