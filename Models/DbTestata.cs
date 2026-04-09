using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("DB_Testata")]
    public class DbTestata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string CodiceDistinta { get; set; } = " ";

        [Required]
        [StringLength(1)]
        [Column("db_fantasma")]
        public string DbFantasma { get; set; } = "N";

        [Required]
        [Column("db_fascialivello", TypeName = "smallint")]
        public short DbFasciaLivello { get; set; }

        [Required]
        [StringLength(1)]
        [Column("db_gruppo")]
        public string DbGruppo { get; set; } = "N";

        [Required]
        [StringLength(10)]
        [Column("db_versione")]
        public string DbVersione { get; set; } = "000";

        [Required]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;
    }
}
