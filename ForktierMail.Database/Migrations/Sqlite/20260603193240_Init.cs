using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForktierMail.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Forks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ForkId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.UniqueConstraint("AK_Characters_ForkId_CharacterId", x => new { x.ForkId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_Characters_Forks_ForkId",
                        column: x => x.ForkId,
                        principalTable: "Forks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    Content = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    SenderForkId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipientForkId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipientId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mails_Characters_RecipientForkId_RecipientId",
                        columns: x => new { x.RecipientForkId, x.RecipientId },
                        principalTable: "Characters",
                        principalColumns: new[] { "ForkId", "CharacterId" });
                    table.ForeignKey(
                        name: "FK_Mails_Characters_SenderForkId_SenderId",
                        columns: x => new { x.SenderForkId, x.SenderId },
                        principalTable: "Characters",
                        principalColumns: new[] { "ForkId", "CharacterId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ForkId_CharacterId",
                table: "Characters",
                columns: new[] { "ForkId", "CharacterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_PlayerId",
                table: "Characters",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Forks_ApiKey",
                table: "Forks",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forks_Name",
                table: "Forks",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mails_RecipientForkId_RecipientId",
                table: "Mails",
                columns: new[] { "RecipientForkId", "RecipientId" });

            migrationBuilder.CreateIndex(
                name: "IX_Mails_SenderForkId_SenderId",
                table: "Mails",
                columns: new[] { "SenderForkId", "SenderId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mails");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Forks");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
