using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddCampiTrasportoOrdiniTestate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'AutotrenoAbbinato')
                    ALTER TABLE [OrdiniTestate] ADD [AutotrenoAbbinato] varchar(1) NOT NULL DEFAULT 'N';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'AutotrenoNoGru')
                    ALTER TABLE [OrdiniTestate] ADD [AutotrenoNoGru] varchar(1) NOT NULL DEFAULT 'N';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'Bilico')
                    ALTER TABLE [OrdiniTestate] ADD [Bilico] varchar(1) NOT NULL DEFAULT 'N';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'BilicoInAbbinamento')
                    ALTER TABLE [OrdiniTestate] ADD [BilicoInAbbinamento] varchar(1) NOT NULL DEFAULT 'N';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'MotriceInAbbinamento')
                    ALTER TABLE [OrdiniTestate] ADD [MotriceInAbbinamento] varchar(1) NOT NULL DEFAULT 'N';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'Trasporto')
                    ALTER TABLE [OrdiniTestate] ADD [Trasporto] varchar(1) NOT NULL DEFAULT 'N';
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrdiniTestate') AND name = 'TrasportoPosa')
                    ALTER TABLE [OrdiniTestate] ADD [TrasportoPosa] varchar(1) NOT NULL DEFAULT 'N';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutotrenoAbbinato",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "AutotrenoNoGru",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "Bilico",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "BilicoInAbbinamento",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "MotriceInAbbinamento",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "Trasporto",
                table: "OrdiniTestate");

            migrationBuilder.DropColumn(
                name: "TrasportoPosa",
                table: "OrdiniTestate");
        }
    }
}
