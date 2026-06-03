#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ForktierMail.Database.Migrations.Postgres;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Forks",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>("text", nullable: false, defaultValue: ""),
                ApiKey = table.Column<string>("text", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Forks", x => x.Id); });

        migrationBuilder.CreateTable(
            "Players",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Players", x => x.Id); });

        migrationBuilder.CreateTable(
            "Characters",
            table => new
            {
                ForkId = table.Column<int>("integer", nullable: false),
                CharacterId = table.Column<int>("integer", nullable: false),
                Id = table.Column<int>("integer", nullable: false),
                PlayerId = table.Column<Guid>("uuid", nullable: false),
                Name = table.Column<string>("text", nullable: false, defaultValue: ""),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Characters", x => new { x.ForkId, x.CharacterId });
                table.ForeignKey(
                    "FK_Characters_Forks_ForkId",
                    x => x.ForkId,
                    "Forks",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_Characters_Players_PlayerId",
                    x => x.PlayerId,
                    "Players",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Mails",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Type = table.Column<int>("integer", nullable: false, defaultValue: 1),
                Content = table.Column<string>("text", nullable: false, defaultValue: ""),
                SenderForkId = table.Column<int>("integer", nullable: false),
                SenderId = table.Column<int>("integer", nullable: false),
                RecipientForkId = table.Column<int>("integer", nullable: false),
                RecipientId = table.Column<int>("integer", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Mails", x => x.Id);
                table.ForeignKey(
                    "FK_Mails_Characters_RecipientForkId_RecipientId",
                    x => new { x.RecipientForkId, x.RecipientId },
                    "Characters",
                    new[] { "ForkId", "CharacterId" },
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_Mails_Characters_SenderForkId_SenderId",
                    x => new { x.SenderForkId, x.SenderId },
                    "Characters",
                    new[] { "ForkId", "CharacterId" },
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_Characters_ForkId_CharacterId",
            "Characters",
            new[] { "ForkId", "CharacterId" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Characters_PlayerId",
            "Characters",
            "PlayerId");

        migrationBuilder.CreateIndex(
            "IX_Forks_ApiKey",
            "Forks",
            "ApiKey",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Forks_Name",
            "Forks",
            "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Mails_RecipientForkId_RecipientId",
            "Mails",
            new[] { "RecipientForkId", "RecipientId" });

        migrationBuilder.CreateIndex(
            "IX_Mails_SenderForkId_SenderId",
            "Mails",
            new[] { "SenderForkId", "SenderId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Mails");

        migrationBuilder.DropTable(
            "Characters");

        migrationBuilder.DropTable(
            "Forks");

        migrationBuilder.DropTable(
            "Players");
    }
}