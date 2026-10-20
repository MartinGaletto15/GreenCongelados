using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePasswordResetTokensToUserTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordResetTokens_Users_IdUser",
                table: "PasswordResetTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordResetTokens",
                table: "PasswordResetTokens");

            migrationBuilder.RenameTable(
                name: "PasswordResetTokens",
                newName: "UserTokens");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordResetTokens_IdUser",
                table: "UserTokens",
                newName: "IX_UserTokens_IdUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens",
                column: "IdPasswordResetToken");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_IdUser",
                table: "UserTokens",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_IdUser",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                newName: "PasswordResetTokens");

            migrationBuilder.RenameIndex(
                name: "IX_UserTokens_IdUser",
                table: "PasswordResetTokens",
                newName: "IX_PasswordResetTokens_IdUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordResetTokens",
                table: "PasswordResetTokens",
                column: "IdPasswordResetToken");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetTokens_Users_IdUser",
                table: "PasswordResetTokens",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
