using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexBetX.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BettingAccounts_Users_UserId",
                table: "BettingAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BettingAccounts_AccountId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_BettingAccounts_Users_UserId",
                table: "BettingAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BettingAccounts_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "BettingAccounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BettingAccounts_Users_UserId",
                table: "BettingAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BettingAccounts_AccountId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_BettingAccounts_Users_UserId",
                table: "BettingAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BettingAccounts_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "BettingAccounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
