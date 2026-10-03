/* ================================================================
   Impostazione unica per la cartella documenti dei cantieri.
   Script IDEMPOTENTE.

   ISTRUZIONI: in SSMS selezionare il database AIDBMASTER e premere Esegui.
   ================================================================ */

IF NOT EXISTS (SELECT 1 FROM [dbo].[TabellaOpzioni] WHERE [NomeOpzione] = N'Cantieri.CartellaDocumenti')
    INSERT INTO [dbo].[TabellaOpzioni] ([NomeOpzione], [ValoreOpzione])
    VALUES (N'Cantieri.CartellaDocumenti', N'\\Svrfav\aidbmaster');
GO
