using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedItemFavoriteMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_item_favorite_matches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    feed_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    siren = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    matched_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    matched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_item_favorite_matches", x => x.id);
                    table.ForeignKey(
                        name: "FK_feed_item_favorite_matches_feed_item_feed_item_id",
                        column: x => x.feed_item_id,
                        principalTable: "feed_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_item_favorite_matches_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_favorite_matches_feed_item_id",
                table: "feed_item_favorite_matches",
                column: "feed_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_favorite_matches_user_id_feed_item_id_siren",
                table: "feed_item_favorite_matches",
                columns: new[] { "user_id", "feed_item_id", "siren" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feed_item_favorite_matches");
        }
    }
}
