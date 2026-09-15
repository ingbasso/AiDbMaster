using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260915114500_AddDaConfermareToViaggiConsegna")]
    public class AddDaConfermareToViaggiConsegna : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: in produzione la colonna può già esistere (SQL manuale).
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ViaggiConsegna', N'DaConfermare') IS NULL
BEGIN
    ALTER TABLE [ViaggiConsegna]
    ADD [DaConfermare] bit NOT NULL
        CONSTRAINT [DF_ViaggiConsegna_DaConfermare] DEFAULT 0;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaConfermare",
                table: "ViaggiConsegna");
        }
    }
}
