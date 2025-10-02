using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGSC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToSolicitacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Solicitacoes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitacoes_UserId",
                table: "Solicitacoes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitacoes_AspNetUsers_UserId",
                table: "Solicitacoes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitacoes_AspNetUsers_UserId",
                table: "Solicitacoes");

            migrationBuilder.DropIndex(
                name: "IX_Solicitacoes_UserId",
                table: "Solicitacoes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Solicitacoes");
        }
    }
}
