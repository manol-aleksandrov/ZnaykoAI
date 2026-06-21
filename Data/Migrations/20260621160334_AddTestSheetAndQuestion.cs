using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZnaykoAI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestSheetAndQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestSheets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SubTopic = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSheets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TestSheetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_TestSheets_TestSheetId",
                        column: x => x.TestSheetId,
                        principalTable: "TestSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TestSheetId",
                table: "Questions",
                column: "TestSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSheets_UserId",
                table: "TestSheets",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TestSheets");
        }
    }
}
