using Microsoft.EntityFrameworkCore.Migrations;

namespace ReadingListPlus.DataAccess.Migrations
{
    public partial class AddCardOwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerID",
                table: "Cards",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerID",
                table: "Cards");
        }
    }
}
