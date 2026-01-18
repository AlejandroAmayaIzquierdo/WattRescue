using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WattRescue.Migrations
{
    /// <inheritdoc />
    public partial class lastScrapedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paragraphs_Parts_PartsId",
                table: "Paragraphs");

            migrationBuilder.DropIndex(
                name: "IX_Paragraphs_PartsId",
                table: "Paragraphs");

            migrationBuilder.DropColumn(
                name: "PartsId",
                table: "Paragraphs");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScrapedDate",
                table: "Stories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScrapedDate",
                table: "Parts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paragraphs_PartId",
                table: "Paragraphs",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Paragraphs_Parts_PartId",
                table: "Paragraphs",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Paragraphs_Parts_PartId",
                table: "Paragraphs");

            migrationBuilder.DropIndex(
                name: "IX_Paragraphs_PartId",
                table: "Paragraphs");

            migrationBuilder.DropColumn(
                name: "LastScrapedDate",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "LastScrapedDate",
                table: "Parts");

            migrationBuilder.AddColumn<int>(
                name: "PartsId",
                table: "Paragraphs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paragraphs_PartsId",
                table: "Paragraphs",
                column: "PartsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Paragraphs_Parts_PartsId",
                table: "Paragraphs",
                column: "PartsId",
                principalTable: "Parts",
                principalColumn: "Id");
        }
    }
}
