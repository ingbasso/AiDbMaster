using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailFieldsAndOpzioniSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aggiunge colonna Email a TabellaAgenti solo se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'TabellaAgenti' AND COLUMN_NAME = 'Email')
                BEGIN
                    ALTER TABLE [TabellaAgenti] ADD [Email] varchar(100) NULL;
                END
            ");

            // Aggiunge colonna Email a AnagraficaClienti solo se non esiste già
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'AnagraficaClienti' AND COLUMN_NAME = 'Email')
                BEGIN
                    ALTER TABLE [AnagraficaClienti] ADD [Email] varchar(100) NULL;
                END
            ");

            // ===== SEED DATA TabellaOpzioni =====
            // Inserisce le opzioni di configurazione per il sistema email
            // Solo se non esistono già (per evitare duplicati)

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'SmtpServer')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('SmtpServer', 'mail.favaro1.com');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'SmtpPort')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('SmtpPort', '587');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'SmtpUsername')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('SmtpUsername', 'noreply@favaro1.com');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'SmtpPassword')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('SmtpPassword', 'eWNWeCGhdu342Pb5vaTh');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'SmtpSender')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('SmtpSender', 'noreply@favaro1.com');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'GiorniEmail')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('GiorniEmail', '7');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'EmailProva')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('EmailProva', 'kathy.breda@favaro1.com;commerciale@favaro1.com');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'GiorniScadenzaMerce')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('GiorniScadenzaMerce', '21');

                IF NOT EXISTS (SELECT 1 FROM [TabellaOpzioni] WHERE [NomeOpzione] = 'ClienteEscluso')
                    INSERT INTO [TabellaOpzioni] ([NomeOpzione], [ValoreOpzione]) VALUES ('ClienteEscluso', '9060650');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Non rimuoviamo le colonne Email perché pre-esistenti nel database
            // Non rimuoviamo i dati da TabellaOpzioni per sicurezza
        }
    }
}
