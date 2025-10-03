using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGSC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitacaoMensagens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacaoMensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitacaoId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacaoMensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacaoMensagens_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoMensagens_SolicitacaoId",
                table: "SolicitacaoMensagens",
                column: "SolicitacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacaoMensagens");
        }
    }
}
