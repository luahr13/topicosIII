using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGSC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitacaoLeitura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacaoLeituras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitacaoId = table.Column<int>(type: "int", nullable: false),
                    UltimaVisualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacaoLeituras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacaoLeituras_Solicitacoes_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "Solicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoLeituras_SolicitacaoId",
                table: "SolicitacaoLeituras",
                column: "SolicitacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacaoLeituras");
        }
    }
}
