using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTabellaInvioEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== COLONNA Porto su OrdiniTestate =====
            // La colonna esiste già nel database, ma EF non la conosceva.
            // Aggiungiamo solo se non esiste per sicurezza.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'OrdiniTestate' AND COLUMN_NAME = 'Porto'
                )
                BEGIN
                    ALTER TABLE [OrdiniTestate] ADD [Porto] smallint NULL;
                END
            ");

            // ===== TABELLA InvioEmail =====
            // Crea la tabella solo se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'InvioEmail')
                BEGIN
                    CREATE TABLE [InvioEmail] (
                        [ID] int IDENTITY(1,1) NOT NULL,
                        [TipoOrdine] varchar(1) NOT NULL,
                        [AnnoOrdine] smallint NOT NULL,
                        [SerieOrdine] varchar(3) NOT NULL,
                        [NumeroOrdine] int NOT NULL,
                        [RigaOrdine] int NOT NULL,
                        [DataInvio] datetime NOT NULL,
                        [Contabilizzato] varchar(1) NOT NULL CONSTRAINT [DF_InvioEmail_Contabilizzato] DEFAULT ('N'),
                        CONSTRAINT [PK_InvioEmail] PRIMARY KEY CLUSTERED ([ID] ASC)
                    );
                END
            ");

            // ===== INDICE UNIVOCO su Ordine+Riga (evita invii duplicati) =====
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_InvioEmail_OrdineRiga' AND object_id = OBJECT_ID('InvioEmail')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_InvioEmail_OrdineRiga] 
                    ON [InvioEmail] ([TipoOrdine], [AnnoOrdine], [SerieOrdine], [NumeroOrdine], [RigaOrdine]);
                END
            ");

            // ===== INDICE su DataInvio (per query temporali) =====
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_InvioEmail_DataInvio' AND object_id = OBJECT_ID('InvioEmail')
                )
                BEGIN
                    CREATE INDEX [IX_InvioEmail_DataInvio] 
                    ON [InvioEmail] ([DataInvio]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvioEmail");

            migrationBuilder.DropColumn(
                name: "Porto",
                table: "OrdiniTestate");
        }
    }
}
