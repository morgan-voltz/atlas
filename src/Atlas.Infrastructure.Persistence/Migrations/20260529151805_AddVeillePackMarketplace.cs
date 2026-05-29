using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVeillePackMarketplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "author_user_id",
                table: "veille_pack",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "likes_count",
                table: "veille_pack",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "visibility",
                table: "veille_pack",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            // F-049 — les packs existants sont des packs système (F-042 catalogue pré-curé).
            // On les normalise explicitement, le seeder confirmera au prochain démarrage.
            migrationBuilder.Sql("UPDATE veille_pack SET visibility = 'System' WHERE visibility = '';");

            migrationBuilder.CreateTable(
                name: "veille_pack_like",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veille_pack_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veille_pack_like", x => x.id);
                    table.ForeignKey(
                        name: "FK_veille_pack_like_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_veille_pack_like_veille_pack_veille_pack_id",
                        column: x => x.veille_pack_id,
                        principalTable: "veille_pack",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "veille_pack_report",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veille_pack_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veille_pack_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_veille_pack_report_users_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_veille_pack_report_veille_pack_veille_pack_id",
                        column: x => x.veille_pack_id,
                        principalTable: "veille_pack",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_veille_pack_author_user_id",
                table: "veille_pack",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_veille_pack_visibility_is_active",
                table: "veille_pack",
                columns: new[] { "visibility", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_veille_pack_like_user_id_veille_pack_id",
                table: "veille_pack_like",
                columns: new[] { "user_id", "veille_pack_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_veille_pack_like_veille_pack_id",
                table: "veille_pack_like",
                column: "veille_pack_id");

            migrationBuilder.CreateIndex(
                name: "IX_veille_pack_report_reporter_user_id_veille_pack_id_status",
                table: "veille_pack_report",
                columns: new[] { "reporter_user_id", "veille_pack_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_veille_pack_report_veille_pack_id_status",
                table: "veille_pack_report",
                columns: new[] { "veille_pack_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "FK_veille_pack_users_author_user_id",
                table: "veille_pack",
                column: "author_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_veille_pack_users_author_user_id",
                table: "veille_pack");

            migrationBuilder.DropTable(
                name: "veille_pack_like");

            migrationBuilder.DropTable(
                name: "veille_pack_report");

            migrationBuilder.DropIndex(
                name: "IX_veille_pack_author_user_id",
                table: "veille_pack");

            migrationBuilder.DropIndex(
                name: "IX_veille_pack_visibility_is_active",
                table: "veille_pack");

            migrationBuilder.DropColumn(
                name: "author_user_id",
                table: "veille_pack");

            migrationBuilder.DropColumn(
                name: "likes_count",
                table: "veille_pack");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "veille_pack");
        }
    }
}
