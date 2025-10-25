using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCodiceLavorazioneToSmallInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Elimina la Foreign Key
            migrationBuilder.DropForeignKey(
                name: "FK_ListaOP_Lavorazioni_CodiceLavorazione",
                table: "ListaOP");

            // 2. Elimina la Primary Key dalla tabella Lavorazioni
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni");

            // 3. Modifica il tipo della colonna nella tabella ListaOP
            migrationBuilder.AlterColumn<short>(
                name: "CodiceLavorazione",
                table: "ListaOP",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1);

            // 4. Modifica il tipo della colonna nella tabella Lavorazioni
            migrationBuilder.AlterColumn<short>(
                name: "CodiceLavorazione",
                table: "Lavorazioni",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1);

            // 5. Ricrea la Primary Key sulla tabella Lavorazioni
            migrationBuilder.AddPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni",
                column: "CodiceLavorazione");

            // 6. Ricrea la Foreign Key
            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_Lavorazioni_CodiceLavorazione",
                table: "ListaOP",
                column: "CodiceLavorazione",
                principalTable: "Lavorazioni",
                principalColumn: "CodiceLavorazione",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Elimina la Foreign Key
            migrationBuilder.DropForeignKey(
                name: "FK_ListaOP_Lavorazioni_CodiceLavorazione",
                table: "ListaOP");

            // 2. Elimina la Primary Key dalla tabella Lavorazioni
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni");

            // 3. Ripristina il tipo della colonna nella tabella ListaOP
            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "ListaOP",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            // 4. Ripristina il tipo della colonna nella tabella Lavorazioni
            migrationBuilder.AlterColumn<string>(
                name: "CodiceLavorazione",
                table: "Lavorazioni",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            // 5. Ricrea la Primary Key sulla tabella Lavorazioni
            migrationBuilder.AddPrimaryKey(
                name: "PK_Lavorazioni",
                table: "Lavorazioni",
                column: "CodiceLavorazione");

            // 6. Ricrea la Foreign Key
            migrationBuilder.AddForeignKey(
                name: "FK_ListaOP_Lavorazioni_CodiceLavorazione",
                table: "ListaOP",
                column: "CodiceLavorazione",
                principalTable: "Lavorazioni",
                principalColumn: "CodiceLavorazione",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
