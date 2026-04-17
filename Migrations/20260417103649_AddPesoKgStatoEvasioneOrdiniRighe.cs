using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddPesoKgStatoEvasioneOrdiniRighe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniRighe') AND name = 'PesoKg')
                    ALTER TABLE [OrdiniRighe] ADD [PesoKg] decimal(27,9) NOT NULL DEFAULT 0;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniRighe') AND name = 'StatoEvasione')
                    ALTER TABLE [OrdiniRighe] ADD [StatoEvasione] varchar(1) NOT NULL DEFAULT 'A';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PesoKg",
                table: "OrdiniRighe");

            migrationBuilder.DropColumn(
                name: "StatoEvasione",
                table: "OrdiniRighe");
        }
    }
}
