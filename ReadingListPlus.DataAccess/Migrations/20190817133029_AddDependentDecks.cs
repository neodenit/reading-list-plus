using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReadingListPlus.DataAccess.Migrations
{
    public partial class AddDependentDecks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DependentDeckID",
                table: "Decks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decks_DependentDeckID",
                table: "Decks",
                column: "DependentDeckID");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Decks_DependentDeckID",
                table: "Decks",
                column: "DependentDeckID",
                principalTable: "Decks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Decks_DependentDeckID",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_DependentDeckID",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "DependentDeckID",
                table: "Decks");
        }
    }
}
