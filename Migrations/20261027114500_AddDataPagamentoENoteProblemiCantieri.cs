using AiDbMaster.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    [DbContextAttribute(typeof(ApplicationDbContext))]
    [Migration("20261027114500_AddDataPagamentoENoteProblemiCantieri")]
    public class AddDataPagamentoENoteProblemiCantieri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Cantieri', N'DataPagamento') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [DataPagamento] datetime2 NULL;
IF COL_LENGTH(N'dbo.Cantieri', N'NoteProblemiCantiere') IS NULL
    ALTER TABLE [dbo].[Cantieri] ADD [NoteProblemiCantiere] nvarchar(max) NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Cantieri', N'DataPagamento') IS NOT NULL
    ALTER TABLE [dbo].[Cantieri] DROP COLUMN [DataPagamento];
IF COL_LENGTH(N'dbo.Cantieri', N'NoteProblemiCantiere') IS NOT NULL
    ALTER TABLE [dbo].[Cantieri] DROP COLUMN [NoteProblemiCantiere];
");
        }
    }
}
