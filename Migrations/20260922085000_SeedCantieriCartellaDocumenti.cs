using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [Migration("20260922085000_SeedCantieriCartellaDocumenti")]
    public class SeedCantieriCartellaDocumenti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [dbo].[TabellaOpzioni] WHERE [NomeOpzione] = N'Cantieri.CartellaDocumenti')
    INSERT INTO [dbo].[TabellaOpzioni] ([NomeOpzione], [ValoreOpzione])
    VALUES (N'Cantieri.CartellaDocumenti', N'\\Svrfav\aidbmaster');
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [dbo].[TabellaOpzioni] WHERE [NomeOpzione] = N'Cantieri.CartellaDocumenti';
");
        }
    }
}
