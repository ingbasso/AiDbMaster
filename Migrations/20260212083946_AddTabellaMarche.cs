using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddTabellaMarche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabella TabellaMarche è già stata creata manualmente nel database.
            // Questa migration serve solo per allineare il modello EF Core con il database.
            // Usiamo IF NOT EXISTS per sicurezza, così funziona sia su DB nuovi che esistenti.

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TabellaMarche')
                BEGIN
                    CREATE TABLE [dbo].[TabellaMarche](
                        [ID] [int] IDENTITY(1,1) NOT NULL,
                        [CodiceMarca] [smallint] NOT NULL,
                        [DescrizioneMarca] [varchar](50) NULL,
                        [UltimoAggiornamento] [datetime] NOT NULL CONSTRAINT [DF_TabellaMarche_UltimoAggiornamento] DEFAULT (GETDATE()),
                        CONSTRAINT [PK_TabellaMarche] PRIMARY KEY CLUSTERED ([ID] ASC)
                    )
                END
            ");

            // Aggiunge l'indice univoco su CodiceMarca se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TabellaMarche_CodiceMarca' AND object_id = OBJECT_ID('TabellaMarche'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_TabellaMarche_CodiceMarca] 
                    ON [dbo].[TabellaMarche] ([CodiceMarca])
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TabellaMarche");
        }
    }
}
