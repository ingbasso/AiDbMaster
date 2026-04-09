using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    public partial class RenameMezziTrasportoToInterni_AddGruTrasbordo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ViaggiConsegna_MezziTrasporto_MezzoTrasportoId",
                table: "ViaggiConsegna");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MezziTrasporto",
                table: "MezziTrasporto");

            migrationBuilder.RenameTable(
                name: "MezziTrasporto",
                newName: "MezziTrasportoInterni");

            migrationBuilder.AddColumn<bool>(
                name: "Gru",
                table: "MezziTrasportoInterni",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Trasbordo",
                table: "MezziTrasportoInterni",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MezziTrasportoInterni",
                table: "MezziTrasportoInterni",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ViaggiConsegna_MezziTrasportoInterni_MezzoTrasportoId",
                table: "ViaggiConsegna",
                column: "MezzoTrasportoId",
                principalTable: "MezziTrasportoInterni",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ViaggiConsegna_MezziTrasportoInterni_MezzoTrasportoId",
                table: "ViaggiConsegna");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MezziTrasportoInterni",
                table: "MezziTrasportoInterni");

            migrationBuilder.DropColumn(
                name: "Gru",
                table: "MezziTrasportoInterni");

            migrationBuilder.DropColumn(
                name: "Trasbordo",
                table: "MezziTrasportoInterni");

            migrationBuilder.RenameTable(
                name: "MezziTrasportoInterni",
                newName: "MezziTrasporto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MezziTrasporto",
                table: "MezziTrasporto",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ViaggiConsegna_MezziTrasporto_MezzoTrasportoId",
                table: "ViaggiConsegna",
                column: "MezzoTrasportoId",
                principalTable: "MezziTrasporto",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
