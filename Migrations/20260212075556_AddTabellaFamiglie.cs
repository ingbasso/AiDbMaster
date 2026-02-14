using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTabellaFamiglie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella TabellaFamiglie è già stata creata manualmente nel database.
            // Questa migration serve solo per allineare il modello EF Core con il database.
            // Usiamo IF NOT EXISTS per sicurezza, così funziona sia su DB nuovi che esistenti.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TabellaFamiglie')
                BEGIN
                    CREATE TABLE [dbo].[TabellaFamiglie](
                        [ID] [int] IDENTITY(1,1) NOT NULL,
                        [CodiceFamiglia] [varchar](4) NOT NULL,
                        [DescrizioneFamiglia] [varchar](50) NULL,
                        [UltimoAggiornamento] [datetime] NOT NULL CONSTRAINT [DF_TabellaFamiglie_UltimoAggiornamento] DEFAULT (GETDATE()),
                        CONSTRAINT [PK_TabellaFamiglie] PRIMARY KEY CLUSTERED ([ID] ASC)
                    )
                END
            ");

            // Aggiunge l'indice univoco su CodiceFamiglia se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TabellaFamiglie_CodiceFamiglia' AND object_id = OBJECT_ID('TabellaFamiglie'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_TabellaFamiglie_CodiceFamiglia] 
                    ON [dbo].[TabellaFamiglie] ([CodiceFamiglia])
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TabellaFamiglie");
        }
    }
}
