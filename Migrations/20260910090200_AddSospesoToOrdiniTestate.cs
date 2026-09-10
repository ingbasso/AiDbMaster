using AiDbMaster.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiDbMaster.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260910090200_AddSospesoToOrdiniTestate")]
    public partial class AddSospesoToOrdiniTestate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'OrdiniTestate') AND name = N'Sospeso')
BEGIN
    ALTER TABLE [OrdiniTestate] ADD [Sospeso] varchar(1) NULL;
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'OrdiniTestate') AND name = N'Sospeso')
BEGIN
    ALTER TABLE [OrdiniTestate] DROP COLUMN [Sospeso];
END;");
        }
    }
}
