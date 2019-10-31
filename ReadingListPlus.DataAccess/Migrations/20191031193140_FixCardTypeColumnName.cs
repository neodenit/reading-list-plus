using Microsoft.EntityFrameworkCore.Migrations;

namespace ReadingListPlus.DataAccess.Migrations
{
    public partial class FixCardTypeColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("Type", "Cards", "CardType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("CardType", "Cards", "Type");
        }
    }
}
