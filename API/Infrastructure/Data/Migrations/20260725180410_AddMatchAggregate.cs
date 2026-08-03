using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberOneId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MemberTwoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatchedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Members_MemberOneId",
                        column: x => x.MemberOneId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Members_MemberTwoId",
                        column: x => x.MemberTwoId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MemberOneId_MemberTwoId",
                table: "Matches",
                columns: new[] { "MemberOneId", "MemberTwoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MemberTwoId",
                table: "Matches",
                column: "MemberTwoId");

            // Backfill : les paires déjà réciproques dans Likes deviennent des Matches.
            // Une seule ligne par paire (Source < Target) puisque la réciprocité produit
            // deux lignes Likes (A->B et B->A) pour un seul match.
            migrationBuilder.Sql(
                """
                INSERT INTO [Matches] ([Id], [MemberOneId], [MemberTwoId], [MatchedOn])
                SELECT NEWID(), l1.[SourceMemberId], l1.[TargetMemberId], SYSUTCDATETIME()
                FROM [Likes] l1
                INNER JOIN [Likes] l2
                    ON l1.[SourceMemberId] = l2.[TargetMemberId]
                    AND l1.[TargetMemberId] = l2.[SourceMemberId]
                WHERE l1.[SourceMemberId] < l1.[TargetMemberId];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
