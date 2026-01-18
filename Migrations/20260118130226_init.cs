using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WattRescue.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Cover = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    NumParts = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    PartNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Length = table.Column<int>(type: "INTEGER", nullable: false),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    StoryId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Paragraphs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PartId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParagraphNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Length = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    PartsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paragraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paragraphs_Parts_PartsId",
                        column: x => x.PartsId,
                        principalTable: "Parts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paragraphs_PartsId",
                table: "Paragraphs",
                column: "PartsId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_StoryId",
                table: "Parts",
                column: "StoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paragraphs");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "Stories");
        }
    }
}
