using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class AggiornaTipoFermoEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aggiorna i valori dell'enum TipoFermo nel database
            // Vecchi valori: 0=WeekEnd, 1=Festivo, 2=TurnoNotturno
            // Nuovi valori: 1=TurnoNotturno, 2=WeekEnd, 3=Manutenzione, 4=Festivo
            
            // Fase 1: Sposta i valori esistenti a numeri temporanei per evitare conflitti
            migrationBuilder.Sql(@"
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 10 
                WHERE TipoFermo = 0; -- WeekEnd: 0 -> temp 10
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 11 
                WHERE TipoFermo = 1; -- Festivo: 1 -> temp 11
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 12 
                WHERE TipoFermo = 2; -- TurnoNotturno: 2 -> temp 12
            ");
            
            // Fase 2: Sposta dai valori temporanei ai nuovi valori finali
            migrationBuilder.Sql(@"
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 2 
                WHERE TipoFermo = 10; -- WeekEnd: temp 10 -> 2
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 4 
                WHERE TipoFermo = 11; -- Festivo: temp 11 -> 4
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 1 
                WHERE TipoFermo = 12; -- TurnoNotturno: temp 12 -> 1
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Ripristina i vecchi valori dell'enum
            // Nuovi valori: 1=TurnoNotturno, 2=WeekEnd, 3=Manutenzione, 4=Festivo
            // Vecchi valori: 0=WeekEnd, 1=Festivo, 2=TurnoNotturno
            
            // Fase 1: Sposta ai valori temporanei
            migrationBuilder.Sql(@"
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 10 
                WHERE TipoFermo = 1; -- TurnoNotturno: 1 -> temp 10
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 11 
                WHERE TipoFermo = 2; -- WeekEnd: 2 -> temp 11
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 12 
                WHERE TipoFermo = 4; -- Festivo: 4 -> temp 12
            ");
            
            // Fase 2: Sposta ai vecchi valori finali
            migrationBuilder.Sql(@"
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 2 
                WHERE TipoFermo = 10; -- TurnoNotturno: temp 10 -> 2
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 0 
                WHERE TipoFermo = 11; -- WeekEnd: temp 11 -> 0
                
                UPDATE CalendarioFermiCentriLavoro 
                SET TipoFermo = 1 
                WHERE TipoFermo = 12; -- Festivo: temp 12 -> 1
            ");
        }
    }
}
