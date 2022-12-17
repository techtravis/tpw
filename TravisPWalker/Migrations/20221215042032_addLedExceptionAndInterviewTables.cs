using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravisPWalker.Migrations
{
    public partial class addLedExceptionAndInterviewTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppException",
                columns: table => new
                {
                    ExceptionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageType = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppException", x => x.ExceptionId);
                });

            migrationBuilder.CreateTable(
                name: "InterviewPrepQuestion",
                columns: table => new
                {
                    InterviewQuestionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewPrepQuestion", x => x.InterviewQuestionId);
                    table.ForeignKey(
                        name: "FK_InterviewPrepQuestion_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InterviewPrepQuestion_AspNetUsers_LastEditById",
                        column: x => x.LastEditById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LedFeed",
                columns: table => new
                {
                    LedMessageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventYear = table.Column<short>(type: "smallint", nullable: true),
                    EventMonth = table.Column<short>(type: "smallint", nullable: true),
                    EventDay = table.Column<short>(type: "smallint", nullable: true),
                    Row = table.Column<short>(type: "smallint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ShowUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedFeed", x => x.LedMessageId);
                    table.ForeignKey(
                        name: "FK_LedFeed_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewPrepQuestion_AddedById",
                table: "InterviewPrepQuestion",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewPrepQuestion_LastEditById",
                table: "InterviewPrepQuestion",
                column: "LastEditById");

            migrationBuilder.CreateIndex(
                name: "IX_LedFeed_AddedById",
                table: "LedFeed",
                column: "AddedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppException");

            migrationBuilder.DropTable(
                name: "InterviewPrepQuestion");

            migrationBuilder.DropTable(
                name: "LedFeed");
        }
    }
}
