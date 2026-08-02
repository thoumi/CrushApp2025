using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptsAndPromptAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PromptId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptAnswers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromptAnswers_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Prompts",
                columns: new[] { "Id", "Text" },
                values: new object[,]
                {
                    { 1, "Un talent inutile mais fascinant que je possède…" },
                    { 2, "Mon dimanche parfait ressemble à…" },
                    { 3, "Je suis convaincu·e que…" },
                    { 4, "La dernière chose qui m'a fait rire aux éclats…" },
                    { 5, "Je cherche quelqu'un qui…" },
                    { 6, "Mon pire talent culinaire…" },
                    { 7, "Un sujet sur lequel je peux parler pendant des heures…" },
                    { 8, "La prochaine destination sur ma liste…" },
                    { 9, "Ma bande-son du moment…" },
                    { 10, "Une habitude bizarre que j'assume totalement…" },
                    { 11, "Le meilleur conseil qu'on m'ait donné…" },
                    { 12, "Je suis plutôt du genre à…" },
                    { 13, "Ce qui compte le plus pour moi dans une relation…" },
                    { 14, "Un fait à mon sujet qui surprend toujours…" },
                    { 15, "Le film ou le livre que je peux revoir sans jamais me lasser…" },
                    { 16, "La question que j'aimerais qu'on me pose plus souvent…" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptAnswers_MemberId_PromptId",
                table: "PromptAnswers",
                columns: new[] { "MemberId", "PromptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptAnswers_PromptId",
                table: "PromptAnswers",
                column: "PromptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptAnswers");

            migrationBuilder.DropTable(
                name: "Prompts");
        }
    }
}
