using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTabellaClassiProvvigioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella TabellaClassiProvvigioni è già stata creata manualmente nel database.
            // Questa migration serve solo per allineare il modello EF Core con il database.
            // Usiamo IF NOT EXISTS per sicurezza, così funziona sia su DB nuovi che esistenti.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TabellaClassiProvvigioni')
                BEGIN
                    CREATE TABLE [dbo].[TabellaClassiProvvigioni](
                        [ID] [int] IDENTITY(1,1) NOT NULL,
                        [CodiceClasse] [smallint] NOT NULL,
                        [DescrizioneClasse] [varchar](50) NULL,
                        [Perc_Sconto] [decimal](27, 9) NOT NULL,
                        [UltimoAggiornamento] [datetime] NOT NULL CONSTRAINT [DF_TabellaClassiProvvigioni_UltimoAggiornamento] DEFAULT (GETDATE()),
                        CONSTRAINT [PK_TabellaClassiProvvigioni] PRIMARY KEY CLUSTERED ([ID] ASC)
                    )
                END
            ");

            // Aggiunge l'indice univoco su CodiceClasse se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TabellaClassiProvvigioni_CodiceClasse' AND object_id = OBJECT_ID('TabellaClassiProvvigioni'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_TabellaClassiProvvigioni_CodiceClasse] 
                    ON [dbo].[TabellaClassiProvvigioni] ([CodiceClasse])
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TabellaClassiProvvigioni");
        }
    }
}
