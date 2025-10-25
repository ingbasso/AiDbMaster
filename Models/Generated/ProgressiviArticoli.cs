using System;
using System.Collections.Generic;

namespace AiDbMaster.Models.Generated;

public partial class ProgressiviArticoli
{
    public int Id { get; set; }

    /// <summary>
    /// Codice identificativo dell’articolo
    /// </summary>
    public string CodiceArticolo { get; set; } = null!;

    /// <summary>
    /// Codice identificativo del magazzino
    /// </summary>
    public short CodiceMagazzino { get; set; }

    /// <summary>
    /// Esistenza del codice articolo nel magazzino
    /// </summary>
    public decimal Esistenza { get; set; }

    /// <summary>
    /// Ordinato del codice articolo nel magazzino
    /// </summary>
    public decimal Ordinato { get; set; }

    /// <summary>
    /// Impegnato del codice articolo nel magazzino
    /// </summary>
    public decimal ImpegnatoDataOdierna { get; set; }

    /// <summary>
    /// Impegnato del codice articolo nel magazzino
    /// </summary>
    public decimal ImpegnatoTotale { get; set; }

    /// <summary>
    /// Prenotato del codice articolo nel magazzino
    /// </summary>
    public decimal Prenotato { get; set; }

    public DateTime UltimoAggiornamento { get; set; }

    public decimal OrdinatoFornitoriDataOdierna { get; set; }
}
